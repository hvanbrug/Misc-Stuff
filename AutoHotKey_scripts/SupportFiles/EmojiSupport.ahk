; EmojiSupport.ahk
; GDI+ helpers for rendering Twemoji PNG images on emoji buttons.
;
; Image sources (tried in order):
;   - Compiled .exe : RT_RCDATA resources embedded by Ahk2Exe
;                     (see EmojiResources.ahk and GenerateEmojiResources.ps1)
;   - Debug script  : Images\Twemoji\{codepoint}.png  on the filesystem
;
; If neither source has the image, the button keeps its emoji-character text
; label as a fallback (no error, no crash).
;
; Twemoji PNG download:
;   https://github.com/jdecked/twemoji  ›  assets/72x72/
;   or via CDN:  https://cdn.jsdelivr.net/gh/jdecked/twemoji@15/assets/72x72/
;   Place files in:  Images\Twemoji\{codepoint}.png   e.g. 1f600.png

; ── GDI+ state ──────────────────────────────────────────────────────────────
g_gdipToken    := 0
g_emojiBitmaps := []   ; accumulated HBITMAPs — kept alive while buttons exist

; ── Initialisation / shutdown ────────────────────────────────────────────────

EmojiSupport_Init()
{
  global g_gdipToken

  DllCall( "LoadLibraryW", "Str", "gdiplus.dll", "Ptr" )

  ; GdiplusStartupInput: version(4) + padding(4) + callback ptr(8) + 2×BOOL(8) = 24 bytes
  startupInput := Buffer( 24, 0 )
  NumPut( "UInt", 1, startupInput, 0 )   ; GdiplusVersion = 1

  DllCall( "gdiplus\GdiplusStartup", "UPtr*", &g_gdipToken, "Ptr", startupInput, "Ptr", 0 )

  OnExit( (*) => EmojiSupport_Shutdown() )
}

EmojiSupport_Shutdown()
{
  global g_gdipToken
  global g_emojiBitmaps

  for hBmp in g_emojiBitmaps
  {
    DllCall( "DeleteObject", "Ptr", hBmp )
  }
  g_emojiBitmaps := []

  if( g_gdipToken )
  {
    DllCall( "gdiplus\GdiplusShutdown", "UPtr", g_gdipToken )
    g_gdipToken := 0
  }
}

; ── Codepoint helpers ────────────────────────────────────────────────────────

; Convert an emoji character (may be a surrogate-pair sequence) to its Twemoji
; PNG filename stem.  e.g.  "😀" → "1f600",  "👨‍👩‍👧" → "1f468-200d-1f469-200d-1f467"
; Variation selector U+FE0F is stripped; ZWJ U+200D is kept.
EmojiToTwemojiFilename( char )
{
  parts := []
  i     := 1
  len   := StrLen( char )

  while( i <= len )
  {
    cu := Ord( SubStr( char, i, 1 ) )

    ; Combine UTF-16 surrogate pair into the real codepoint.
    if( cu >= 0xD800 && cu <= 0xDBFF && i < len )
    {
      low := Ord( SubStr( char, i + 1, 1 ) )
      if( low >= 0xDC00 && low <= 0xDFFF )
      {
        cu := 0x10000 + (cu - 0xD800) * 0x400 + (low - 0xDC00)
        ++i
      }
    }

    if( cu != 0xFE0F )   ; strip variation selector-16
    {
      parts.Push( Format( "{:x}", cu ) )
    }

    ++i
  }

  if( parts.Length = 0 )
  {
    return ""
  }

  result := parts[1]
  j      := 2
  while( j <= parts.Length )
  {
    result .= "-" parts[j]
    ++j
  }
  return result
}

; ── Image loading ────────────────────────────────────────────────────────────

; Load a PNG from disk into a GDI+ image.  Returns the GpImage* or 0.
_EmojiLoadGdipImageFromFile( path )
{
  pImage := 0
  if( DllCall( "gdiplus\GdipLoadImageFromFile", "WStr", path, "Ptr*", &pImage ) != 0 )
  {
    return 0
  }
  return pImage
}

; Load a PNG from an RT_RCDATA exe resource into a GDI+ image.  Returns the GpImage* or 0.
; The resource must have been embedded via @Ahk2Exe-AddResource with resName as its name.
_EmojiLoadGdipImageFromResource( resName )
{
  hMod     := DllCall( "GetModuleHandleW", "Ptr",  0,       "Ptr" )
  hResInfo := DllCall( "FindResourceW",    "Ptr",  hMod,
                                           "WStr", resName,
                                           "Ptr",  10,       "Ptr" )   ; RT_RCDATA = 10
  if( !hResInfo )
  {
    return 0
  }

  dataSize := DllCall( "SizeofResource", "Ptr", hMod,     "Ptr", hResInfo, "UInt" )
  hResData := DllCall( "LoadResource",   "Ptr", hMod,     "Ptr", hResInfo, "Ptr"  )
  pData    := DllCall( "LockResource",   "Ptr", hResData,                  "Ptr"  )

  ; Copy resource bytes into a moveable heap block that IStream can own.
  hMem := DllCall( "GlobalAlloc", "UInt", 0x0002, "UPtr", dataSize, "Ptr" )  ; GMEM_MOVEABLE
  pMem := DllCall( "GlobalLock",  "Ptr",  hMem,                     "Ptr" )
  DllCall( "RtlMoveMemory", "Ptr", pMem, "Ptr", pData, "UPtr", dataSize )
  DllCall( "GlobalUnlock",  "Ptr", hMem )

  ; Wrap in an IStream (fDeleteOnRelease=TRUE so the HGLOBAL is freed with the stream).
  pStream := 0
  DllCall( "ole32\CreateStreamOnHGlobal", "Ptr", hMem, "Int", 1, "Ptr*", &pStream )

  pImage := 0
  DllCall( "gdiplus\GdipLoadImageFromStream", "Ptr", pStream, "Ptr*", &pImage )

  ObjRelease( pStream )
  return pImage
}

