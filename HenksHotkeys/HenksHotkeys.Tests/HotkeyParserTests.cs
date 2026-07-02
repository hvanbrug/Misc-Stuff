using HenksHotkeys.Core;
using HenksHotkeys.Native;
using Xunit;

namespace HenksHotkeys.Tests;

public class HotkeyParserTests
{
  [Fact]
  public void Parse_CtrlShiftLetter()
  {
    HotkeyParser.Parsed? p = HotkeyParser.Parse( "^+a" );
    Assert.NotNull( p );
    Assert.Equal( NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, p!.Value.Modifiers );
    Assert.Equal( (uint)0x41, p.Value.VirtualKey ); // 'A'
  }

  [Fact]
  public void Parse_AltShiftDigit()
  {
    HotkeyParser.Parsed? p = HotkeyParser.Parse( "!+1" );
    Assert.NotNull( p );
    Assert.Equal( NativeMethods.MOD_ALT | NativeMethods.MOD_SHIFT, p!.Value.Modifiers );
    Assert.Equal( (uint)0x31, p.Value.VirtualKey ); // '1'
  }

  [Fact]
  public void Parse_WinAltDigit()
  {
    HotkeyParser.Parsed? p = HotkeyParser.Parse( "#!1" );
    Assert.NotNull( p );
    Assert.Equal( NativeMethods.MOD_WIN | NativeMethods.MOD_ALT, p!.Value.Modifiers );
    Assert.Equal( (uint)0x31, p.Value.VirtualKey );
  }

  [Fact]
  public void Parse_CtrlShiftWin_FunctionKey()
  {
    HotkeyParser.Parsed? p = HotkeyParser.Parse( "^+#F9" );
    Assert.NotNull( p );
    Assert.Equal( NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT | NativeMethods.MOD_WIN, p!.Value.Modifiers );
    Assert.Equal( (uint)0x78, p.Value.VirtualKey ); // VK_F9 = 0x70 + 8
  }

  [Fact]
  public void Parse_CtrlAltLetter()
  {
    HotkeyParser.Parsed? p = HotkeyParser.Parse( "^!v" );
    Assert.NotNull( p );
    Assert.Equal( NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT, p!.Value.Modifiers );
    Assert.Equal( (uint)0x56, p.Value.VirtualKey ); // 'V'
  }

  [Theory]
  [InlineData( "" )]
  [InlineData( "^+" )]       // modifiers but no key
  [InlineData( "^+F99" )]    // out-of-range function key
  public void Parse_InvalidReturnsNull( string hotkey )
  {
    Assert.Null( HotkeyParser.Parse( hotkey ) );
  }

  [Fact]
  public void Label_ExpandsModifiers()
  {
    Assert.Equal( "Ctrl-Shift-Win-F9", HotkeyParser.Label( "^+#F9" ) );
    Assert.Equal( "Ctrl-Alt-V",        HotkeyParser.Label( "^!v" ) );
    Assert.Equal( "Win-Alt-1",         HotkeyParser.Label( "#!1" ) );
  }

  [Fact]
  public void Split_SeparatesModifiersFromKey()
  {
    HotkeyParser.Split( "#!1", out bool ctrl, out bool alt, out bool win, out bool shift, out string key );
    Assert.False( ctrl );
    Assert.True( alt );
    Assert.True( win );
    Assert.False( shift );
    Assert.Equal( "1", key );
  }

  [Fact]
  public void Split_HandlesNullAndKeyOnly()
  {
    HotkeyParser.Split( null, out bool c1, out _, out _, out _, out string k1 );
    Assert.False( c1 );
    Assert.Equal( "", k1 );

    HotkeyParser.Split( "F9", out bool c2, out bool a2, out bool w2, out bool s2, out string k2 );
    Assert.False( c2 || a2 || w2 || s2 );
    Assert.Equal( "F9", k2 );
  }

  [Fact]
  public void Compose_BuildsInCtrlAltWinShiftOrder()
  {
    Assert.Equal( "^!#+A", HotkeyParser.Compose( true, true, true, true, "A" ) );
    Assert.Equal( "!#1",   HotkeyParser.Compose( false, true, true, false, "1" ) ); // Alt(!) then Win(#)
    Assert.Equal( "",      HotkeyParser.Compose( true, true, true, true, "" ) );     // no key → no hotkey
  }

  [Fact]
  public void SplitThenCompose_RoundTrips()
  {
    HotkeyParser.Split( "^+#F9", out bool c, out bool a, out bool w, out bool s, out string k );
    // Recomposed in the canonical order still parses to the same modifiers + key.
    string re = HotkeyParser.Compose( c, a, w, s, k );
    Assert.Equal( HotkeyParser.Parse( "^+#F9" )!.Value.Modifiers, HotkeyParser.Parse( re )!.Value.Modifiers );
    Assert.Equal( HotkeyParser.Parse( "^+#F9" )!.Value.VirtualKey, HotkeyParser.Parse( re )!.Value.VirtualKey );
  }
}
