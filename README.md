## Portable Executable (PE/COFF & .NET CLI) reader
[![Auto build](https://github.com/DKorablin/PEReader/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/PEReader/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/AlphaOmega.PEReader)](https://www.nuget.org/packages/AlphaOmega.PEReader)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AlphaOmega.PEReader)](https://www.nuget.org/packages/AlphaOmega.PEReader)

PE / PE+ / CLI executable low‑level reader. Targets .NET Framework 2.0 and .NET Standard 2.0 (usable from .NET 6+). No unsafe code; pure managed parsing.

---
### Installation
NuGet package:
```
dotnet add package AlphaOmega.PEReader
```
Legacy projects (.NET Framework < 4.0) can reference the assembly directly after build.

### Supported frameworks
- .NET Framework 2.0+
- .NET Standard 2.0 (covers .NET Core 2.0+, .NET 5/6/7/8, Mono, etc.)

> **.NET Core / .NET 5+ note:** some resources (RT_MANIFEST, RT_MESSAGETABLE, RT_VERSION) are decoded with the code page stored in the image (e.g. 1252). Those code pages are not available by default outside .NET Framework, so register the provider once at startup:
> ```csharp
> System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
> ```
> The `System.Text.Encoding.CodePages` dependency is already referenced by the `netstandard2.0` build of the package.

### Quick start
All public types live in the `AlphaOmega.Debug` namespace (plus `AlphaOmega.Debug.NTDirectory`, `AlphaOmega.Debug.CorDirectory` and their sub-namespaces).

```csharp
using AlphaOmega.Debug;

String filePath = @"C:\Windows\System32\kernel32.dll";
using(PEFile file = new PEFile(filePath, StreamLoader.FromFile(filePath)))
{
    if(!file.Header.IsValid)
        return; // Not a PE/PE+ image

    Console.WriteLine(file.Header.Is64Bit ? "PE32+ (64-bit)" : "PE32 (32-bit)");

    // Sections
    foreach(var section in file.Sections)
        Console.WriteLine($"{section.Header.Section} {section.Description}");

    // Exports
    if(!file.Export.IsEmpty)
        foreach(var func in file.Export.GetExportFunctions())
            Console.WriteLine(func.IsForwarded
                ? $"{func.Ordinal} {func.Name} -> {func.ForwarderName}"
                : $"{func.Ordinal} {func.Name} RVA=0x{func.Rva:X}");

    // Imports (ImportModule → IMAGE_IMPORT_BY_NAME)
    if(!file.Import.IsEmpty)
        foreach(var module in file.Import)
            foreach(var func in module)
                Console.WriteLine($"{module.ModuleName}!{(func.IsByOrdinal ? "#" + func.Hint : func.Name)}");

    // Delay imports
    if(!file.DelayImport.IsEmpty)
        foreach(var module in file.DelayImport)
            Console.WriteLine($"Delay-load: {module.ModuleName}");

    // Authenticode certificate
    if(!file.Certificate.IsEmpty)
    {
        var cert = file.Certificate.X509;
        Console.WriteLine($"{cert.Subject} ({cert.NotBefore:d} - {cert.NotAfter:d})");
    }

    // Debug directory (PDB path & signature)
    if(!file.Debug.IsEmpty && file.Debug.Pdb7CodeView.HasValue)
        Console.WriteLine($"PDB: {file.Debug.Pdb7CodeView.Value.PdbFileName}");
}
```

### Image loaders
`PEFile` reads the image through an `IImageLoader`. The loader is owned by `PEFile` and is disposed together with it.

| Loader | Use case |
|---|---|
| `StreamLoader.FromFile(path)` | Read a file from disk (cross-platform, file is opened with `FileShare.ReadWrite`). |
| `StreamLoader.FromMemory(bytes)` | Parse an image that is already in memory (downloaded, extracted from an archive, etc.). |
| `new StreamLoader(stream)` | Any seekable `Stream`. |
| `Win32Loader.FromFile(path)` | **Windows only.** Maps the image with `LoadLibraryEx(DONT_RESOLVE_DLL_REFERENCES)` — data is read from the mapped (RVA) layout. |
| `Win32Loader.FromModule(ProcessModule)` / `new Win32Loader(hModule)` | **Windows only.** Inspect a module that is already loaded into the current process. |

You can implement `IImageLoader` yourself to read from other sources (e.g. a remote process or a memory dump).

```csharp
Byte[] payload = File.ReadAllBytes(filePath);
using(PEFile file = new PEFile("in-memory", StreamLoader.FromMemory(payload)))
{
    // ...
}
```

### Reading Win32 resources
```csharp
using AlphaOmega.Debug.NTDirectory.Resources;

if(!file.Resource.IsEmpty)
{
    // Strongly typed helpers
    ResourceVersion version = file.Resource.GetVersion();
    if(version != null)
    {
        Console.WriteLine(version.FileInfo?.FileVersion);
        Console.WriteLine(version.GetFileInfo("CompanyName")?.Value);
    }

    ResourceManifest manifest = file.Resource.GetManifest();
    if(manifest != null)
        Console.WriteLine(manifest.Xml);

    foreach(ResourceString block in file.Resource.GetStrings())
        foreach(var str in block)
            Console.WriteLine($"{str.ID}: {str.Value}");

    // Raw tree: Type → Name → Language
    foreach(var type in file.Resource)
        foreach(var name in type)
            foreach(var lang in name)
                if(lang.DirectoryEntry.IsDataEntry)
                    Console.WriteLine($"{type.Name}/{name.Name}/{lang.Name}: {lang.GetData().Length} bytes");
}
```
Other helpers: `GetAccelerators()`, `GetMessageTables()`, `GetDialogs()`, `GetDialogInint()`, `GetMenus()`, `GetToolBars()`, `GetBitmaps()`, `GetIcons()`, `GetFontDirs()`, `GetFonts()` and `GetResources(RESOURCE_DIRECTORY_TYPE)`.

### Reading .NET metadata
`file.ComDescriptor` returns `null` for native (unmanaged) images.

```csharp
using System.Linq;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;

var com = file.ComDescriptor;
if(com != null)
{
    var tables = com.MetaData.StreamTables;

    // Raw table access
    foreach(var typeDef in tables.TypeDef)
        Console.WriteLine($"{typeDef.TypeNamespace}.{typeDef.TypeName}");

    // High-level readers (type → methods → arguments / IL)
    foreach(var typeDef in tables.TypeDef)
    {
        var type = new TypeReader(typeDef);
        Console.WriteLine($"{type.FullName} : {type.BaseType}");

        foreach(MethodReader method in type.GetMembers())
        {
            Console.WriteLine($"  {method.Name}({method.GetArguments().Count()} args)");

            if(method.MethodDef.RVA != 0) // Abstract / extern methods have no body
                foreach(var line in method.MethodDef.Body.GetMethodBody2())
                    Console.WriteLine($"    IL_{line.Line:X4}: {line.IL.Name}");
        }
    }

    // Embedded managed resources. Any file can be embedded; .resources files can be enumerated.
    // Values are decoded without deserialization (System.Resources.Extensions files are supported as well)
    if(com.Resources != null)
        foreach(var resource in com.Resources)
            if(resource.CanRead)
                foreach(var item in resource)
                    Console.WriteLine($"{resource.Name}: {item.Name} ({item.Type}) = {(item.IsPrimitive ? item.Value : "<serialized>")}");
            else
                Console.WriteLine($"{resource.Name}: {resource.Data.Length} bytes");
}
```
Heaps are available via `MetaData.StringHeap`, `USHeap`, `GuidHeap` and `BlobHeap`; any table can also be accessed by type with `StreamTables[Cor.MetaTableType.XXX]`.

### Reading COFF object files (.obj)
```csharp
using(ObjFile obj = new ObjFile(StreamLoader.FromFile(@"C:\build\module.obj")))
{
    if(obj.IsValid)
    {
        foreach(var section in obj.Sections)
            Console.WriteLine(obj.GetSectionName(section)); // resolves "/nnn" long names

        var symbols = obj.Symbols; // indexed by raw symbol index; aux record slots are left empty
        for(Int32 i = 0; i < symbols.Length; i += 1 + symbols[i].NumberOfAuxSymbols)
            Console.WriteLine($"{obj.GetSymbolName(symbols[i])} {symbols[i].SectionNumber} {symbols[i].StorageClass} 0x{symbols[i].Value:X}");
    }
}
```

More complete samples covering every directory and resource type are in the [Demo project](https://github.com/DKorablin/PEReader/tree/master/Demo) (see `Reader.cs`).

### Core capabilities
- Full DOS, PE and optional header parsing
- Section headers & data directory enumeration
- Import / Export / Delay‑Import tables
- Resources (Win32 resource types including VERSION, MANIFEST, STRING, MESSAGETABLE, ACCELERATOR, DIALOG, DLGINIT, MENU, TOOLBAR, BITMAP, ICON, FONT, FONTDIR, HTML)
- COFF object files (`.obj`): file header, sections, symbol & string tables
- Relocations, TLS, Load Config, Bound Import, Debug (CodeView PDB 2.0 / 7.0, Misc)
- Authenticode (WIN_CERTIFICATE + X509 extraction)
- .NET CLR header & metadata streams (#~, #Strings, #US, #Guid, #Blob)
- Rich access to MetaData tables (TypeDef, MethodDef, Signatures, CustomAttribute, Assembly*, ManifestResource, Generic*, etc.)
- IL method body reader + signature decoding

### High level object map
```
PEFile
 ├─ Header (DOS → NT → File/Optional → Sections)
 ├─ Directories
 │   ├─ Import / DelayImport / Export
 │   ├─ Resource
 │   ├─ Certificate
 │   ├─ Base Relocation / TLS / Load Config
 │   ├─ Debug
 │   └─ CLR (.NET) → MetaData → Tables & Streams
 └─ Helpers (IL reader, Signatures, Strong Name, Manifest resources)
```

### Selected supported structures
(Non‑exhaustive; full list in Wiki) DOS/NT headers, IMAGE_* directories, WIN_CERTIFICATE, ImgDelayDescr, relocation & config directories, Debug (IMAGE_DEBUG_DIRECTORY, CodeView), TLS, CLR (IMAGE_COR20_HEADER, metadata streams & tables), unmanaged resource templates (dialogs, menus, version info, bitmaps, accelerators, message tables, fonts).

### Performance & memory
Lazily loads sections and directories. Avoids reading large blobs until accessed. Suitable for scanning many binaries (e.g. signature, import/export enumeration) with minimal allocations.

### Error handling
The reader is tolerant to unusual images, but malformed or deliberately corrupted files can still throw while a specific structure is being decoded (e.g. `ArgumentOutOfRangeException` for a truncated bitmap, `IndexOutOfRangeException` / `ArgumentException` for a corrupt dialog template). When scanning untrusted or large sets of binaries:
- check `file.Header.IsValid` (or `ObjFile.IsValid`) first;
- check `IsEmpty` on a directory before enumerating it;
- wrap per-resource / per-directory decoding in `try/catch` so one bad structure does not abort the whole file;
- `Win32Loader.FromFile` throws `Win32Exception` if Windows refuses to map the image — fall back to `StreamLoader` in that case.

### Building from source
The repository uses a git submodule for code shared with other AlphaOmega readers:
```
git clone --recurse-submodules https://github.com/DKorablin/PEReader.git
cd PEReader
dotnet build PEReader.sln -c Release
```
If you already cloned without submodules, run `git submodule update --init`. The CI builds the multi-target project with MSBuild on Windows (`msbuild PEReader.sln /p:Configuration=Release`).

### License
MIT

---
<details>
<summary>Full structure list</summary>

- DOS header (IMAGE_DOS_HEADER)
  - PE/PE+ header (IMAGE_NT_HEADERS)
  - File header (IMAGE_FILE_HEADER)
  - Optional header (IMAGE_OPTIONAL_HEADER)
  - COFF header (IMAGE_COFF_SYMBOLS_HEADER)
  - Sections header (IMAGE_SECTION_HEADER[])
  - Directories:
    - Architecture
    - Bound import
    - Certificate
    - CLR runtime header
      - MetaData
        - #~
          - Module
          - TypeRef
          - TypeDef
          - FieldPtr
          - Field
          - MethodPtr
          - MethodDef
            - Signature
            - IL Instructions
          - ParamPtr
          - Param
          - InterfaceImpl
          - MemberRef
            - Signature
          - Constant
          - CustomAttribute
            - Signature
          - FieldMarshal
          - DeclSecurity
          - ClassLayout
          - FieldLayout
          - StandAloneSig
          - EventMap
          - EventPtr
          - Event
          - PropertyMap
          - PropertyPtr
          - Property
            - Signature
          - MethodSemantics
          - MethodImpl
          - ModuleRef
          - TypeSpec
          - ImplMap
          - FieldRVA
          - ENCLog
          - ENCMap
          - Assembly
          - AssemblyProcessor
          - AssemblyOS
          - AssemblyRef
          - AssemblyRefProcessor
          - AssemblyRefOS
          - File
          - ExportedType
          - ManifestResource
          - NestedClass
          - GenericParam
          - MethodSpec
          - GenericParamConstraint
        - #Strings
        - #US
        - #Guid
        - #Blob
      - Resource Table (Linked to ManifestResource MetaData table)
      - VTable fixup
      - Code Manager Table
      - Export Address Table
      - Managed Native Header
      - Strong Name Signature
    - Debug
      - CodeView PDB2
      - CodeView PDB7
      - Misc (IMAGE_DEBUG_MISC)
    - Delay Import Descriptor
      - List of delay load imported libraries
      - List of delay load imported functions
    - Exception Table
    - Export Table
      - List of exported functions
    - Global Ptr
    - IAT
    - Import Table
      - List of imported libraries
      - List of imported functions
    - Load Config Table
    - Base Relocation Table
      - Relocation Blocks
      - Relocation Sections
    - Resource Table. Supported resource types:
      - RT_STRING
      - RT_HTML
      - RT_MANIFEST
      - RT_ACCELERATOR
      - RT_MESSAGETABLE
      - RT_DIALOG (Without DLU conversion)
      - RT_DLGINIT
      - RT_BITMAP
      - RT_ICON (including PNG-compressed icons)
      - RT_FONT
      - RT_FONTDIR
      - RT_MENU
      - RT_TOOLBAR
      - RT_VERSION
    - TLS Table

Few supported structures:

- DOS Header
  - IMAGE_DOS_HEADER
- PE/PE+ Headers
  - IMAGE_FILE_HEADER
  - IMAGE_OPTIONAL_HEADER32
  - IMAGE_OPTIONAL_HEADER64
  - IMAGE_NT_HEADERS32
  - IMAGE_NT_HEADERS64
  - IMAGE_SECTION_HEADER
  - Unmanaged resources
    - IMAGE_RESOURCE_DIRECTORY
    - IMAGE_RESOURCE_DIRECTORY_ENTRY
    - IMAGE_RESOURCE_DATA_ENTRY
    - IMAGE_RESOURCE_DIRECTORY_STRING
    - ACCELTABLEENTRY
    - DLGTEMPLATE
    - DLGITEMTEMPLATE
    - DLGTEMPLATEEX
    - DLGITEMTEMPLATEEX
    - MENUHEADER
    - MENUITEMTEMPLATE
    - MENUITEMEX
    - BITMAPINFOHEADER
    - VS_VERSIONINFO
    - VS_FIXEDFILEINFO
    - VarFileInfo
    - StringTable
    - V_STRING
    - FONTDIRENTRY
    - MESSAGE_RESOURCE_BLOCK
    - MESSAGE_RESOURCE_ENTRY
  - IMAGE_IMPORT_DESCRIPTOR
  - IMAGE_THUNK_DATA32
  - IMAGE_THUNK_DATA64
  - IMAGE_IMPORT_BY_NAME
  - IMAGE_RUNTIME_FUNCTION_ENTRY
  - IMAGE_DATA_DIRECTORY
  - IMAGE_EXPORT_DIRECTORY
  - IMAGE_BOUND_IMPORT_DESCRIPTOR
  - IMAGE_BOUND_FORWARDER_REF
  - WIN_CERTIFICATE
  - ImgDelayDescr
  - IMAGE_BASE_RELOCATION
  - IMAGE_LOAD_CONFIG_DIRECTORY32
  - IMAGE_LOAD_CONFIG_DIRECTORY64
  - IMAGE_TLS_DIRECTORY32
  - IMAGE_TLS_DIRECTORY64
  - Debug directory
    - IMAGE_DEBUG_DIRECTORY
    - CV_INFO_PDB70
    - CV_HEADER
    - CV_INFO_PDB20
    - IMAGE_DEBUG_MISC
  - .NET CLI
    - IMAGE_COR20_HEADER
    - IMAGE_COR20_METADATA
    - IMAGE_COR20_VTABLE
    - ResourceManagerHeader
    - ResourceSetHeader
    - STREAM_HEADER
    - STREAM_TABLE_HEADER
</details>