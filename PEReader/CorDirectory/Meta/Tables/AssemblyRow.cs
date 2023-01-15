using System;
using System.ComponentModel;
using System.Configuration.Assemblies;
using System.Globalization;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// The Assembly table is defined using the .assembly directive (�II.6.2);
	/// its columns are obtained from the respective .hash algorithm, .ver, .publickey, and .culture (�II.6.2.1).
	/// </summary>
	[DefaultProperty("Name")]
	public class AssemblyRow : BaseMetaRow
	{
		/// <summary>Specifies all the hash algorithms used for hashing files and for generating the strong name.</summary>
		public AssemblyHashAlgorithm HashAlgId { get { return (AssemblyHashAlgorithm)base.GetValue<UInt32>(0); } }

		/// <summary>Assembly major version</summary>
		internal UInt16 MajorVersion { get { return base.GetValue<UInt16>(1); } }

		/// <summary>Assembly minor version</summary>
		internal UInt16 MinorVersion { get { return base.GetValue<UInt16>(2); } }

		/// <summary>Assembly build number</summary>
		internal UInt16 BuildNumber { get { return base.GetValue<UInt16>(3); } }

		/// <summary>Assembly revision number</summary>
		internal UInt16 RevisionNumber { get { return base.GetValue<UInt16>(4); } }

		/// <summary>MetaData applied to an assembly compilation</summary>
		public AssemblyNameFlags Flags { get { return (AssemblyNameFlags)base.GetValue<UInt32>(5); } }

		/// <summary>Public key or token</summary>
		public Byte[] PublicKey { get { return base.GetValue<Byte[]>(6); } }

		/// <summary>Assembly name</summary>
		public String Name { get { return base.GetValue<String>(7); } }

		/// <summary>Assembly locale</summary>
		public String Locale { get { return base.GetValue<String>(8); } }

		/// <summary>Assembly version</summary>
		public Version Version { get { return new Version(this.MajorVersion, this.MinorVersion, this.BuildNumber, this.RevisionNumber); } }

		/// <summary>Managed assembly name</summary>
		public AssemblyName AssemblyName
		{
			get
			{
				AssemblyName result = new AssemblyName()
				{
					Name = this.Name,
					Version = this.Version,
					Flags = this.Flags,
					CultureInfo = this.Locale == null || this.Locale.Length == 0
						? CultureInfo.InvariantCulture
						: new CultureInfo(this.Locale),
					HashAlgorithm = this.HashAlgId,
				};
				result.SetPublicKey(this.PublicKey);
				return result;
			}
		}
	}
}