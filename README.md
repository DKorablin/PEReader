Portable Executable reader
========

PE/PE+/CLI executable reader assembly.

Usage:
<pre>
using(PEFile file = new PEFile(StreamLoader.FromFile(@"C:\Windows\System32\kernel32.dll")))
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

		if(!file.Crtificate.IsEmpty)
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
</pre>

<ul>
  <li>DOS header (IMAGE_DOS_HEADER)</li>
	<li>PE/PE+ header (IMAGE_NT_HEADERS)</li>
	<li>File header (IMAGE_FILE_HEADER)</li>
	<li>Optional header (IMAGE_OPTIONAL_HEADER)</li>
	<li>COFF header (IMAGE_COFF_SYMBOLS_HEADER)</li>
	<li>Sections header (IMAGE_SECTION_HEADER[])</li>
	<li>Directories:
		<ul>
			<li>Architecture</li>
			<li>Bound import</li>
			<li>Certificate</li>
			<li>CLR runtime header
				<ul>
					<li>MetaData
						<ul>
							<li>#~
								<ul>
									<li>Module</li>
									<li>TypeRef</li>
									<li>TypeDef</li>
									<li>FieldPtr</li>
									<li>Field</li>
									<li>MethodPtr</li>
									<li>MethodDef</li>
									<li>ParamPtr</li>
									<li>Param</li>
									<li>InterfaceImpl</li>
									<li>MemberRef</li>
									<li>Constant</li>
									<li>CustomAttribute</li>
									<li>FieldMarshal</li>
									<li>DeclSecurity</li>
									<li>ClassLayout</li>
									<li>FieldLayout</li>
									<li>StandAloneSig</li>
									<li>EventMap</li>
									<li>EventPtr</li>
									<li>Event</li>
									<li>PropertyMap</li>
									<li>PropertyPtr</li>
									<li>Property</li>
									<li>MethodSemantics</li>
									<li>MethodImpl</li>
									<li>ModuleRef</li>
									<li>TypeSpec</li>
									<li>ImplMap</li>
									<li>FieldRVA</li>
									<li>ENCLog</li>
									<li>ENCMap</li>
									<li>Assembly</li>
									<li>AssemblyProcessor</li>
									<li>AssemblyOS</li>
									<li>AssemblyRef</li>
									<li>AssemblyRefProcessor</li>
									<li>AssemblyRefOS</li>
									<li>File</li>
									<li>ExportedType</li>
									<li>ManifestResource</li>
									<li>NestedClass</li>
									<li>GenericParam</li>
									<li>MethodSpec</li>
									<li>GenericParamConstraint</li>
								</ul>
							</li>
							<li>#Strings</li>
							<li>#US</li>
							<li>#Guid</li>
							<li>#Blob</li>
						</ul>
					</li>
					<li>Resource Table (Linked to ManifestResource MetaData table)</li>
					<li>VTable fixup</li>
					<li>Code Manager Table</li>
					<li>Export Address Table</li>
					<li>Managed Native Header</li>
					<li>Strong Name Signature</li>
				</ul>
			</li>
			<li>Debug
				<ul>
					<li>CodeView PDB2</li>
					<li>CodeView PDB7</li>
					<li>Misc (IMAGE_DEBUG_MISC)</li>
				</ul>
			</li>
			<li>Delay Import Descriptor
				<ul>
					<li>List of delay load imported libraries</li>
					<li>List of delay load imported functions</li>
				</ul>
			</li>
			<li>Exception Table</li>
			<li>Export Table
				<ul>
					<li>List of exported functions</li>
				</ul>
			</li>
			<li>Global Ptr</li>
			<li>IAT</li>
			<li>Import Table
				<ul>
					<li>List of imported libraries</li>
					<li>List of imported functions</li>
				</ul>
			</li>
			<li>Load Config Table</li>
			<li>Base Relocation Table
				<ul>
					<li>Relocation Blocks</li>
					<li>Relocation Sections</li>
				</ul>
			</li>
			<li>Resource Table. Supported resource types:
				<ul>
					<li>RT_STRING</li>
					<li>RT_HTML</li>
					<li>RT_MANIFEST</li>
					<li>RT_ACCELERATOR</li>
					<li>RT_MESSAGETABLE</li>
					<li>RT_DIALOG (Without DLU conversion)</li>
					<li>RT_BITMAP</li>
					<li>RT_MENU</li>
					<li>RT_VERSION</li>
				</ul>
			</li>
			<li>TLS Table</li>
		</ul>
	</li>
