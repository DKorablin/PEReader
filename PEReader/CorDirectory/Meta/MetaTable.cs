using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable class</summary>
	[DefaultProperty(nameof(TableType))]
	public class MetaTable : ITable, ISectionData
	{
		#region Fields
		//Size of the row with payload
		private UInt32? _rowSize;
		//Indent from the beginning of the block with tables
		private UInt32 Padding { get; }

		private MetaRow[] _rows;
		private Byte[] _data;
		#endregion Fields

		internal StreamTables Root { get; }

		/// <summary>Array of columns in a table</summary>
		public MetaColumn[] Columns { get; }
		IColumn[] ITable.Columns => this.Columns;

		/// <summary>Array of rows in a table</summary>
		private MetaRow[] RowsI => this._rows ?? (this._rows = new MetaRow[this.Root.GetRowsCount(this.TableType)]);

		IEnumerable<IRow> ITable.Rows => this.RowsI;

		/// <summary>Row count in table</summary>
		public UInt32 RowsCount => (UInt32)this.RowsI.Length;

		/// <summary>Row size</summary>
		private UInt32 RowSize => (this._rowSize ?? (this._rowSize = this.SizeOfColumns())).Value;

		/// <summary>Table type</summary>
		public Cor.MetaTableType TableType { get; }
		Cor.MetaTableType ITable.Type => this.TableType;

		/// <summary>The size of the entire data table</summary>
		public UInt32 TableSize => this.RowSize * (UInt32)this.RowsI.Length;

		/// <summary>Rows in table</summary>
		public MetaRowCollection Rows => new MetaRowCollection(this);

		/// <summary>Get row from table</summary>
		/// <param name="rowIndex">Row index in table</param>
		/// <exception cref="ArgumentOutOfRangeException">rowIndex out of range of table rows</exception>
		/// <returns>Row with payload</returns>
		public MetaRow this[UInt32 rowIndex]
		{
			get
			{
				if(this._rows.Length <= rowIndex)
					throw new ArgumentOutOfRangeException(nameof(rowIndex));

				return this._rows[rowIndex] ?? (this._rows[rowIndex] = this.GetRow(rowIndex));
			}
		}
		IRow ITable.this[UInt32 rowIndex] => this[rowIndex];

		/// <summary>Create metatable instance</summary>
		/// <param name="root">Root stream</param>
		/// <param name="tableType">Table type</param>
		/// <param name="padding">Indent from the start of the metadata stream</param>
		/// <exception cref="ArgumentNullException">root is null</exception>
		public MetaTable(StreamTables root, Cor.MetaTableType tableType, UInt32 padding)
		{
			this.Root = root ?? throw new ArgumentNullException(nameof(root));
			this.TableType = tableType;
			this.Padding = padding;

			this.Columns = MetaTable.GetTableDescription(this.TableType);
		}

		/// <summary>Gets section data</summary>
		/// <returns>Byte array</returns>
		public Byte[] GetData()
			=> this._data
				?? (this._data = this.Root.Parent.Parent.Parent.Header.ReadBytes(this.Root.DataPosition + this.Padding, this.TableSize));

		private MetaRow GetRow(UInt32 rowIndex)
		{
			using(MemoryStream stream = new MemoryStream(this.GetData()))
			using(BinaryReader reader = new BinaryReader(stream))
			{
				reader.BaseStream.Position = this.RowSize * rowIndex;

				MetaCell[] cells = new MetaCell[this.Columns.Length];
				for(Int32 loop = 0;loop < this.Columns.Length;loop++)
					cells[loop] = new MetaCell(this, this.Columns[loop], rowIndex, reader);

				return new MetaRow(this, rowIndex, cells);
			}
		}

		/// <summary>Get the size of all columns in a table</summary>
		/// <returns>Row size in table</returns>
		private UInt32 SizeOfColumns()
		{
			UInt32 result = 0;
			foreach(MetaColumn column in this.Columns)
				result += this.SizeOfColumn(column.ColumnType);
			return result;
		}

		/// <summary>Get the size of a column in a table</summary>
		/// <param name="type">Column type</param>
		/// <returns>Column size</returns>
		internal UInt32 SizeOfColumn(MetaColumnType type)
		{
			switch(type)
			{
				case MetaColumnType.UInt16:
					return 2;
				case MetaColumnType.UInt32:
					return 4;
				case MetaColumnType.String:
					return this.Root.StreamTableHeader.StringIndexSize;
				case MetaColumnType.Blob:
					return this.Root.StreamTableHeader.BlobIndexSize;
				case MetaColumnType.Guid:
					return this.Root.StreamTableHeader.GuidIndexSize;
				default:
					if(MetaColumn.IsColumnCellPointer(type))//Pointer to another table
					return (UInt32)(this.Root.GetRowsCount((Cor.MetaTableType)type) < 65536 ? 2 : 4);

					//CodedToken
					MetaColumnType[] referredTypes = MetaTable.GetCodedTokenTypes(type);
					UInt32 maxRows = 0;
					foreach(MetaColumnType referredType in referredTypes)
						if(referredType != MetaColumnType.UserString)//but what if there is a large user string table?
						{
							UInt32 rows = (UInt32)this.Root.GetRowsCount((Cor.MetaTableType)referredType);
							if(maxRows < rows)
								maxRows = rows;
						}
					maxRows = maxRows << MetaCellCodedToken.CodedTokenBits[referredTypes.Length];
					return (UInt32)(maxRows < 65536 ? 2 : 4);
			}
		}

		internal static MetaColumnType[] GetCodedTokenTypes(MetaColumnType column)
		{
			switch(column)
			{
			case MetaColumnType.TypeDefOrRef:
				return new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeRef, MetaColumnType.TypeSpec, };
			case MetaColumnType.HasConstant:
				return new MetaColumnType[] { MetaColumnType.Field, MetaColumnType.Param, MetaColumnType.Property, };
			case MetaColumnType.HasCustomAttribute:
				return new MetaColumnType[] { MetaColumnType.MethodDef, MetaColumnType.Field, MetaColumnType.TypeRef, MetaColumnType.TypeDef, MetaColumnType.Param, MetaColumnType.InterfaceImpl, MetaColumnType.MemberRef, MetaColumnType.Module, MetaColumnType.DeclSecurity, MetaColumnType.Property, MetaColumnType.Event, MetaColumnType.StandAloneSig, MetaColumnType.ModuleRef, MetaColumnType.TypeSpec, MetaColumnType.Assembly, MetaColumnType.AssemblyRef, MetaColumnType.File, MetaColumnType.ExportedType, MetaColumnType.ManifestResource,
					MetaColumnType.GenericParam, MetaColumnType.GenericParamConstraint, MetaColumnType.MethodSpec, };//NuGet.exe v6.4.0.123 points to column 18 (But I don't see any documentation about this columns that's why I guess that it's linkin to new tables)
			case MetaColumnType.HasFieldMarshal:
				return new MetaColumnType[] { MetaColumnType.Field, MetaColumnType.Param, };
			case MetaColumnType.HasDeclSecurity:
				return new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.MethodDef, MetaColumnType.Assembly, };
			case MetaColumnType.MemberRefParent:
				return new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeRef, MetaColumnType.ModuleRef, MetaColumnType.MethodDef, MetaColumnType.TypeSpec, };
			case MetaColumnType.HasSemantic:
				return new MetaColumnType[] { MetaColumnType.Event, MetaColumnType.Property, };
			case MetaColumnType.MethodDefOrRef:
				return new MetaColumnType[] { MetaColumnType.MethodDef, MetaColumnType.MemberRef, };
			case MetaColumnType.MemberForwarded:
				return new MetaColumnType[] { MetaColumnType.Field, MetaColumnType.MethodDef, };
			case MetaColumnType.Implementation:
				return new MetaColumnType[] { MetaColumnType.File, MetaColumnType.AssemblyRef, MetaColumnType.ExportedType, };
			case MetaColumnType.CustomAttributeType:
				return new MetaColumnType[] { MetaColumnType.TypeRef, MetaColumnType.TypeDef, MetaColumnType.MethodDef, MetaColumnType.MemberRef, MetaColumnType.UserString, };
			case MetaColumnType.ResolutionScope:
				return new MetaColumnType[] { MetaColumnType.Module, MetaColumnType.ModuleRef, MetaColumnType.AssemblyRef, MetaColumnType.TypeRef, };
			case MetaColumnType.TypeOrMethodDef:
				return new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.MethodDef, };
			default: throw new NotSupportedException();
			}
		}

		/// <summary>Get table descriptor</summary>
		/// <param name="table">Table to get columns</param>
		/// <exception cref="NotSupportedException">Unknown table</exception>
		/// <exception cref="InvalidOperationException">Column names not equal column types</exception>
		/// <returns>Array of columns in a table</returns>
		private static MetaColumn[] GetTableDescription(Cor.MetaTableType table)
		{
			MetaColumnType[] columnType;
			String[] columnName;
			switch(table)
			{
			case Cor.MetaTableType.Module:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Guid, MetaColumnType.Guid, MetaColumnType.Guid, };
				columnName = new String[] { nameof(Tables.ModuleRow.Generation), nameof(Tables.ModuleRow.Name), nameof(Tables.ModuleRow.Mvid), nameof(Tables.ModuleRow.EncId), nameof(Tables.ModuleRow.EncBaseId), };
				break;
			case Cor.MetaTableType.TypeRef:
				columnType = new MetaColumnType[] { MetaColumnType.ResolutionScope, MetaColumnType.String, MetaColumnType.String, };
				columnName = new String[] { nameof(Tables.TypeRefRow.ResolutionScope), nameof(Tables.TypeRefRow.TypeName), nameof(Tables.TypeRefRow.TypeNamespace) };
				break;
			case Cor.MetaTableType.TypeDef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.String, MetaColumnType.TypeDefOrRef, MetaColumnType.Field, MetaColumnType.MethodDef, };
				columnName = new String[] { nameof(Tables.TypeDefRow.Flags), nameof(Tables.TypeDefRow.TypeName), nameof(Tables.TypeDefRow.TypeNamespace), nameof(Tables.TypeDefRow.Extends), nameof(Tables.TypeDefRow.FieldList), nameof(Tables.TypeDefRow.MethodList), };
				break;
			case Cor.MetaTableType.FieldPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Field, };
				columnName = new String[] { nameof(Tables.FieldPtrRow.Field) };
				break;
			case Cor.MetaTableType.Field:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.FieldRow.Flags), nameof(Tables.FieldRow.Name), nameof(Tables.FieldRow.Signature), };
				break;
			case Cor.MetaTableType.MethodPtr:
				columnType = new MetaColumnType[] { MetaColumnType.MethodDef, };
				columnName = new String[] { nameof(Tables.MethodPtrRow.Method), };
				break;
			case Cor.MetaTableType.MethodDef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, MetaColumnType.Param, };
				columnName = new String[] { nameof(Tables.MethodDefRow.RVA), nameof(Tables.MethodDefRow.ImplFlags), nameof(Tables.MethodDefRow.Flags), nameof(Tables.MethodDefRow.Name), nameof(Tables.MethodDefRow.Signature), nameof(Tables.MethodDefRow.ParamList), };
				break;
			case Cor.MetaTableType.ParamPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Param, };
				columnName = new String[] { nameof(Tables.ParamPtrRow.Param), };
				break;
			case Cor.MetaTableType.Param:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.String, };
				columnName = new String[] { nameof(Tables.ParamRow.Flags), nameof(Tables.ParamRow.Sequence), nameof(Tables.ParamRow.Name), };
				break;
			case Cor.MetaTableType.InterfaceImpl:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { nameof(Tables.InterfaceImplRow.Class), nameof(Tables.InterfaceImplRow.Interface), };
				break;
			case Cor.MetaTableType.MemberRef:
				columnType = new MetaColumnType[] { MetaColumnType.MemberRefParent, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.MemberRefRow.Class), nameof(Tables.MemberRefRow.Name), nameof(Tables.MemberRefRow.Signature), };
				break;
			case Cor.MetaTableType.Constant:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.HasConstant, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.ConstantRow.Type), nameof(Tables.ConstantRow.Parent), nameof(Tables.ConstantRow.Value), };
				break;
			case Cor.MetaTableType.CustomAttribute:
				columnType = new MetaColumnType[] { MetaColumnType.HasCustomAttribute, MetaColumnType.CustomAttributeType, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.CustomAttributeRow.Parent), nameof(Tables.CustomAttributeRow.Type), nameof(Tables.CustomAttributeRow.Value), };
				break;
			case Cor.MetaTableType.FieldMarshal:
				columnType = new MetaColumnType[] { MetaColumnType.HasFieldMarshal, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.FieldMarshalRow.Parent), nameof(Tables.FieldMarshalRow.Native), };
				break;
			case Cor.MetaTableType.DeclSecurity:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.HasDeclSecurity, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.DeclSecurityRow.Action), nameof(Tables.DeclSecurityRow.Parent), nameof(Tables.DeclSecurityRow.PermissionSet), };
				break;
			case Cor.MetaTableType.ClassLayout:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.TypeDef, };
				columnName = new String[] { nameof(Tables.ClassLayoutRow.PackingSize), nameof(Tables.ClassLayoutRow.ClassSize), nameof(Tables.ClassLayoutRow.Parent), };
				break;
			case Cor.MetaTableType.FieldLayout:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.Field, };
				columnName = new String[] { nameof(Tables.FieldLayoutRow.Offset), nameof(Tables.FieldLayoutRow.Field), };
				break;
			case Cor.MetaTableType.StandAloneSig:
				columnType = new MetaColumnType[] { MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.StandAloneSigRow.Signature), };
				break;
			case Cor.MetaTableType.EventMap:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.Event, };
				columnName = new String[] { nameof(Tables.EventMapRow.Parent), nameof(Tables.EventMapRow.EventList), };
				break;
			case Cor.MetaTableType.EventPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Event, };
				columnName = new String[] { nameof(Tables.EventPtrRow.Event), };
				break;
			case Cor.MetaTableType.Event:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { nameof(Tables.EventRow.EventFlags), nameof(Tables.EventRow.Name), nameof(Tables.EventRow.EventType), };
				break;
			case Cor.MetaTableType.PropertyMap:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.Property, };
				columnName = new String[] { nameof(Tables.PropertyMapRow.Parent), nameof(Tables.PropertyMapRow.PropertyList), };
				break;
			case Cor.MetaTableType.PropertyPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Property, };
				columnName = new String[] { nameof(Tables.PropertyPtrRow.Property), };
				break;
			case Cor.MetaTableType.Property:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.PropertyRow.Flags), nameof(Tables.PropertyRow.Name), nameof(Tables.PropertyRow.Type), };
				break;
			case Cor.MetaTableType.MethodSemantics:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.MethodDef, MetaColumnType.HasSemantic, };
				columnName = new String[] { nameof(Tables.MethodSemanticsRow.Semantic), nameof(Tables.MethodSemanticsRow.Method), nameof(Tables.MethodSemanticsRow.Association), };
				break;
			case Cor.MetaTableType.MethodImpl:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.MethodDefOrRef, MetaColumnType.MethodDefOrRef, };
				columnName = new String[] { nameof(Tables.MethodImplRow.Class), nameof(Tables.MethodImplRow.MethodBody), nameof(Tables.MethodImplRow.MethodDeclaration), };
				break;
			case Cor.MetaTableType.ModuleRef:
				columnType = new MetaColumnType[] { MetaColumnType.String, };
				columnName = new String[] { nameof(Tables.ModuleRefRow.Name), };
				break;
			case Cor.MetaTableType.TypeSpec:
				columnType = new MetaColumnType[] { MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.TypeSpecRow.Signature), };
				break;
			case Cor.MetaTableType.ImplMap:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.MemberForwarded, MetaColumnType.String, MetaColumnType.ModuleRef, };
				columnName = new String[] { nameof(Tables.ImplMapRow.MappingFlags), nameof(Tables.ImplMapRow.MemberForwarded), nameof(Tables.ImplMapRow.ImportName), nameof(Tables.ImplMapRow.ImportScope), };
				break;
			case Cor.MetaTableType.FieldRVA:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.Field, };
				columnName = new String[] { nameof(Tables.FieldRVARow.RVA), nameof(Tables.FieldRVARow.Field), };
				break;
			case Cor.MetaTableType.ENCLog:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, };
				columnName = new String[] { nameof(Tables.ENCLogRow.Token), nameof(Tables.ENCLogRow.FuncCode), };
				break;
			case Cor.MetaTableType.ENCMap:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, };
				columnName = new String[] { nameof(Tables.ENCMapRow.Token), };
				break;
			case Cor.MetaTableType.Assembly:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.Blob, MetaColumnType.String, MetaColumnType.String, };
				columnName = new String[] { nameof(Tables.AssemblyRow.HashAlgId), nameof(Tables.AssemblyRow.MajorVersion), nameof(Tables.AssemblyRow.MinorVersion), nameof(Tables.AssemblyRow.BuildNumber), nameof(Tables.AssemblyRow.RevisionNumber), nameof(Tables.AssemblyRow.Flags), nameof(Tables.AssemblyRow.PublicKey), nameof(Tables.AssemblyRow.Name), nameof(Tables.AssemblyRow.Locale), };
				break;
			case Cor.MetaTableType.AssemblyProcessor:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, };
				columnName = new String[] { nameof(Tables.AssemblyProcessorRow.Processor), };
				break;
			case Cor.MetaTableType.AssemblyOS:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.UInt32, };
				columnName = new String[] { nameof(Tables.AssemblyOSRow.OSPlatformId), nameof(Tables.AssemblyOSRow.OSMajorVersion), nameof(Tables.AssemblyOSRow.OSMinorVersion), };
				break;
			case Cor.MetaTableType.AssemblyRef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.Blob, MetaColumnType.String, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.AssemblyRefRow.MajorVersion), nameof(Tables.AssemblyRefRow.MinorVersion), nameof(Tables.AssemblyRefRow.BuildNumber), nameof(Tables.AssemblyRefRow.RevisionNumber), nameof(Tables.AssemblyRefRow.Flags), nameof(Tables.AssemblyRefRow.PublicKeyOrToken), nameof(Tables.AssemblyRefRow.Name), nameof(Tables.AssemblyRefRow.Locale), nameof(Tables.AssemblyRefRow.HashValue), };
				break;
			case Cor.MetaTableType.AssemblyRefProcessor:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.AssemblyRef, };
				columnName = new String[] { nameof(Tables.AssemblyRefProcessorRow.Processor), nameof(Tables.AssemblyRefProcessorRow.AssemblyRef), };
				break;
			case Cor.MetaTableType.AssemblyRefOS:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.AssemblyRef, };
				columnName = new String[] { nameof(Tables.AssemblyRefOSRow.OSPlatformId), nameof(Tables.AssemblyRefOSRow.OSMajorVersion), nameof(Tables.AssemblyRefOSRow.OSMinorVersion), nameof(Tables.AssemblyRefOSRow.AssemblyRef), };
				break;
			case Cor.MetaTableType.File:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.FileRow.Flags), nameof(Tables.FileRow.Name), nameof(Tables.FileRow.HashValue), };
				break;
			case Cor.MetaTableType.ExportedType:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.String, MetaColumnType.Implementation, };
				columnName = new String[] { nameof(Tables.ExportedTypeRow.Flags), nameof(Tables.ExportedTypeRow.TypeDefId), nameof(Tables.ExportedTypeRow.TypeName), nameof(Tables.ExportedTypeRow.TypeNamespace), nameof(Tables.ExportedTypeRow.Implementation), };
				break;
			case Cor.MetaTableType.ManifestResource:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.Implementation, };
				columnName = new String[] { nameof(Tables.ManifestResourceRow.Offset), nameof(Tables.ManifestResourceRow.Flags), nameof(Tables.ManifestResourceRow.Name), nameof(Tables.ManifestResourceRow.Implementation), };
				break;
			case Cor.MetaTableType.NestedClass:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeDef, };
				columnName = new String[] { nameof(Tables.NestedClassRow.NestedClass), nameof(Tables.NestedClassRow.EnclosingClass), };
				break;
			case Cor.MetaTableType.GenericParam:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.TypeOrMethodDef, MetaColumnType.String, };
				columnName = new String[] { nameof(Tables.GenericParamRow.Number), nameof(Tables.GenericParamRow.Flags), nameof(Tables.GenericParamRow.Owner), nameof(Tables.GenericParamRow.Name), };
				break;
			case Cor.MetaTableType.MethodSpec:
				columnType = new MetaColumnType[] { MetaColumnType.MethodDefOrRef, MetaColumnType.Blob, };
				columnName = new String[] { nameof(Tables.MethodSpecRow.Method), nameof(Tables.MethodSpecRow.Instantiation), };
				break;
			case Cor.MetaTableType.GenericParamConstraint:
				columnType = new MetaColumnType[] { MetaColumnType.GenericParam, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { nameof(Tables.GenericParamConstraintRow.Owner), nameof(Tables.GenericParamConstraintRow.Constraint), };
				break;
			default: throw new NotSupportedException();
			}

			if(columnType.Length != columnName.Length)
				throw new InvalidOperationException("Length of column type and names must be equal");

			MetaColumn[] result = new MetaColumn[columnType.Length];
			for(UInt16 loop = 0; loop < result.Length; loop++)
				result[loop] = new MetaColumn(table, columnType[loop], columnName[loop], loop);

			return result;
		}
	}
}