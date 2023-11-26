## Portable Executable reader
[![Auto build](https://github.com/DKorablin/PEReader/actions/workflows/release.yml/badge.svg)](https://github.com/DKorablin/PEReader/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/AlphaOmega.PEReader)](https://www.nuget.org/packages/AlphaOmega.PEReader)

PE/PE+/CLI executable reader assembly. Compatible with .NET Framework 2.0 & .NET Standard 2.0

Usage (See Wiki for details):

    String filePath=@"C:\Windows\System32\kernel32.dll";
    using(PEFile file = new PEFile(filePath, StreamLoader.FromFile(filePath)))
    {
        if(file.Header.IsValid)
        {
            if(!file.Resource.IsEmpty)
            {//IMAGE_RESOURCE_DIRECTORY
                //...
            }

            if(!file.Export.IsEmpty)
            {//IMAGE_EXPORT_DIRECTORY
                //...
            }

            if(!file.Import.IsEmpty)
            {//IMAGE_IMPORT_DESCRIPTOR
                //...
            }

            if(!file.Certificate.IsEmpty)
            {//WIN_CERTIFICATE
                X509Certificate2 cert = file.Certificate.X509;
                //...
            }

            if(file.ComDescriptor != null)
            {//CLI
                var metaData = file.ComDescriptor.MetaData;
                //...
            }
        }
    }

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
      - RT_BITMAP
      - RT_MENU
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
    - MENUITEM
    - MENUITEMPOPUP
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
