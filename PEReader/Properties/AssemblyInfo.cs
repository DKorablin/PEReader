using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("ee008adf-5990-4e3e-83a3-b8f1cebc4957")]
#if NETSTANDARD
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/DKorablin/PEReader")]
#endif
[assembly: System.CLSCompliant(false)]

#if !NETSTANDARD
[assembly: AssemblyCompany("Danila Korablin")]
[assembly: AssemblyCopyright("Copyright © Danila Korablin 2012-2023")]
[assembly: AssemblyProduct("Portable Executable Reader")]
[assembly: AssemblyTitle("PE Reader")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
#endif