</ul>

Few suported structures:

<ul>
	<li>DOS Header
		<ul>
			<li>IMAGE_DOS_HEADER</li>
		</ul>
	</li>
	<li>PE/PE+ Headers
		<ul>
			<li>IMAGE_FILE_HEADER</li>
			<li>IMAGE_OPTIONAL_HEADER32</li>
			<li>IMAGE_OPTIONAL_HEADER64</li>
			<li>IMAGE_NT_HEADERS32</li>
			<li>IMAGE_NT_HEADERS64</li>
			<li>IMAGE_SECTION_HEADER</li>
			<li>Unmanaged resources
				<ul>
					<li>IMAGE_RESOURCE_DIRECTORY</li>
					<li>IMAGE_RESOURCE_DIRECTORY_ENTRY</li>
					<li>IMAGE_RESOURCE_DATA_ENTRY</li>
					<li>IMAGE_RESOURCE_DIRECTORY_STRING</li>
					<li>ACCELTABLEENTRY</li>
					<li>DLGTEMPLATE</li>
					<li>DLGITEMTEMPLATE</li>
					<li>DLGTEMPLATEEX</li>
					<li>DLGITEMTEMPLATEEX</li>
					<li>MENUHEADER</li>
					<li>MENUITEM</li>
					<li>MENUITEMPOPUP</li>
					<li>MENUITEMEX</li>
					<li>BITMAPINFOHEADER</li>
					<li>VS_VERSIONINFO</li>
					<li>VS_FIXEDFILEINFO</li>
					<li>VarFileInfo</li>
					<li>StringTable</li>
					<li>V_STRING</li>
					<li>FONTDIRENTRY</li>
					<li>MESSAGE_RESOURCE_BLOCK</li>
					<li>MESSAGE_RESOURCE_ENTRY</li>
				</ul>
			</li>
			<li>IMAGE_IMPORT_DESCRIPTOR</li>
			<li>IMAGE_THUNK_DATA32</li>
			<li>IMAGE_THUNK_DATA64</li>
			<li>IMAGE_IMPORT_BY_NAME</li>
			<li>IMAGE_RUNTIME_FUNCTION_ENTRY</li>
			<li>IMAGE_DATA_DIRECTORY</li>
			<li>IMAGE_EXPORT_DIRECTORY</li>
			<li>IMAGE_BOUND_IMPORT_DESCRIPTOR</li>
			<li>IMAGE_BOUND_FORWARDER_REF</li>
			<li>WIN_CERTIFICATE</li>
			<li>ImgDelayDescr</li>
			<li>IMAGE_BASE_RELOCATION</li>
			<li>IMAGE_LOAD_CONFIG_DIRECTORY32</li>
			<li>IMAGE_LOAD_CONFIG_DIRECTORY64</li>
			<li>IMAGE_TLS_DIRECTORY32</li>
			<li>IMAGE_TLS_DIRECTORY64</li>
			<li>Debug directory
				<ul>
					<li>IMAGE_DEBUG_DIRECTORY</li>
					<li>CV_INFO_PDB70</li>
					<li>CV_HEADER</li>
					<li>CV_INFO_PDB20</li>
					<li>IMAGE_DEBUG_MISC</li>
				</ul>
			</li>
			<li>.NET CLI
				<ul>
					<li>IMAGE_COR20_HEADER</li>
					<li>IMAGE_COR20_METADATA</li>
					<li>IMAGE_COR20_VTABLE</li>
					<li>ResourceManagerHeader</li>
					<li>ResourceSetHeader</li>
					<li>STREAM_HEADER</li>
					<li>STREAM_TABLE_HEADER</li>
				</ul>
			</li>
		</ul>
	</li>
</ul>
