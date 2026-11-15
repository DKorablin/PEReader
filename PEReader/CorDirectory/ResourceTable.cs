using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;
using AlphaOmega.Debug.NTDirectory;

namespace AlphaOmega.Debug.CorDirectory
{
	/// <summary>Managed resources directory</summary>
	/// <remarks>
	/// The directory is a sequence of embedded resources, each prefixed with <see cref="UInt32"/> length.
	/// Every resource is described by a row in the ManifestResource metadata table.
	/// Resources linked from another file or assembly are not stored in this directory and are not enumerated here,
	/// use <see cref="StreamTables.ManifestResource"/> to get them.
	/// </remarks>
	[DefaultProperty(nameof(Count))]
	public class ResourceTable : CorDirectoryBase, IEnumerable<ResourceTableReader>
	{
		private ResourceTableReader[] _resources;

		/// <summary>Number of embedded resources</summary>
		public Int32 Count => this.GetResources().Length;

		/// <summary>Get embedded resource by its manifest name</summary>
		/// <param name="name">Name of the resource from ManifestResource table</param>
		/// <returns>Embedded resource or <c>null</c> if resource is not found</returns>
		public ResourceTableReader this[String name]
		{
			get
			{
				foreach(ResourceTableReader resource in this.GetResources())
					if(resource.Name == name)
						return resource;
				return null;
			}
		}

		/// <summary>Create instance of managed resource class</summary>
		/// <param name="parent">NT directory</param>
		public ResourceTable(ComDescriptor parent)
			: base(parent, WinNT.COR20_DIRECTORY_ENTRY.Resources) { }

		/// <summary>Get all embedded resources from this directory</summary>
		/// <returns>Embedded resources</returns>
		public IEnumerator<ResourceTableReader> GetEnumerator()
		{
			foreach(ResourceTableReader resource in this.GetResources())
				yield return resource;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();

		/// <summary>Get length of the embedded resource</summary>
		/// <param name="offset">Offset of the resource from the beginning of the directory (ManifestResource.Offset)</param>
		/// <exception cref="InvalidDataException">Resource is located outside of the directory bounds</exception>
		/// <returns>Length of the resource in bytes</returns>
		internal UInt32 GetResourceLength(UInt32 offset)
		{
			UInt32 directorySize = base.Directory.Size;
			if(directorySize < sizeof(UInt32) || offset > directorySize - sizeof(UInt32))
				throw new InvalidDataException($"Resource offset 0x{offset:X} is outside of the resource directory (Size: 0x{directorySize:X})");

			UInt32 length = base.Parent.Parent.Header.PtrToStructure<UInt32>(base.Directory.VirtualAddress + offset);
			if(length > directorySize - sizeof(UInt32) - offset)
				throw new InvalidDataException($"Resource at offset 0x{offset:X} with length 0x{length:X} is outside of the resource directory (Size: 0x{directorySize:X})");

			return length;
		}

		/// <summary>Get contents of the embedded resource</summary>
		/// <param name="offset">Offset of the resource from the beginning of the directory (ManifestResource.Offset)</param>
		/// <exception cref="InvalidDataException">Resource is located outside of the directory bounds</exception>
		/// <returns>Resource contents without length prefix</returns>
		internal Byte[] GetResourceData(UInt32 offset)
		{
			UInt32 length = this.GetResourceLength(offset);
			return length == 0
				? new Byte[0]
				: base.Parent.Parent.Header.ReadBytes(base.Directory.VirtualAddress + offset + sizeof(UInt32), length);
		}

		private ResourceTableReader[] GetResources()
		{
			if(this._resources == null)
			{
				List<ResourceTableReader> result = new List<ResourceTableReader>();

				StreamTables tables = base.Parent.MetaData.StreamTables;
				if(tables != null && tables[Cor.MetaTableType.ManifestResource] != null)
					foreach(ManifestResourceRow row in tables.ManifestResource)
						if(row.IsEmbedded)
							result.Add(new ResourceTableReader(row));

				this._resources = result.ToArray();
			}
			return this._resources;
		}
	}
}