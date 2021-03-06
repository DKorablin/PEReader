﻿using System.Resources;

namespace AlphaOmega.Debug
{
	internal static class Resources
	{
		private static ResourceManager _section;

		public static ResourceManager Section
		{
			get
			{
				return _section == null
					? _section = new ResourceManager("AlphaOmega.Debug.Section", typeof(Resources).Assembly)
					: _section;
			}
		}
	}
}