; ── Bitmap compositing ───────────────────────────────────────────────────────

; Scale pImage to w×h pixels, composite onto the system button-face colour,
; and return an HBITMAP.  Returns 0 on failure.
_EmojiGdipImageToBitmap( pImage, w, h )
{
  ; Create a 32bpp ARGB GDI+ bitmap at the target size.
  pScaled := 0
  if( DllCall( "gdiplus\GdipCreateBitmapFromScan0",
               "Int",  w, "Int", h,
               "Int",  0, "Int", 0x26200A,   ; PixelFormat32bppARGB
               "Ptr",  0, "Ptr*", &pScaled ) != 0 )
  {
    return 0
  }

  pGraphics := 0
  DllCall( "gdiplus\GdipGetImageGraphicsContext", "Ptr", pScaled, "Ptr*", &pGraphics )
  DllCall( "gdiplus\GdipSetInterpolationMode",    "Ptr", pGraphics, "Int", 7 )  ; HighQualityBicubic

  ; Fill the background with the system button-face colour so alpha blends cleanly.
  btnColor := DllCall( "GetSysColor", "Int", 15, "UInt" )   ; COLOR_BTNFACE = 15
  r        := ( btnColor        & 0xFF )
  g        := ( (btnColor >> 8) & 0xFF )
  b        := ( (btnColor >> 16) & 0xFF )
  argb     := 0xFF000000 | (r << 16) | (g << 8) | b
  pBrush   := 0
  DllCall( "gdiplus\GdipCreateSolidFill",  "UInt", argb, "Ptr*", &pBrush )
  DllCall( "gdiplus\GdipFillRectangleI",   "Ptr",  pGraphics, "Ptr", pBrush,
                                           "Int",  0, "Int", 0, "Int", w, "Int", h )
  DllCall( "gdiplus\GdipDeleteBrush",      "Ptr",  pBrush )

  ; Draw the emoji scaled to fill the bitmap.
  DllCall( "gdiplus\GdipDrawImageRectI", "Ptr", pGraphics, "Ptr", pImage,
                                         "Int", 0, "Int", 0, "Int", w, "Int", h )

  hBitmap := 0
  DllCall( "gdiplus\GdipCreateHBITMAPFromBitmap", "Ptr", pScaled, "Ptr*", &hBitmap, "UInt", 0 )

  DllCall( "gdiplus\GdipDeleteGraphics", "Ptr", pGraphics )
  DllCall( "gdiplus\GdipDisposeImage",   "Ptr", pScaled )

  return hBitmap
}

; ── Public entry point ───────────────────────────────────────────────────────

; Apply a Twemoji PNG image to a button control at the given physical pixel size.
;
; Source priority:
;   A_IsCompiled = true  →  RT_RCDATA resource named after the codepoint stem
;   A_IsCompiled = false →  Images\Twemoji\{stem}.png on the filesystem
;
; On any failure the function returns silently; the button retains its existing
; emoji-character text label as a fallback.
ApplyEmojiBitmapToButton( btn, char, pixelSize )
{
  global g_emojiBitmaps

  filename := EmojiToTwemojiFilename( char )
  if( filename = "" )
  {
    return
  }

  pImage := 0
  if( A_IsCompiled )
  {
    pImage := _EmojiLoadGdipImageFromResource( filename )
  }
  else
  {
    filePath := A_ScriptDir "\Images\Twemoji\" filename ".png"
    pImage   := _EmojiLoadGdipImageFromFile( filePath )
  }

  if( !pImage )
  {
    return   ; Fallback: leave button text (emoji character) unchanged.
  }

  hBitmap := _EmojiGdipImageToBitmap( pImage, pixelSize, pixelSize )
  DllCall( "gdiplus\GdipDisposeImage", "Ptr", pImage )

  if( !hBitmap )
  {
    return   ; Fallback: leave button text unchanged.
  }

  g_emojiBitmaps.Push( hBitmap )   ; keep alive for the button's lifetime

  ; Switch the button to bitmap-display mode and assign the image.
  GWL_STYLE := -16
  style     := DllCall( "GetWindowLong", "Ptr", btn.Hwnd, "Int", GWL_STYLE, "Int" )
  DllCall( "SetWindowLong", "Ptr", btn.Hwnd, "Int", GWL_STYLE, "Int", style | 0x80 )  ; BS_BITMAP
  btn.Text := ""
  SendMessage( 0x00F7, 0, hBitmap, btn.Hwnd )   ; BM_SETIMAGE, IMAGE_BITMAP = 0
}

; ── Auto-initialise on #Include ──────────────────────────────────────────────
EmojiSupport_Init()
