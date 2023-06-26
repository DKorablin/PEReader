using System;

namespace AlphaOmega.Debug
{
	/// <summary>Directory from PE file</summary>
	public interface IDirectory
	{
		/// <summary>Directory is empty</summary>
		Boolean IsEmpty { get; }

		/// <summary>Directory descriptor</summary>
		WinNT.IMAGE_DATA_DIRECTORY Directory { get; }
	}
}