using System;
using System.IO;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Internal or external resource</summary>
	public class ManifestResourceRow : BaseMetaRow
	{
		private UInt32? _size;
		private Byte[] _file;

		/// <summary>The Offset specifies the byte offset within the referenced file at which this resource record begins</summary>
		/// <remarks>For embedded resources it points to the <see cref="UInt32"/> length prefix in the managed resources directory</remarks>
		public UInt32 Offset => this.GetValue<UInt32>(0);

		/// <summary>Resource flags</summary>
		public ResourceAttributes Flags => (ResourceAttributes)this.GetValue<UInt32>(1);

		/// <summary>Manifest resource name</summary>
		public String Name => this.GetValue<String>(2);

		/// <summary>The Implementation specifies which file holds this resource</summary>
		/// <remarks>
		/// An index into a File table, a AssemblyRef table, or null;
		/// more precisely, an Implementation (§II.24.2.6) coded index.
		/// </remarks>
		public MetaCellCodedToken Implementation => this.GetValue<MetaCellCodedToken>(3);

		/// <summary>Resource is embedded in the managed resources directory of the current PE file</summary>
		/// <remarks>Otherwise the resource is stored in another file (File table) or in another assembly (AssemblyRef table)</remarks>
		public Boolean IsEmbedded => this.Implementation.RowIndex == null;

		/// <summary>Size of the embedded resource or 0 if resource is not embedded</summary>
		/// <exception cref="InvalidDataException">Resource is located outside of the managed resources directory</exception>
		public UInt32 Size
		{
			get
			{
				if(this._size == null)
					this._size = this.IsEmbedded
						? this.GetResourceDirectory().GetResourceLength(this.Offset)
						: 0;

				return this._size.Value;
			}
		}

		/// <summary>Contents of the embedded resource or <c>null</c> if resource is not embedded</summary>
		/// <exception cref="InvalidDataException">Resource is located outside of the managed resources directory</exception>
		public Byte[] File
		{
			get
			{
				if(this._file == null && this.IsEmbedded)
					this._file = this.GetResourceDirectory().GetResourceData(this.Offset);

				return this._file;
			}
		}

		/// <summary>Create instance of <see cref="ManifestResourceRow"/></summary>
		public ManifestResourceRow()
			: base(Cor.MetaTableType.ManifestResource) { }

		/// <summary>Create instance of resource reader class pointed to resource file on current PE file</summary>
		/// <exception cref="InvalidOperationException">Resource is not embedded in the current PE file</exception>
		/// <returns>Embedded resource reader</returns>
		public ResourceTableReader GetResourceReader()
			=> this.IsEmbedded
				? new ResourceTableReader(this)
				: throw new InvalidOperationException($"Resource {this.Name} is not embedded in the current PE file");

		/// <summary>String representation</summary>
		/// <returns>String</returns>
		public override String ToString()
			=> base.ToString(this.Name);

		/// <summary>Managed resource directory</summary>
		/// <exception cref="InvalidDataException">Managed resource directory is empty</exception>
		private ResourceTable GetResourceDirectory()
			=> this.Row.Table.Root.Parent.Parent.Resources
				?? throw new InvalidDataException($"Resource {this.Name} is marked as embedded but managed resource directory is empty");
	}
}