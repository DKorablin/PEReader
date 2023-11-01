using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Type of method element type</summary>
	public struct ElementType
	{
		/// <summary>Managed element type</summary>
		public Cor.ELEMENT_TYPE Type { get; internal set; }

		/// <summary>Element is array</summary>
		public Boolean IsArray { get; internal set; }

		/// <summary>Element is pointer</summary>
		public Boolean IsPointer { get; internal set; }

		internal ElementType(Byte[] signature, ref UInt32 offset)
		{
			this.Type = (Cor.ELEMENT_TYPE)signature[offset++];
			this.IsArray = this.Type == Cor.ELEMENT_TYPE.SZARRAY;
			if(this.IsArray)
				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];
			this.IsPointer = this.Type == Cor.ELEMENT_TYPE.PTR;
			if(this.IsPointer)
				this.Type = (Cor.ELEMENT_TYPE)signature[offset++];
		}

		public override String ToString()
		{
			String arr = this.IsArray ? "[]" : String.Empty;
			String ptr = this.IsPointer ? "*" : String.Empty;
			return $"{this.GetType().Name}: {{{this.Type}}}{arr}{ptr}";
		}
	}
}