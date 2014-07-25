namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaData column type</summary>
	public enum MetaColumnType : int
	{
		/// <summary>Module descriptor.</summary>
		Module = 0,
		/// <summary>Class reference descriptors.</summary>
		TypeRef = 1,
		/// <summary>Class or interface definition descriptors.</summary>
		TypeDef = 2,
		/// <summary>A field definition descriptos.</summary>
		Field = 4,
		/// <summary>Method definition descriptors.</summary>
		MethodDef = 6,
		/// <summary>Parameter definition descriptors.</summary>
		Param = 8,
		/// <summary>Interface implementation descriptors.</summary>
		InterfaceImpl = 9,
		/// <summary>Member (field or method) reference descriptors.</summary>
		MemberRef = 10,
		/// <summary>Security descriptors.</summary>
		DeclSecurity = 14,
		/// <summary>Stand-alone signature descriptors. Signatures per se are used in two capacities: as composite signatures of local variables of methods and as parameters of the call indirect (calli) IL instruction.</summary>
		StandAloneSig = 17,
		/// <summary>Event descriptors.</summary>
		Event = 20,
		/// <summary>Property descriptors.</summary>
		Property = 23,
		/// <summary>Module reference descriptors.</summary>
		ModuleRef = 26,
		/// <summary>Type specification descriptors.</summary>
		TypeSpec = 27,
		/// <summary>The current assembly descriptor, whitch sould appear only in the prime moduel metadata.</summary>
		Assembly = 32,
		/// <summary>Assembly reference descriptors.</summary>
		AssemblyRef = 35,
		/// <summary>File descriptors that contain information about other files in the current assembly.</summary>
		File = 38,
		/// <summary>
		/// Exported type descriptors that contain information about public classes exported by the current assembly, whitch are declared in other modules of the assembly.
		/// Only the prime module of the assembly sould carry this table.
		/// </summary>
		ExportedType = 39,
		/// <summary>Managed resource descriptors.</summary>
		ManifestResource = 40,
		/// <summary>Type parameter descriptors for generic (parameterized) classes and methods.</summary>
		GenericParam = 42,
		/// <summary>Generic method instantiation descriptors.</summary>
		MethodSpec = 43,
		/// <summary>Descriptors of constraints specified for type parameters of generic classes and methods.</summary>
		GenericParamConstraint = 44,

		//Coded Token Types
		/// <summary>These items are compact ways to store a TypeDef, TypeRef or TypeSpec token.</summary>
		TypeDefOrRef = 64,
		/// <summary>Field, Param, Property</summary>
		HasConstant = 65,
		/// <summary>MethodDef, MemberRef</summary>
		CustomAttributeType = 66,
		/// <summary>Event, Property</summary>
		HasSemantic = 67,
		/// <summary>Module, ModuleRef, AssemblyRef, TypeRef</summary>
		ResolutionScope = 68,
		/// <summary>Field, Param</summary>
		HasFieldMarshal = 69,
		/// <summary>TypeDef, MethodDef, Assembly</summary>
		HasDeclSecurity = 70,
		/// <summary>TypeDef, TypeRef, ModuleRef, MethodDef, TypeSpec</summary>
		MemberRefParent = 71,
		/// <summary>MethodDef, MemberRef</summary>
		MethodDefOrRef = 72,
		/// <summary>Field, MethodDef</summary>
		MemberForwarded = 73,
		/// <summary>File, AssemblyRef, ExportedType</summary>
		Implementation = 74,
		/// <summary>MethodDef, Field, TypeRef, TypeDef, Param, InterfaceImpl, MemberRef, Module, Permission, Property, Event, StandAloneSig, ModuleRef, TypeSpec, Assembly, AssemblyRef, File, ExportedType, ManifestResource, GenericParam, GenericParamConstraint, MethodSpec</summary>
		HasCustomAttribute = 75,
		/// <summary>TypeDef, MethodDef</summary>
		TypeOrMethodDef = 76,
		//Simple
		/// <summary>UInt16</summary>
		UInt16 = 97,
		/// <summary>UInt32</summary>
		UInt32 = 99,
		/// <summary>String</summary>
		String = 101,
		/// <summary>Blob</summary>
		Blob = 102,
		/// <summary>Guid</summary>
		Guid = 103,
		/// <summary>UserString</summary>
		UserString = 112,
	}
}