using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

// The project targets net9.0-windows and uses Windows-only APIs (Win32 interop,
// the registry, DWM, Shell_NotifyIcon). With GenerateAssemblyInfo disabled the SDK
// no longer emits this automatically, so declare it here to silence CA1416.
[assembly: SupportedOSPlatform( "windows" )]

// Expose internal types (HotkeyParser, TextSender, TabModel, …) to the test project.
[assembly: InternalsVisibleTo( "HenksHotkeys.Tests" )]
