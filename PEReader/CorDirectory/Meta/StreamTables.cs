using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaData tables</summary>
	public class StreamTables : StreamHeader
	{
		private UInt32[] _rowsCount;
		private Cor.STREAM_TABLE_HEADER? _streamTableHeader;
		private Dictionary<Cor.MetaTableType, MetaTable> _tables;
		/// <summary>Таблица с информацией о типе</summary>
		/// <param name="tableType">Тип таблицы</param>
		/// <returns>Данные в таблице</returns>
		public MetaTable this[Cor.MetaTableType tableType]
		{
			get
			{
				if(this._tables == null)
					DataBind();
				MetaTable result;
				if(this._tables.TryGetValue(tableType, out result))
					return result;
				else
					return null;
			}
		}
		#region Tables
		/// <summary>Module descriptor.</summary>
		public Tables.BaseMetaTable<Tables.ModuleRow> Module
		{
			get { return new Tables.BaseMetaTable<Tables.ModuleRow>(this, Cor.MetaTableType.Module); }
		}
		/// <summary>Class reference descriptors.</summary>
		public Tables.BaseMetaTable<Tables.TypeRefRow> TypeRef
		{
			get { return new Tables.BaseMetaTable<Tables.TypeRefRow>(this, Cor.MetaTableType.TypeRef); }
		}
		/// <summary>Class or interface definition descriptors.</summary>
		public Tables.BaseMetaTable<Tables.TypeDefRow> TypeDef
		{
			get { return new Tables.BaseMetaTable<Tables.TypeDefRow>(this, Cor.MetaTableType.TypeDef); }
		}
		/// <summary>A class-to-fields lookup table, whitch does not exist on optimized metadata (#~ stream).</summary>
		public Tables.BaseMetaTable<Tables.FieldPtrRow> FieldPtr
		{
			get { return new Tables.BaseMetaTable<Tables.FieldPtrRow>(this, Cor.MetaTableType.FieldPtr); }
		}
		/// <summary>A field definition descriptos.</summary>
		public Tables.BaseMetaTable<Tables.FieldRow> Field
		{
			get { return new Tables.BaseMetaTable<Tables.FieldRow>(this, Cor.MetaTableType.Field); }
		}
		/// <summary>A class-to-methods lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
		public Tables.BaseMetaTable<Tables.MethodPtrRow> MethodPtr
		{
			get { return new Tables.BaseMetaTable<Tables.MethodPtrRow>(this, Cor.MetaTableType.MethodPtr); }
		}
		/// <summary>Method definition descriptors.</summary>
		public Tables.BaseMetaTable<Tables.MethodDefRow> MethodDef
		{
			get { return new Tables.BaseMetaTable<Tables.MethodDefRow>(this, Cor.MetaTableType.MethodDef); }
		}
		/// <summary>A method-to-parameters lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
		public Tables.BaseMetaTable<Tables.ParamPtrRow> ParamPtr
		{
			get { return new Tables.BaseMetaTable<Tables.ParamPtrRow>(this, Cor.MetaTableType.ParamPtr); }
		}
		/// <summary>Parameter definition descriptors.</summary>
		public Tables.BaseMetaTable<Tables.ParamRow> Param
		{
			get { return new Tables.BaseMetaTable<Tables.ParamRow>(this, Cor.MetaTableType.Param); }
		}
		/// <summary>Interface implementation descriptors.</summary>
		public Tables.BaseMetaTable<Tables.InterfaceImplRow> InterfaceImpl
		{
			get { return new Tables.BaseMetaTable<Tables.InterfaceImplRow>(this, Cor.MetaTableType.InterfaceImpl); }
		}
		/// <summary>Member (field or method) reference descriptors.</summary>
		public Tables.BaseMetaTable<Tables.MemberRefRow> MemberRef
		{
			get { return new Tables.BaseMetaTable<Tables.MemberRefRow>(this, Cor.MetaTableType.MemberRef); }
		}
		/// <summary>Constant value descriptors that map the default values stored in the #Blob stream to respective fields, parameters, and properties.</summary>
		public Tables.BaseMetaTable<Tables.ConstantRow> Constant
		{
			get { return new Tables.BaseMetaTable<Tables.ConstantRow>(this, Cor.MetaTableType.Constant); }
		}
		/// <summary>Custom attribute descriptors.</summary>
		public Tables.BaseMetaTable<Tables.CustomAttributeRow> CustomAttribute
		{
			get { return new Tables.BaseMetaTable<Tables.CustomAttributeRow>(this, Cor.MetaTableType.CustomAttribute); }
		}
		/// <summary>Field or parameter marshaling descriptors for managed/unmanaged interoperations.</summary>
		public Tables.BaseMetaTable<Tables.FieldMarshalRow> FieldMarshal
		{
			get { return new Tables.BaseMetaTable<Tables.FieldMarshalRow>(this, Cor.MetaTableType.FieldMarshal); }
		}
		/// <summary>Security descriptors.</summary>
		public Tables.BaseMetaTable<Tables.DeclSecurityRow> DeclSecurity
		{
			get { return new Tables.BaseMetaTable<Tables.DeclSecurityRow>(this, Cor.MetaTableType.DeclSecurity); }
		}
		/// <summary>Class layout descriptors that hold information about how the loader should lay out respective classes.</summary>
		public Tables.BaseMetaTable<Tables.ClassLayoutRow> ClassLayout
		{
			get { return new Tables.BaseMetaTable<Tables.ClassLayoutRow>(this, Cor.MetaTableType.ClassLayout); }
		}
		/// <summary>Field layout descriptors that specify the offset or ordinal of invidual fields.</summary>
		public Tables.BaseMetaTable<Tables.FieldLayoutRow> FieldLayout
		{
			get { return new Tables.BaseMetaTable<Tables.FieldLayoutRow>(this, Cor.MetaTableType.FieldLayout); }
		}
		/// <summary>Stand-alone signature descriptors. Signatures per se are used in two capacities: as composite signatures of local variables of methods and as parameters of the call indirect (calli) IL instruction.</summary>
		public Tables.BaseMetaTable<Tables.StandAloneSigRow> StandAloneSig
		{
			get { return new Tables.BaseMetaTable<Tables.StandAloneSigRow>(this, Cor.MetaTableType.StandAloneSig); }
		}
		/// <summary>
		/// A class-to-events mapping table.
		/// This is not an intermidate lookup table, and it does not exist in optimized metadata.
		/// </summary>
		public Tables.BaseMetaTable<Tables.EventMapRow> EventMap
		{
			get { return new Tables.BaseMetaTable<Tables.EventMapRow>(this, Cor.MetaTableType.EventMap); }
		}
		/// <summary>An event map-to-events lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
		public Tables.BaseMetaTable<Tables.EventPtrRow> EventPtr
		{
			get { return new Tables.BaseMetaTable<Tables.EventPtrRow>(this, Cor.MetaTableType.EventPtr); }
		}
		/// <summary>Event descriptors.</summary>
		public Tables.BaseMetaTable<Tables.EventRow> Event
		{
			get { return new Tables.BaseMetaTable<Tables.EventRow>(this, Cor.MetaTableType.Event); }
		}
		/// <summary>
		/// A class-to-properties mapping table.
		/// This is not an intermidate lookup table, and it does not exist in optimized metadata.
		/// </summary>
		public Tables.BaseMetaTable<Tables.PropertyMapRow> PropertyMap
		{
			get { return new Tables.BaseMetaTable<Tables.PropertyMapRow>(this, Cor.MetaTableType.PropertyMap); }
		}
		/// <summary>A property map-to-properties lookup table, whitch does not exists on optimized metadata (#~ stream).</summary>
		public Tables.BaseMetaTable<Tables.PropertyPtrRow> PropertyPtr
		{
			get { return new Tables.BaseMetaTable<Tables.PropertyPtrRow>(this, Cor.MetaTableType.PropertyPtr); }
		}
		/// <summary>Property descriptors.</summary>
		public Tables.BaseMetaTable<Tables.PropertyRow> Property
		{
			get { return new Tables.BaseMetaTable<Tables.PropertyRow>(this, Cor.MetaTableType.Property); }
		}
		/// <summary>Method semantics descriptors that hold information about whitch method is associated with a specific property or event and in what capacity.</summary>
		public Tables.BaseMetaTable<Tables.MethodSemanticsRow> MethodSemantics
		{
			get { return new Tables.BaseMetaTable<Tables.MethodSemanticsRow>(this, Cor.MetaTableType.MethodSemantics); }
		}
		/// <summary>Method implementation descriptors.</summary>
		public Tables.BaseMetaTable<Tables.MethodImplRow> MethodImpl
		{
			get { return new Tables.BaseMetaTable<Tables.MethodImplRow>(this, Cor.MetaTableType.MethodImpl); }
		}
		/// <summary>Module reference descriptors.</summary>
		public Tables.BaseMetaTable<Tables.ModuleRefRow> ModuleRef
		{
			get { return new Tables.BaseMetaTable<Tables.ModuleRefRow>(this, Cor.MetaTableType.ModuleRef); }
		}
		/// <summary>
		/// The TypeSpec table has just one column, which indexes the specification of a Type, stored in the Blob heap.
		/// This provides a metadata token for that Type (rather than simply an index into the Blob heap).
		/// This is required, typically, for array operations, such as creating, or calling methods on the array class.
		/// </summary>
		/// <remarks>
		/// Note that TypeSpec tokens can be used with any of the CIL instructions that take a TypeDef or TypeRef token;
		/// specifically, castclass, cpobj, initobj, isinst, ldelema, ldobj, mkrefany, newarr, refanyval, sizeof, stobj, box, and unbox.
		/// </remarks>
		public Tables.BaseMetaTable<Tables.TypeSpecRow> TypeSpec
		{
			get { return new Tables.BaseMetaTable<Tables.TypeSpecRow>(this, Cor.MetaTableType.TypeSpec); }
		}
		/// <summary>Implementation map descriptors used for the platform invocation (P/Invoke) type of managed/unmanaged code interoperation.</summary>
		public Tables.BaseMetaTable<Tables.ImplMapRow> ImplMap
		{
			get { return new Tables.BaseMetaTable<Tables.ImplMapRow>(this, Cor.MetaTableType.ImplMap); }
		}
		/// <summary>Field-to-data mapping descriptors.</summary>
		public Tables.BaseMetaTable<Tables.FieldRVARow> FieldRVA
		{
			get { return new Tables.BaseMetaTable<Tables.FieldRVARow>(this, Cor.MetaTableType.FieldRVA); }
		}
		/// <summary>
		/// Edit-and-continue log descriptors that hold information about what changes have been made to specific metadata items during in-memory editing.
		/// This table does not exist in optimized metadata (#~ stream).
		/// </summary>
		public Tables.BaseMetaTable<Tables.ENCLogRow> ENCLog
		{
			get { return new Tables.BaseMetaTable<Tables.ENCLogRow>(this, Cor.MetaTableType.ENCLog); }
		}
		/// <summary>
		/// Edit-and-continue mapping descriptors.
		/// This table does not exist in optimized metadata (#~ stream).
		/// </summary>
		public Tables.BaseMetaTable<Tables.ENCMapRow> ENCMap
		{
			get { return new Tables.BaseMetaTable<Tables.ENCMapRow>(this, Cor.MetaTableType.ENCMap); }
		}
		/// <summary>The current assembly descriptor, whitch sould appear only in the prime moduel metadata.</summary>
		public Tables.BaseMetaTable<Tables.AssemblyRow> Assembly
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyRow>(this, Cor.MetaTableType.Assembly); }
		}
		/// <summary>
		/// These records should not be emitted into any PE file.
		/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
		/// </summary>
		public Tables.BaseMetaTable<Tables.AssemblyProcessorRow> AssemblyProcessor
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyProcessorRow>(this, Cor.MetaTableType.AssemblyProcessor); }
		}
		/// <summary>
		/// These records should not be emitted into any PE file.
		/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
		/// </summary>
		public Tables.BaseMetaTable<Tables.AssemblyOSRow> AssemblyOS
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyOSRow>(this, Cor.MetaTableType.AssemblyOS); }
		}
		/// <summary>Assembly reference descriptors.</summary>
		public Tables.BaseMetaTable<Tables.AssemblyRefRow> AssemblyRef
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyRefRow>(this, Cor.MetaTableType.AssemblyRef); }
		}
		/// <summary>
		/// These records should not be emitted into any PE file.
		/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
		/// </summary>
		public Tables.BaseMetaTable<Tables.AssemblyRefProcessorRow> AssemblyRefProcessor
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyRefProcessorRow>(this, Cor.MetaTableType.AssemblyRefProcessor); }
		}
		/// <summary>
		/// These records should not be emitted into any PE file.
		/// However, if present in a PE file, they should be treated as-if their fields were zero. They should be ignored by the CLI.
		/// </summary>
		public Tables.BaseMetaTable<Tables.AssemblyRefOSRow> AssemblyRefOS
		{
			get { return new Tables.BaseMetaTable<Tables.AssemblyRefOSRow>(this, Cor.MetaTableType.AssemblyRefOS); }
		}
		/// <summary>File descriptors that contain information about other files in the current assembly.</summary>
		public Tables.BaseMetaTable<Tables.FileRow> File
		{
			get { return new Tables.BaseMetaTable<Tables.FileRow>(this, Cor.MetaTableType.File); }
		}
		/// <summary>
		/// Exported type descriptors that contain information about public classes exported by the current assembly, whitch are declared in other modules of the assembly.
		/// Only the prime module of the assembly sould carry this table.
		/// </summary>
		public Tables.BaseMetaTable<Tables.ExportedTypeRow> ExportedType
		{
			get { return new Tables.BaseMetaTable<Tables.ExportedTypeRow>(this, Cor.MetaTableType.ExportedType); }
		}
		/// <summary>Managed resource descriptors.</summary>
		public Tables.BaseMetaTable<Tables.ManifestResourceRow> ManifestResource
		{
			get { return new Tables.BaseMetaTable<Tables.ManifestResourceRow>(this, Cor.MetaTableType.ManifestResource); }
		}
		/// <summary>Nested class descriptors that provide mapping of nested classes to their respective enclosing classes.</summary>
		public Tables.BaseMetaTable<Tables.NestedClassRow> NestedClass
		{
			get { return new Tables.BaseMetaTable<Tables.NestedClassRow>(this, Cor.MetaTableType.NestedClass); }
		}
		/// <summary>Type parameter descriptors for generic (parameterized) classes and methods.</summary>
		public Tables.BaseMetaTable<Tables.GenericParamRow> GenericParam
		{
			get { return new Tables.BaseMetaTable<Tables.GenericParamRow>(this, Cor.MetaTableType.GenericParam); }
		}
		/// <summary>Generic method instantiation descriptors.</summary>
		public Tables.BaseMetaTable<Tables.MethodSpecRow> MethodSpec
		{
			get { return new Tables.BaseMetaTable<Tables.MethodSpecRow>(this, Cor.MetaTableType.MethodSpec); }
		}
		/// <summary>Descriptors of constraints specified for type parameters of generic classes and methods.</summary>
		public Tables.BaseMetaTable<Tables.GenericParamConstraintRow> GenericParamConstraint
		{
			get { return new Tables.BaseMetaTable<Tables.GenericParamConstraintRow>(this, Cor.MetaTableType.GenericParamConstraint); }
		}
		#endregion Tables
		/// <summary>Table header</summary>
		public Cor.STREAM_TABLE_HEADER StreamTableHeader
		{
			get
			{
				if(!this._streamTableHeader.HasValue)
				{
					UInt32 position = base.Parent.Directory.VirtualAddress
							+ base.Header.Offset;
					this._streamTableHeader = base.Parent.Parent.Parent.Header.PtrToStructure<Cor.STREAM_TABLE_HEADER>(position);
				}
				return this._streamTableHeader.Value;
			}
		}
		/// <summary>Tables position</summary>
		public override UInt32 Position { get { return base.Position + (UInt32)Marshal.SizeOf(typeof(Cor.STREAM_TABLE_HEADER)); ; } }
		/// <summary>Позиция, с которых начинаются данные в таблицах</summary>
		public UInt32 DataPosition
		{
			get { return this.Position + this.StreamTableHeader.PresentTablesCount * sizeof(UInt32); }
		}
		/// <summary>Create instance of stream tables class</summary>
		/// <param name="meta">MetaData directory</param>
		/// <param name="header">stream header</param>
		/// <exception cref="T:InvalidOperationException">StreamTable can only read optimized or unoptimized stream tables</exception>
		public StreamTables(MetaData meta, Cor.STREAM_HEADER header)
			: base(meta, header)
		{
			if(header.Type != Cor.StreamHeaderType.StreamTable && header.Type != Cor.StreamHeaderType.StreamTableUnoptimized)
				throw new InvalidOperationException();
		}
		/// <summary>Получить данные всех таблиц</summary>
		private void DataBind()
		{
			this._tables = new Dictionary<Cor.MetaTableType, MetaTable>();
			UInt32 padding = 0;
			this.AddTable(Cor.MetaTableType.Module, ref padding);
			this.AddTable(Cor.MetaTableType.TypeRef, ref padding);
			this.AddTable(Cor.MetaTableType.TypeDef, ref padding);
			this.AddTable(Cor.MetaTableType.FieldPtr, ref padding);
			this.AddTable(Cor.MetaTableType.Field, ref padding);
			this.AddTable(Cor.MetaTableType.MethodPtr, ref padding);
			this.AddTable(Cor.MetaTableType.MethodDef, ref padding);
			this.AddTable(Cor.MetaTableType.ParamPtr, ref padding);
			this.AddTable(Cor.MetaTableType.Param, ref padding);
			this.AddTable(Cor.MetaTableType.InterfaceImpl, ref padding);
			this.AddTable(Cor.MetaTableType.MemberRef, ref padding);
			this.AddTable(Cor.MetaTableType.Constant, ref padding);
			this.AddTable(Cor.MetaTableType.CustomAttribute, ref padding);
			this.AddTable(Cor.MetaTableType.FieldMarshal, ref padding);
			this.AddTable(Cor.MetaTableType.DeclSecurity, ref padding);
			this.AddTable(Cor.MetaTableType.ClassLayout, ref padding);
			this.AddTable(Cor.MetaTableType.FieldLayout, ref padding);
			this.AddTable(Cor.MetaTableType.StandAloneSig, ref padding);
			this.AddTable(Cor.MetaTableType.EventMap, ref padding);
			this.AddTable(Cor.MetaTableType.EventPtr, ref padding);
			this.AddTable(Cor.MetaTableType.Event, ref padding);
			this.AddTable(Cor.MetaTableType.PropertyMap, ref padding);
			this.AddTable(Cor.MetaTableType.PropertyPtr, ref padding);
			this.AddTable(Cor.MetaTableType.Property, ref padding);
			this.AddTable(Cor.MetaTableType.MethodSemantics, ref padding);
			this.AddTable(Cor.MetaTableType.MethodImpl, ref padding);
			this.AddTable(Cor.MetaTableType.ModuleRef, ref padding);
			this.AddTable(Cor.MetaTableType.TypeSpec, ref padding);
			this.AddTable(Cor.MetaTableType.ImplMap, ref padding);
			this.AddTable(Cor.MetaTableType.FieldRVA, ref padding);
			this.AddTable(Cor.MetaTableType.ENCLog, ref padding);
			this.AddTable(Cor.MetaTableType.ENCMap, ref padding);
			this.AddTable(Cor.MetaTableType.Assembly, ref padding);

			this.AddTable(Cor.MetaTableType.AssemblyProcessor, ref padding);
			this.AddTable(Cor.MetaTableType.AssemblyOS, ref padding);
			this.AddTable(Cor.MetaTableType.AssemblyRef, ref padding);
			this.AddTable(Cor.MetaTableType.AssemblyRefProcessor, ref padding);
			this.AddTable(Cor.MetaTableType.AssemblyRefOS, ref padding);
			this.AddTable(Cor.MetaTableType.File, ref padding);
			this.AddTable(Cor.MetaTableType.ExportedType, ref padding);
			this.AddTable(Cor.MetaTableType.ManifestResource, ref padding);
			this.AddTable(Cor.MetaTableType.NestedClass, ref padding);
			this.AddTable(Cor.MetaTableType.GenericParam, ref padding);
			this.AddTable(Cor.MetaTableType.MethodSpec, ref padding);
			this.AddTable(Cor.MetaTableType.GenericParamConstraint, ref padding);
		}
		/// <summary>Создать таблицу мета-данных</summary>
		/// <param name="tableType">Тип создаваемой таблицы</param>
		/// <param name="padding">Отступ от начала потока с мета данными</param>
		private void AddTable(Cor.MetaTableType tableType, ref UInt32 padding)
		{
			MetaTable table = new MetaTable(this, tableType, padding);
			padding += table.TableSize;
			this._tables.Add(tableType, table);
		}
		/// <summary>Получить кол-во рдво во всех таблицах</summary>
		/// <returns>Кол-во рядов во всех таблицах</returns>
		public UInt32[] GetRowsCount()
		{
			if(this._rowsCount == null)
				this._rowsCount = this.GetRowsCountI();
			return this._rowsCount;
		}
		/// <summary>Получить кол-во рядов в определённой таблице</summary>
		/// <param name="tableType">Таблица для которой получить кол-во рядов</param>
		/// <returns>Кол-во рядов в определённой таблице</returns>
		public UInt32 GetRowsCount(Cor.MetaTableType tableType)
		{
			if(this._rowsCount == null)
				this._rowsCount = this.GetRowsCount();
			return this._rowsCount[(Int32)tableType];
		}
		/// <summary>Код получение списка нераспакованных таблиц</summary>
		/// <returns>Список нераспакованных таблиц</returns>
		protected virtual UInt32[] GetRowsCountI()
		{
			var table = this.StreamTableHeader;

			Byte[] bytes = base.Bytes;
			UInt32 position=0;
			UInt32[] result = new UInt32[Cor.STREAM_TABLE_HEADER.TablesCount];
			UInt32 sizeOfUint = sizeof(UInt32);
			for(Int32 loop = 0;loop < Cor.STREAM_TABLE_HEADER.TablesCount;loop++)
			{
				UInt32 count;
				if(table.IsTablePresent(loop))
				{
					count = BitConverter.ToUInt32(bytes, (Int32)position);
					position += sizeOfUint;
				} else count = 0;

				result[loop] = count;
			}
			return result;
		}
	}
}