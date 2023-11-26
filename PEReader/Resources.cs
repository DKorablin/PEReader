using System.Resources;

namespace AlphaOmega.Debug
{
	/// <summary>Strongly typed resource layer</summary>
	internal static class Resources
	{
		private static ResourceManager _section;

		/// <summary>Sections description</summary>
		public static ResourceManager Section
			=> _section ?? (_section = new ResourceManager("AlphaOmega.Debug.Section", typeof(Resources).Assembly));
	}
}