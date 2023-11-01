using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Member signature parser from Byte array to strongly typed array of input/output values</summary>
	internal class SignatureParser
	{
		public Cor.IMAGE_CEE_CS CorCallingConvention { get; }

		/// <summary>Member return type</summary>
		public ElementType ReturnType { get; }
		
		/// <summary>Count of input arguments</summary>
		public Int32 ArgsCount { get; }

		/// <summary>Member arguments types</summary>
		public ElementType[] ArgumentsTypes { get; }

		internal SignatureParser(Byte[] signature)
		{
			this.ArgsCount = signature[1];
			this.CorCallingConvention = ((Cor.IMAGE_CEE_CS)signature[0] & Cor.IMAGE_CEE_CS.MASK);

			UInt32 offset = 2;
			this.ReturnType = new ElementType(signature, ref offset);

			this.ArgumentsTypes = new ElementType[this.ArgsCount];
			for(Byte loop = 0; loop < this.ArgsCount; loop++)
			{
				this.ArgumentsTypes[loop] = new ElementType(signature, ref offset);
			}
		}
	}
}