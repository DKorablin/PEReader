using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary><see cref="WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR"/> resource reader class</summary>
	/// <remarks>
	/// An accelerator table resource is an array of <see cref="WinUser.ACCELTABLEENTRY"/> structures.
	/// The last entry in the table is marked with the <see cref="WinUser.AccelFlags.Last"/> flag.
	/// </remarks>
	public class ResourceAccelerator : ResourceBase, IEnumerable<WinUser.ACCELTABLEENTRY>
	{
		/// <summary>Create instance of accelerators resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceAccelerator(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR) { }

		/// <summary>Get all accelerators from the accelerator table</summary>
		/// <remarks>
		/// Enumeration stops after the entry marked with <see cref="WinUser.AccelFlags.Last"/> (the same way the Windows loader does),
		/// so any alignment padding after the last entry is ignored.
		/// If the table has no terminating entry, enumeration stops at the last whole structure in the resource data and a trailing partial structure is ignored.
		/// </remarks>
		/// <returns>Enumerator over the accelerator table entries</returns>
		public IEnumerator<WinUser.ACCELTABLEENTRY> GetEnumerator()
		{
			UInt32 entrySize = (UInt32)Marshal.SizeOf(typeof(WinUser.ACCELTABLEENTRY));
			UInt32 position = 0;

			using(PinnedBufferReader reader = this.CreateDataReader())
				while(position + entrySize <= reader.Length)
				{
					WinUser.ACCELTABLEENTRY entry = reader.BytesToStructure<WinUser.ACCELTABLEENTRY>(ref position);
					yield return entry;

					if((entry.fFlags & WinUser.AccelFlags.Last) == WinUser.AccelFlags.Last)
						yield break;
				}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}