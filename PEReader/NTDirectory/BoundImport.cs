using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Bound import class</summary>
	[DefaultProperty(nameof(ModuleName))]
	public class BoundImport : PEDirectoryBase, IEnumerable<BoundImportReference>
	{
		/// <summary>Header</summary>
		public WinNT.IMAGE_BOUND_IMPORT_DESCRIPTOR? Descriptor
			=> base.IsEmpty
					? (WinNT.IMAGE_BOUND_IMPORT_DESCRIPTOR?)null
					: base.Parent.Header.PtrToStructure<WinNT.IMAGE_BOUND_IMPORT_DESCRIPTOR>(base.Directory.VirtualAddress);

		/// <summary>Module name which this image is bounded</summary>
		public String ModuleName
		{
			get
			{
				var descriptor = this.Descriptor;
				return descriptor == null
					? null
					: base.Parent.Header.PtrToStringAnsi(base.Directory.VirtualAddress + descriptor.Value.OffsetModuleName);
			}
		}
		/// <summary>Directory is empty</summary>
		public override Boolean IsEmpty
			=> base.IsEmpty || this.Descriptor.Value.NumberOfModuleForwarderRefs == 0;

		/// <summary>Create instance of bound import class</summary>
		/// <param name="root">Data directory</param>
		public BoundImport(PEFile root)
			: base(root, WinNT.IMAGE_DIRECTORY_ENTRY.BOUND_IMPORT) { }

		/// <summary>Get the list of bound import class</summary>
		/// <returns>Stream of bound import classes</returns>
		public IEnumerator<BoundImportReference> GetEnumerator()
		{
			WinNT.IMAGE_BOUND_IMPORT_DESCRIPTOR? descriptor = this.Descriptor;
			if(descriptor == null)
				yield break;

			if(descriptor.Value.NumberOfModuleForwarderRefs > 0)
			{
				var directory = base.Directory;
				UInt32 sizeOfStruct = (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_BOUND_FORWARDER_REF));

				UInt32 padding = directory.VirtualAddress + (UInt32)Marshal.SizeOf(typeof(WinNT.IMAGE_BOUND_IMPORT_DESCRIPTOR));
				for(Int32 loop = 0; loop < descriptor.Value.NumberOfModuleForwarderRefs; loop++)
				{
					//UInt32 padding = checked((UInt32)(directory.VirtualAddress + descriptorSize + (sizeOfStruct * loop)));
					WinNT.IMAGE_BOUND_FORWARDER_REF ffdRef = this.Parent.Header.PtrToStructure<WinNT.IMAGE_BOUND_FORWARDER_REF>(padding);
					padding += sizeOfStruct;

					String moduleName = this.Parent.Header.PtrToStringAnsi(base.Directory.VirtualAddress + ffdRef.OffsetModuleName);

					yield return new BoundImportReference() { FfwdRef = ffdRef, ModuleName = moduleName, };
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}