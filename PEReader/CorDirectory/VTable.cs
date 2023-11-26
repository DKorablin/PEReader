using System;
using System.ComponentModel;
using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Function pointers for fixup class</summary>
	/// <remarks>
	/// Certain languages, which choose not to follow the common type system runtime model,
	/// can have virtual functions which need to be represented in a v-table.
	/// These v-tables are laid out by the compiler, not by the runtime.
	/// Finding the correct v-table slot and calling indirectly through the value held in that slot
	/// is also done by the compiler.
	/// </remarks>
	[DefaultProperty(nameof(Header))]
	public class VTable : CorDirectoryBase
	{//TODO: Не протестировано

		/// <summary>VTable header</summary>
		public Cor.IMAGE_COR20_VTABLE? Header
		{
			get
			{
				if(base.IsEmpty)
					return null;

				UInt32 position = base.Directory.VirtualAddress;
				return base.Parent.Parent.Header.PtrToStructure<Cor.IMAGE_COR20_VTABLE>(position);
			}
		}
		/// <summary>Create instance of VTable class</summary>
		/// <param name="parent">.NET directory</param>
		public VTable(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.VTableFuxups) { }
	}
}