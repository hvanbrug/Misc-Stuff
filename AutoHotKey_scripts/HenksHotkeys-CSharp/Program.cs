using System.Windows.Forms;

namespace HenksHotkeys;

internal static class Program
{
  private static Mutex? s_mutex;

  [STAThread]
  private static void Main()
  {
    // #SingleInstance Force — only one running copy.
    s_mutex = new Mutex( true, "HenksHotkeys_SingleInstance_2A6F", out bool createdNew );
    if( !createdNew )
    {
      return;
    }

    ApplicationConfiguration.Initialize();
    Application.Run( new HotkeyAppContext() );

    GC.KeepAlive( s_mutex );
  }
}
