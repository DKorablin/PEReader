using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug
{
	public static class Utils
	{
		public static String ValueToString(Object value)
		{
			if(value == null)
				return "null";
			Type type = value.GetType();
			if(type == typeof(String))
				return "\"" + value.ToString() + "\"";
			else
				return value.ToString();
		}

		public static String ElementTypeToString(ElementType type)
		{
			String sType;
			switch(type.Type)
			{
			case Cor.ELEMENT_TYPE.OBJECT:
				sType = typeof(Object).Name;
				break;
			case Cor.ELEMENT_TYPE.BOOLEAN:
				sType = typeof(Boolean).Name;
				break;
			case Cor.ELEMENT_TYPE.STRING:
				sType = typeof(String).Name;
				break;
			case Cor.ELEMENT_TYPE.I:
				sType = typeof(IntPtr).Name;
				break;
			case Cor.ELEMENT_TYPE.I2:
				sType = typeof(Int16).Name;
				break;
			case Cor.ELEMENT_TYPE.I4:
				sType = typeof(Int32).Name;
				break;
			case Cor.ELEMENT_TYPE.I8:
				sType = typeof(Int64).Name;
				break;
			case Cor.ELEMENT_TYPE.U:
				sType = typeof(UIntPtr).Name;
				break;
			case Cor.ELEMENT_TYPE.U1:
				sType = typeof(Byte).Name;
				break;
			case Cor.ELEMENT_TYPE.U2:
				sType = typeof(UInt16).Name;
				break;
			case Cor.ELEMENT_TYPE.U4:
				sType = typeof(UInt32).Name;
				break;
			case Cor.ELEMENT_TYPE.U8:
				sType = typeof(UInt64).Name;
				break;
			case Cor.ELEMENT_TYPE.VOID:
				sType = typeof(void).Name;
				break;
			case Cor.ELEMENT_TYPE.CLASS:
			case Cor.ELEMENT_TYPE.VALUETYPE:
				switch(type.TypeDefOrRef.TableType)
				{
				case Cor.MetaTableType.TypeDef:
					TypeDefRow typeDef = type.TypeDefOrRef.GetTargetRowTyped<TypeDefRow>();
					sType = typeDef.TypeNamespace == String.Empty
						? typeDef.TypeName
						: String.Join(".", typeDef.TypeNamespace, typeDef.TypeName);
					break;
				case Cor.MetaTableType.TypeRef:
					TypeRefRow typeRef = type.TypeDefOrRef.GetTargetRowTyped<TypeRefRow>();
					sType = typeRef.TypeNamespace == String.Empty
						? typeRef.TypeName
						: String.Join(".", typeRef.TypeNamespace, typeRef.TypeName);
					break;
				case Cor.MetaTableType.TypeSpec:
					TypeSpecRow typeSpec = type.TypeDefOrRef.GetTargetRowTyped<TypeSpecRow>();
					sType = type.Type.ToString() + ".???.TYPESPEC.???";
					break;
				default:
					throw new InvalidOperationException();
				}
				break;
			case Cor.ELEMENT_TYPE.MVAR:
				sType = "!" + type.RawPointer;
				break;
			case Cor.ELEMENT_TYPE.VAR:
				goto default;
			default:
				sType = type.Type.ToString();
				break;
			}
			String generic = type.GenericArguments == null
				? String.Empty
				: "<" + String.Join(", ", Array.ConvertAll(type.GenericArguments, a => Utils.ElementTypeToString(a))) + ">";
			return sType
			+ generic
				+ (type.IsArray ? String.Join(String.Empty, Array.ConvertAll(new Object[type.MultiArray], o => { return "[]"; })) : String.Empty)
				+ (type.IsByRef ? "&" : String.Empty)
				+ (type.IsPointer ? "*" : String.Empty);
		}

		public static String GetReflectedMembers(Object obj)
		{
			if(obj == null)
				return "<NULL>";

			
			StringBuilder result = new StringBuilder();
			Type objType = obj.GetType();
			if(objType.Assembly.GetName().Name == "mscorlib")
				result.Append(obj.ToString() + "\t");
			else
			{
				foreach(PropertyInfo prop in objType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
					result.AppendFormat("{0}: {1}\t", prop.Name, prop.GetValue(obj, null));
				foreach(FieldInfo field in objType.GetFields(BindingFlags.Instance | BindingFlags.Public))
					result.AppendFormat("{0}: {1}\t", field.Name, field.GetValue(obj));
			}

			return result.ToString();
		}

		[Conditional("DEBUG")]
		public static void SaveMetaTableClass(MetaTable table)
		{
			StringBuilder result = new StringBuilder(@"using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
");
			result.AppendLine(String.Format("\tpublic class {0}TableRow : BaseMetaRow", table.TableType));
			result.AppendLine("\t{");

			MetaRow row=table.RowsCount==0?null:table[0];
			if(row == null)
				result.AppendLine("#warning \"Class created predictially\"");

			foreach(var column in table.Columns)
				result.AppendLine(String.Format("\t\tpublic {0} {1} {{ get {{ return base.GetValue<{0}>({2}); }} }}",
					row == null ? column.ColumnType.ToString() : row[column].Value.GetType().Name,
					column.Name,
					column.Index));

			result.AppendLine("\t}");
			result.Append('}');

			//Save
			String path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), table.TableType.ToString() + "TableRow.cs");
			if(!File.Exists(path))
				File.WriteAllText(path, result.ToString());
		}

		//[Conditional("DEBUG")]
		public static String CteateMetaTableProperty(Cor.MetaTableType tableType)
		{
			return String.Format(@"public Tables.BaseMetaTable<Tables.{0}TableRow> {0}
{{
	get {{ return new Tables.BaseMetaTable<Tables.{0}TableRow>(this, MetaTableType.{0}); }}
}}
", tableType);
		}
	}
}