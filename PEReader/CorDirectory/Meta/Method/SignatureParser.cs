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

		internal SignatureParser(MetaRow row, Byte[] signature)
		{
			Cor.IMAGE_CEE_CS signatureFlags = (Cor.IMAGE_CEE_CS)signature[0];
			this.CorCallingConvention = signatureFlags & Cor.IMAGE_CEE_CS.MASK;

			UInt32 offset;
			if((signatureFlags & Cor.IMAGE_CEE_CS.FIELD) == Cor.IMAGE_CEE_CS.FIELD)
			{
				this.ArgsCount = 0;
				offset = 1u;
			} else if((signatureFlags & Cor.IMAGE_CEE_CS.GENERIC) == Cor.IMAGE_CEE_CS.GENERIC)
			{
				Int32 genericArgumentsCount = signature[1];
				this.ArgsCount = signature[2];//TODO: Check for compressed unsigned integer
				offset = 3u;
				//GENRICINST has the value 0x0A. [Note: This value is known as IMAGE_CEE_CS_CALLCONV_GENERICINST in
				//the Microsoft CLR implementation. end note] The GenArgCount is a compressed unsigned integer indicating
				//the number of generic arguments in the method. The blob then specifies the instantiated type, repeating a total
				//of GenArgCount times
			} else
			{
				this.ArgsCount = signature[1];
				offset = 2u;
			}

			this.ReturnType = new ElementType(row.Cells[0], signature, ref offset);
			this.ArgumentsTypes = new ElementType[this.ArgsCount];
			for(Byte loop = 0; loop < this.ArgsCount; loop++)
				this.ArgumentsTypes[loop] = new ElementType(row.Cells[0], signature, ref offset);

			if(offset != signature.Length)
			{
				Exception exc = new InvalidOperationException($"Signature value not fully parsed. Length: {signature.Length:X} Offset:{offset:X}");
				exc.Data.Add("Signature", signature);
				throw exc;
			}
		}
	}
}