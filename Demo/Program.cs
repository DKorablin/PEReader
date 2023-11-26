using System;
using System.ComponentModel;
using System.Text;

namespace AlphaOmega.Debug
{
	class Program
	{
		static void Main(String[] args)
		{
#if NETCOREAPP
			//.NET Core Error: No data is available for encoding 1252. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			ConsoleWriter writer = new ConsoleWriter(false);
			Reader reader = new Reader(writer);
			writer.StartThreadAsync(() =>
			{
				//String dll = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\symsrv.yes";
				//String obj = @"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_src\Debug\TabPageSSL.obj";
				//String dll = @"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_U.exe";
				//String dll = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe";
				//String dll = @"C:\Windows\Microsoft.NET\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll";
				//String dll = @"C:\Windows\SysWOW64\jscript.dll";
				//String dll = @"C:\WINDOWS\System32\Mpegc32f.dll";//TODO: Не получается прочитать PE файл через стандартный WinApi
				//String dll = @"C:\Visual Studio Projects\C++\SeQueL Explorer\bin\ManagedFlatbed.dll";
				//TODO: Не правильно читается MSDOS файл. (Вылетает с ошибкой при поптыке чтения по адресу e_lfanew)
				//String dll = @"C:\Program Files\HOMM_3.5\Data\zvs\h3blade.exe";
				//String dll = @"C:\WINDOWS\WinSxS\x86_Microsoft.Tools.VisualCPlusPlus.Runtime-Libraries_6595b64144ccf1df_6.0.9792.0_x-ww_08a6620a\atl.dll";
				//String dll = @"C:\Visual Studio Projects\Tests\ConsoleApp2\bin\Debug\net7.0\ConsoleApp2.dll";

				//TODO: INVALID. MetaData Address: 0x52463C Resources Address: 0x20780
				//String dll = @"C:\Visual Studio Projects\C#\Shared.Classes\AjaxControlToolkit.dll";

				foreach(String dll in Directory.GetFiles(@"C:\Visual Studio Projects\C#", "*.*", SearchOption.AllDirectories))
				switch(Path.GetExtension(dll.ToLowerInvariant()))
				{
				case ".dll":
				case ".exe":
						try
						{
							//reader.ReadObjInfo(obj);
							reader.ReadPeInfo2(dll, true);

						} catch(Win32Exception exc)
						{
							writer.ConsoleWriteError(exc, $"EXCEPTION IN FILE ({dll})", true);
						} catch(Exception exc)
						{
							exc.Data.Add("FilePath", dll);
							throw;
						}
						break;
				}
			});
		}
	}
}