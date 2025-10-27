using System;
using System.Globalization;
using System.Reflection;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>The table is defined by the .assembly extern directive</summary>
	/// <remarks>
	/// Its columns are filled using directives similar to those of the Assembly table
	/// except for the PublicKeyOrToken column, which is defined using the .publickeytoken directive.
	/// </remarks>
	public class AssemblyRefRow : BaseMetaRow
	{
		/// <summary>Major assembly version</summary>
		public UInt16 MajorVersion => base.GetValue<UInt16>(0);

		/// <summary>Minor assembly version</summary>
		public UInt16 MinorVersion => base.GetValue<UInt16>(1);

		/// <summary>Build number</summary>
		public UInt16 BuildNumber => base.GetValue<UInt16>(2);

		/// <summary>Revision number</summary>
		public UInt16 RevisionNumber => base.GetValue<UInt16>(3);

		/// <summary>Assembly flags</summary>
		/// <remarks>Flags shall have only one bit set, the PublicKey bit (§II.23.1.2). All other bits shall be zero</remarks>
		public AssemblyNameFlags Flags => (AssemblyNameFlags)base.GetValue<UInt32>(4);

		/// <summary>an index into the Blob heap, indicating the public key or token that identifies the author of this Assembly</summary>
		public Byte[] PublicKeyOrToken => base.GetValue<Byte[]>(5);

		/// <summary>Assembly name</summary>
		public String Name => base.GetValue<String>(6);

		/// <summary>Culture</summary>
		public String Locale => base.GetValue<String>(7);

		/// <summary>an index into the Blob heap</summary>
		public Byte[] HashValue => base.GetValue<Byte[]>(8);

		/// <summary>Full assembly version</summary>
		public Version Version => new Version(this.MajorVersion, this.MinorVersion, this.BuildNumber, this.RevisionNumber);

		/// <summary>Describes an assembly's unique identity in full</summary>
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
				};

				switch(this.Flags)
				{
				case AssemblyNameFlags.PublicKey:
					result.SetPublicKey(this.PublicKeyOrToken);
					break;
				default:
					result.SetPublicKeyToken(this.PublicKeyOrToken);
					break;
				}
				
				return result;
			}
		}

		/// <summary>Create instance of assembly reference row</summary>
		public AssemblyRefRow()
			: base(Cor.MetaTableType.AssemblyRef) { }

		/// <summary>Describes an assembly's unique identity in full</summary>
		/// <returns>String representation of the assembly</returns>
		public override String ToString()
			=> this.AssemblyName.ToString();
	}
}