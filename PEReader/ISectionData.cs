using System;

namespace AlphaOmega.Debug
{
	/// <summary>Data from andy PE section</summary>
	public interface ISectionData
	{
		/// <summary>Get all payload from PE section as Byte array</summary>
		/// <returns>Byte array from section</returns>
		Byte[] GetData();
	}
}