using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Portable Executable Demo")]
[assembly: AssemblyDescription("Demonstration application for Portable Executable file reader project")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyProduct("Demo")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("bf1dbc74-d376-4b74-94cf-94b09e4b804b")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: System.CLSCompliant(false)]