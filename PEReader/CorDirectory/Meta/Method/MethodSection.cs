using System;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Fat method header section descriptor</summary>
	public class MethodSection
	{
		/// <summary>Method header section descriptor</summary>
		public Cor.CorILMethodSection Section { get; private set; }

		/// <summary>Fat exception handling clause</summary>
		public Cor.CorILMethodExceptionFat[] Fat { get; private set; }

		/// <summary>Small exception handling clause</summary>
		public Cor.CorILMethodExceptionSmall[] Small { get; private set; }

		internal MethodSection(Cor.CorILMethodSection section, Cor.CorILMethodExceptionFat[] fat, Cor.CorILMethodExceptionSmall[] small)
		{
			this.Section = section;
			this.Fat = fat;
			this.Small = small;
		}
	}
}