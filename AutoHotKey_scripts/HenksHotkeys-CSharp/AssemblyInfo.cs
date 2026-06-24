using System.Runtime.Versioning;

// The project targets net9.0-windows and uses Windows-only APIs (WinForms tray /
// clipboard / screen, registry, DWM). With GenerateAssemblyInfo disabled the SDK
// no longer emits this automatically, so declare it here to silence CA1416.
[assembly: SupportedOSPlatform( "windows" )]
