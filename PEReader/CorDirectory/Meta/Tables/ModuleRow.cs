﻿using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>Module descriptor</summary>
	/// <remarks>
	/// The Generation, EncId, and EncBaseId columns can be written as zero, and can be ignored by conforming implementations of the CLI.
	/// The rows in the Module table result from .module directives in the Assembly (§II.6.4)
	/// </remarks>
	public class ModuleRow : BaseMetaRow
	{
		/// <summary>A 2-byte value, reserved, shall be zero</summary>
		public UInt16 Generation => base.GetValue<UInt16>(0);

		/// <summary>Module name</summary>
		public String Name => base.GetValue<String>(1);

		/// <summary>
		/// The Mvid column shall index a unique GUID in the GUID heap (§II.24.2.5) that identifies this instance of the module.
		/// The Mvid can be ignored on read by conforming implementations of the CLI.
		/// The Mvid should be newly generated for every module, using the algorithm specified in ISO/IEC 11578:1996 (Annex A) or another compatible algorithm
		/// </summary>
		public Guid Mvid => base.GetValue<Guid>(2);

		/// <summary>An index into the Guid heap; reserved, shall be zero</summary>
		public Guid EncId => base.GetValue<Guid>(3);

		/// <summary>An index into the Guid heap; reserved, shall be zero</summary>
		public Guid EncBaseId => base.GetValue<Guid>(4);

		/// <summary>Create instance of module descriptor row</summary>
		public ModuleRow()
			: base(Cor.MetaTableType.Module) { }
	}
}