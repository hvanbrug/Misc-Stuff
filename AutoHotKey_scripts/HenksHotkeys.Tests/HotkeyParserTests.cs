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
}
