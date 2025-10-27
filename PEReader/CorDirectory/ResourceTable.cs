using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;
using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Managed resource table class</summary>
	[DefaultProperty(nameof(Header))]
	public class ResourceTable : CorDirectoryBase, IEnumerable<ResourceTableReader>
	{
		private Cor.ResourceManagerHeader? _header;
		private Cor.ResourceSetHeader? _set;

		/// <summary>Managed resource header</summary>
		public Cor.ResourceManagerHeader Header
			=> this._header ??
				(this._header = base.Parent.Parent.Header.PtrToStructure<Cor.ResourceManagerHeader>(base.Directory.VirtualAddress)).Value;

		/// <summary>Resource header</summary>
		/// <exception cref="InvalidOperationException">Managed resource header is invalid</exception>
		public Cor.ResourceSetHeader RuntimeHeader
		{
			get
			{
				if(this._set == null)
				{
					if(!this.Header.IsValid)
						throw new InvalidOperationException("Invalid magic");

					UInt32 offset = base.Directory.VirtualAddress + (UInt32)Marshal.SizeOf(typeof(Cor.ResourceManagerHeader))
						+ this.Header.SizeOfReaderType;
					this._set = base.Parent.Parent.Header.PtrToStructure<Cor.ResourceSetHeader>(offset);
				}
				return this._set.Value;
			}
		}

		/// <summary>Create instance of managed resource class</summary>
		/// <param name="parent">NT directory</param>
		public ResourceTable(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.Resources) { }

		/// <summary>Get all metadata table rows that contains files in this directory</summary>
		/// <returns>ManifestResource table rows</returns>
		public IEnumerator<ResourceTableReader> GetEnumerator()
		{
			foreach(ManifestResourceRow row in base.Parent.MetaData.StreamTables.ManifestResource)
				if(row.FileInDirectory)
					yield return row.GetResourceReader();
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>Decode 7bit encoded Int</summary>
		/// <param name="offset">Offset from resource_manager header</param>
		/// <param name="value"></param>
		/// <param name="size"></param>
		private void DecodeInt(UInt32 offset, out UInt32 value, out UInt32 size)
		{//TODO: Remaining from unmanaged method of reading resources from file
			Int32 a=0, b=0;
			UInt32 x = 0;
			while(true)
			{
				Byte c = base.Parent.Parent.Header.ReadBytes(offset++, 1)[0];
				a |= ((c & 0x7F) << (b & 0x1F));
				b += 7;

				x++;

				if((c & 0x80) == 0)
					break;
			}
			value = (UInt32)a;
			size = x;
		}
	}
}