using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta;

namespace AlphaOmega.Debug
{
	public static class Utils
	{
		public static void ConsoleWriteMembers(Object obj)
		{
			Utils.ConsoleWriteMembers(null, obj);
		}
		public static void ConsoleWriteMembers(String title, Object obj)
		{
			if(!String.IsNullOrEmpty(title))
				Console.Write(title + ": ");
			Console.WriteLine(Utils.GetReflectedMembers(obj));
		}

		public static void ConsoleWriteError(Exception exc, String title, Boolean waitForInput = true)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(title + ": " + exc.Message);
			Console.WriteLine("========================");
			Console.WriteLine(exc.StackTrace);
			Console.ForegroundColor = color;
			if(waitForInput)
				Console.ReadKey();
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

		public static String ElementTypeToString(ElementType type)
		{
			return type.Type
				+ (type.IsArray ? "[]" : String.Empty)
				+ (type.IsPointer ? "*" : String.Empty);
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