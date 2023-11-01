using System;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Internal or external resource</summary>
	public class ManifestResourceRow : BaseMetaRow
	{
		#region Fields
		private UInt32? _size;
		private Byte[] _file;
		#endregion Fields

		/// <summary>The Offset specifies the byte offset within the referenced file at which this resource record begins</summary>
		internal UInt32 OffsetI { get { return base.GetValue<UInt32>(0); } }

		/// <summary>The Offset specifies the byte offset within the referenced file at which this resource record begins and size of resource file is skipped</summary>
		public UInt32 Offset { get { return this.OffsetI + sizeof(UInt32); } }

		/// <summary>Resource flags</summary>
		public ResourceAttributes Flags { get { return (ResourceAttributes)base.GetValue<UInt32>(1); } }

		/// <summary>Manifest resource name</summary>
		public String Name { get { return base.GetValue<String>(2); } }

		/// <summary>The Implementation specifies which file holds this resource</summary>
		/// <remarks>
		/// An index into a File table, a AssemblyRef table, or null;
		/// more precisely, an Implementation (§II.24.2.6) coded index.
		/// </remarks>
		public MetaCellCodedToken Implementation { get { return base.GetValue<MetaCellCodedToken>(3); } }

		/// <summary>Size of file in the resource directory</summary>
		public UInt32 Size
		{
			get
			{
				if(this._size == null)
					if(this.FileInDirectory)
					{
						ResourceTable dir = this.ResourceDirectory;
						PEHeader header = dir.Parent.Parent.Header;

						this._size = header.PtrToStructure<UInt32>(dir.Directory.VirtualAddress + this.OffsetI);
					} else
						this._size = 0;

				return this._size.Value;
			}
		}

		/// <summary>File contents from managed Resource directory</summary>
		/// <exception cref="NotImplementedException">Implementation points to row</exception>
		public Byte[] File
		{
			get
			{
				if(this._file == null)
					if(this.FileInDirectory)
					{
						//if(this.Offset == 0) throw new InvalidOperationException("Offset must points to Resource directory");

						ResourceTable dir = this.ResourceDirectory;
						PEHeader header = dir.Parent.Parent.Header;

						UInt32 padding = dir.Directory.VirtualAddress + this.Offset;

						this._file = header.ReadBytes(padding, this.Size);
					}
				return this._file;
			}
		}
		/// <summary>File located in directory with managed resources</summary>
		internal Boolean FileInDirectory { get { return this.Implementation.TargetRow == null; } }

		/// <summary>Managed resource directory</summary>
		private ResourceTable ResourceDirectory { get { return base.Row.Table.Root.Parent.Parent.Resources; } }

		/// <summary>Create instance of manifest resource row</summary>
		public ManifestResourceRow()
			: base(Cor.MetaTableType.ManifestResource) { }

		/// <summary>Create instance of resource reader class pointed to resource file on current PE file</summary>
		/// <returns>Contexts of resource file</returns>
		public ResourceTableReader GetResourceReader()
		{
			return new ResourceTableReader(this.Name, this.File);//While calling Dispose method - stream is closed
		}

		/// <summary>String representation</summary>
		/// <returns>String</returns>
		public override String ToString()
		{
			return base.ToString(this.Name);
		}
	}
}