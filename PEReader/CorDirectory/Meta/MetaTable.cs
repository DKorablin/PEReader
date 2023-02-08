using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>MetaTable class</summary>
	[DefaultProperty("TableType")]
	public class MetaTable : ITable, ISectionData
	{
		#region Fields
		private readonly StreamTables _root;
		private readonly Cor.MetaTableType _tableType;
		/// <summary>Размер ряда с данными</summary>
		private UInt32? _rowSize;
		/// <summary>Отступ от начала блока с таблицами</summary>
		private readonly UInt32 _padding;
		private readonly MetaColumn[] _columns;

		private MetaRow[] _rows;
		private Byte[] _data;
		#endregion Fields

		internal StreamTables Root { get { return this._root; } }

		/// <summary>Массив колонок в таблице</summary>
		public MetaColumn[] Columns { get { return this._columns; } }
		IColumn[] ITable.Columns { get { return this._columns; } }

		/// <summary>Массив рядов в таблице</summary>
		private MetaRow[] RowsI
		{
			get
			{
				return this._rows == null
					? this._rows = new MetaRow[this.Root.GetRowsCount(this.TableType)]
					: this._rows;
			}
		}
		IEnumerable<IRow> ITable.Rows { get { return this.RowsI; } }

		/// <summary>Кол-во рядов в таблице</summary>
		public UInt32 RowsCount { get { return (UInt32)this.RowsI.Length; } }

		/// <summary>Размер ряда</summary>
		private UInt32 RowSize
		{
			get
			{
				return (this._rowSize == null
					? this._rowSize = this.SizeOfColumns()
					: this._rowSize).Value;
			}
		}
		/// <summary>Тип таблицы</summary>
		public Cor.MetaTableType TableType { get { return this._tableType; } }
		Object ITable.Type { get { return this._tableType; } }

		/// <summary>Размер всей таблицы с данными</summary>
		public UInt32 TableSize { get { return this.RowSize * (UInt32)this.RowsI.Length; } }

		/// <summary>Rows in table</summary>
		public MetaRowCollection Rows { get { return new MetaRowCollection(this); } }

		/// <summary>Получить ряд из таблицы</summary>
		/// <param name="rowIndex">Индекс ряда в таблице</param>
		/// <exception cref="T:ArgumentOutOfRangeException">rowIndex out of range of table rows</exception>
		/// <returns>Ряд с данными</returns>
		public MetaRow this[UInt32 rowIndex]
		{
			get
			{
				if(this._rows.Length <= rowIndex)
					throw new ArgumentOutOfRangeException(nameof(rowIndex));

				if(this._rows[rowIndex] == null)
					this._rows[rowIndex] = this.GetRow(rowIndex);
				return this._rows[rowIndex];
			}
		}
		IRow ITable.this[UInt32 rowIndex] { get { return this[rowIndex]; } }

		/// <summary>Создание экземпляра класса мета-таблицы</summary>
		/// <param name="root">Корневой поток</param>
		/// <param name="tableType">Тип таблицы</param>
		/// <param name="padding">Отступ от начала потома метаданных</param>
		/// <exception cref="T:ArgumentNullException">root is null</exception>
		public MetaTable(StreamTables root, Cor.MetaTableType tableType, UInt32 padding)
		{
			this._root = root ?? throw new ArgumentNullException(nameof(root));
			this._tableType = tableType;
			this._padding = padding;

			this._columns = MetaTable.GetTableDescription(this.TableType);
		}

		/// <summary>Gets section data</summary>
		/// <returns>Byte array</returns>
		public Byte[] GetData()
		{
			return this._data == null
				? this._data = this.Root.Parent.Parent.Parent.Header.ReadBytes(this.Root.DataPosition + this._padding, this.TableSize)
				: this._data;
		}

		private MetaRow GetRow(UInt32 rowIndex)
		{
			using(MemoryStream stream = new MemoryStream(this.GetData()))
			using(BinaryReader reader = new BinaryReader(stream))
			{
				reader.BaseStream.Position = this.RowSize * rowIndex;

				MetaCell[] cells = new MetaCell[this._columns.Length];
				for(Int32 loop = 0;loop < this._columns.Length;loop++)
					cells[loop] = new MetaCell(this, this._columns[loop], rowIndex, reader);

				return new MetaRow(this, rowIndex, cells);
			}
		}

		/// <summary>Получить размер всех колонок в таблице</summary>
		/// <returns>Размер ряда в таблице</returns>
		private UInt32 SizeOfColumns()
		{
			UInt32 result = 0;
			foreach(MetaColumn column in this.Columns)
				result += this.SizeOfColumn(column.ColumnType);
			return result;
		}

		/// <summary>Получить размер колонки в таблице</summary>
		/// <param name="type">Тип колонки</param>
		/// <returns>Размер колонки</returns>
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
					if(MetaColumn.IsColumnCellPointer(type))//Указатель на другую таблицу
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
				return new MetaColumnType[] { MetaColumnType.MethodDef, MetaColumnType.Field, MetaColumnType.TypeRef, MetaColumnType.TypeDef, MetaColumnType.Param, MetaColumnType.InterfaceImpl, MetaColumnType.MemberRef, MetaColumnType.Module, MetaColumnType.DeclSecurity, MetaColumnType.Property, MetaColumnType.Event, MetaColumnType.StandAloneSig, MetaColumnType.ModuleRef, MetaColumnType.TypeSpec, MetaColumnType.Assembly, MetaColumnType.AssemblyRef, MetaColumnType.File, MetaColumnType.ExportedType, MetaColumnType.ManifestResource, /*MetaColumnType.GenericParam, MetaColumnType.GenericParamConstraint, MetaColumnType.MethodSpec,*/ };
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

		/// <summary>Получить описатель таблицы</summary>
		/// <param name="table">Таблица для получения колонок</param>
		/// <exception cref="T:NotSupportedException">Unknown table</exception>
		/// <exception cref="T:InvalidOperationException">Column names not equal column types</exception>
		/// <returns>Массив колонок в таблице</returns>
		private static MetaColumn[] GetTableDescription(Cor.MetaTableType table)
		{
			MetaColumnType[] columnType;
			String[] columnName;
			switch(table)
			{
			case Cor.MetaTableType.Module:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Guid, MetaColumnType.Guid, MetaColumnType.Guid, };
				columnName = new String[] { "Generation", "Name", "Mvid", "EncId", "EncBaseId", };
				break;
			case Cor.MetaTableType.TypeRef:
				columnType = new MetaColumnType[] { MetaColumnType.ResolutionScope, MetaColumnType.String, MetaColumnType.String, };
				columnName = new String[] { "ResolutionScope", "TypeName", "TypeNamespace" };
				break;
			case Cor.MetaTableType.TypeDef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.String, MetaColumnType.TypeDefOrRef, MetaColumnType.Field, MetaColumnType.MethodDef, };
				columnName = new String[] { "Flags", "TypeName", "TypeNamespace", "Extends", "FieldList", "MethodList", };
				break;
			case Cor.MetaTableType.FieldPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Field, };
				columnName = new String[] { "Field" };
				break;
			case Cor.MetaTableType.Field:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { "Flags", "Name", "Signature", };
				break;
			case Cor.MetaTableType.MethodPtr:
				columnType = new MetaColumnType[] { MetaColumnType.MethodDef, };
				columnName = new String[] { "Method", };
				break;
			case Cor.MetaTableType.MethodDef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, MetaColumnType.Param, };
				columnName = new String[] { "RVA", "ImplFlags", "Flags", "Name", "Signature", "ParamList", };
				break;
			case Cor.MetaTableType.ParamPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Param, };
				columnName = new String[] { "Param", };
				break;
			case Cor.MetaTableType.Param:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.String, };
				columnName = new String[] { "Flags", "Sequence", "Name", };
				break;
			case Cor.MetaTableType.InterfaceImpl:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { "Class", "Interface", };
				break;
			case Cor.MetaTableType.MemberRef:
				columnType = new MetaColumnType[] { MetaColumnType.MemberRefParent, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { "Class", "Name", "Signature", };
				break;
			case Cor.MetaTableType.Constant:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.HasConstant, MetaColumnType.Blob, };
				columnName = new String[] { "Type", "Parent", "Value", };
				break;
			case Cor.MetaTableType.CustomAttribute:
				columnType = new MetaColumnType[] { MetaColumnType.HasCustomAttribute, MetaColumnType.CustomAttributeType, MetaColumnType.Blob, };
				columnName = new String[] { "Parent", "Type", "Value", };
				break;
			case Cor.MetaTableType.FieldMarshal:
				columnType = new MetaColumnType[] { MetaColumnType.HasFieldMarshal, MetaColumnType.Blob, };
				columnName = new String[] { "Parent", "Native", };
				break;
			case Cor.MetaTableType.DeclSecurity:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.HasDeclSecurity, MetaColumnType.Blob, };
				columnName = new String[] { "Action", "Parent", "PermissionSet", };
				break;
			case Cor.MetaTableType.ClassLayout:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.TypeDef, };
				columnName = new String[] { "PackingSize", "ClassSize", "Parent", };
				break;
			case Cor.MetaTableType.FieldLayout:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.Field, };
				columnName = new String[] { "Offset", "Field", };
				break;
			case Cor.MetaTableType.StandAloneSig:
				columnType = new MetaColumnType[] { MetaColumnType.Blob, };
				columnName = new String[] { "Signature", };
				break;
			case Cor.MetaTableType.EventMap:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.Event, };
				columnName = new String[] { "Parent", "EventList", };
				break;
			case Cor.MetaTableType.EventPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Event, };
				columnName = new String[] { "Event", };
				break;
			case Cor.MetaTableType.Event:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { "EventFlags", "Name", "EventType", };
				break;
			case Cor.MetaTableType.PropertyMap:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.Property, };
				columnName = new String[] { "Parent", "PropertyList", };
				break;
			case Cor.MetaTableType.PropertyPtr:
				columnType = new MetaColumnType[] { MetaColumnType.Property, };
				columnName = new String[] { "Property", };
				break;
			case Cor.MetaTableType.Property:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { "Flags", "Name", "Type", };
				break;
			case Cor.MetaTableType.MethodSemantics:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.MethodDef, MetaColumnType.HasSemantic, };
				columnName = new String[] { "Semantic", "Method", "Association", };
				break;
			case Cor.MetaTableType.MethodImpl:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.MethodDefOrRef, MetaColumnType.MethodDefOrRef, };
				columnName = new String[] { "Class", "MethodBody", "MethodDeclaration", };
				break;
			case Cor.MetaTableType.ModuleRef:
				columnType = new MetaColumnType[] { MetaColumnType.String, };
				columnName = new String[] { "Name", };
				break;
			case Cor.MetaTableType.TypeSpec:
				columnType = new MetaColumnType[] { MetaColumnType.Blob, };
				columnName = new String[] { "Signature", };
				break;
			case Cor.MetaTableType.ImplMap:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.MemberForwarded, MetaColumnType.String, MetaColumnType.ModuleRef, };
				columnName = new String[] { "MappingFlags", "MemberForwarded", "ImportName", "ImportScope", };
				break;
			case Cor.MetaTableType.FieldRVA:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.Field, };
				columnName = new String[] { "RVA", "Field", };
				break;
			case Cor.MetaTableType.ENCLog:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, };
				columnName = new String[] { "Token", "FuncCode", };
				break;
			case Cor.MetaTableType.ENCMap:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, };
				columnName = new String[] { "Token", };
				break;
			case Cor.MetaTableType.Assembly:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.Blob, MetaColumnType.String, MetaColumnType.String, };
				columnName = new String[] { "HashAlgId", "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKey", "Name", "Locale", };
				break;
			case Cor.MetaTableType.AssemblyProcessor:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, };
				columnName = new String[] { "Processor", };
				break;
			case Cor.MetaTableType.AssemblyOS:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.UInt32, };
				columnName = new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", };
				break;
			case Cor.MetaTableType.AssemblyRef:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.UInt32, MetaColumnType.Blob, MetaColumnType.String, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKeyOrToken", "Name", "Locale", "HashValue", };
				break;
			case Cor.MetaTableType.AssemblyRefProcessor:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.AssemblyRef, };
				columnName = new String[] { "Processor", "AssemblyRef", };
				break;
			case Cor.MetaTableType.AssemblyRefOS:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.AssemblyRef, };
				columnName = new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", "AssemblyRef", };
				break;
			case Cor.MetaTableType.File:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.Blob, };
				columnName = new String[] { "Flags", "Name", "HashValue", };
				break;
			case Cor.MetaTableType.ExportedType:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.String, MetaColumnType.Implementation, };
				columnName = new String[] { "Flags", "TypeDefId", "TypeName", "TypeNamespace", "Implementation", };
				break;
			case Cor.MetaTableType.ManifestResource:
				columnType = new MetaColumnType[] { MetaColumnType.UInt32, MetaColumnType.UInt32, MetaColumnType.String, MetaColumnType.Implementation, };
				columnName = new String[] { "Offset", "Flags", "Name", "Implementation", };
				break;
			case Cor.MetaTableType.NestedClass:
				columnType = new MetaColumnType[] { MetaColumnType.TypeDef, MetaColumnType.TypeDef, };
				columnName = new String[] { "NestedClass", "EnclosingClass", };
				break;
			case Cor.MetaTableType.GenericParam:
				columnType = new MetaColumnType[] { MetaColumnType.UInt16, MetaColumnType.UInt16, MetaColumnType.TypeOrMethodDef, MetaColumnType.String, };
				columnName = new String[] { "Number", "Flags", "Owner", "Name", };
				break;
			case Cor.MetaTableType.MethodSpec:
				columnType = new MetaColumnType[] { MetaColumnType.MethodDefOrRef, MetaColumnType.Blob, };
				columnName = new String[] { "Method", "Instantiation", };
				break;
			case Cor.MetaTableType.GenericParamConstraint:
				columnType = new MetaColumnType[] { MetaColumnType.GenericParam, MetaColumnType.TypeDefOrRef, };
				columnName = new String[] { "Owner", "Constraint", };
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