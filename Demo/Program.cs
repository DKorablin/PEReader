using System;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

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
				//String filePath = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\symsrv.yes";
				//String filePath = @"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_src\Debug\TabPageSSL.obj";
				//String filePath = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe";
				//String filePath = @"C:\Windows\Microsoft.NET\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll";
				//String filePath = @"C:\Windows\SysWOW64\jscript.dll";
				//String filePath = @"C:\WINDOWS\System32\Mpegc32f.dll";//TODO: Не получается прочитать PE файл через стандартный WinApi
				//String filePath = @"C:\Visual Studio Projects\C++\SeQueL Explorer\bin\ManagedFlatbed.dll";
				//TODO: Не правильно читается MSDOS файл. (Вылетает с ошибкой при поптыке чтения по адресу e_lfanew)
				//String filePath = @"C:\Program Files\HOMM_3.5\Data\zvs\h3blade.exe";
				//String filePath = @"C:\WINDOWS\WinSxS\x86_Microsoft.Tools.VisualCPlusPlus.Runtime-Libraries_6595b64144ccf1df_6.0.9792.0_x-ww_08a6620a\atl.dll";
				//String filePath = @"C:\Visual Studio Projects\Tests\ConsoleApp2\bin\Debug\net7.0\ConsoleApp2.dll";
				//String filePath = @"C:\Visual Studio Projects\Tests\EnumTestApp\EnumTestApp\bin\Debug\net7.0\ConsoleApp.bat.exe";

				//TODO: INVALID. MetaData Address: 0x52463C Resources Address: 0x20780
				//String filePath = @"C:\Visual Studio Projects\C#\Shared.Classes\AjaxControlToolkit.dll";
				//TODO: INVALID. CustomAttribute enum value is stored in a external assebly and we don't know it's size...
				//String filePath = @"C:\Program Files\IIS Express\Microsoft.VisualStudio.Telemetry.dll";
				String filePath = @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\CommonExtensions\Microsoft\Terminal\cs\Microsoft.VisualStudio.Terminal.Implementation.resources.dll";

				//foreach(String filePath in Directory.EnumerateFiles(@"C:\Program Files", "*.*", SearchOption.AllDirectories))
				//foreach(String filePath in EnumerateFiles(@"C:\Program Files"))
					switch(Path.GetExtension(filePath.ToLowerInvariant()))
					{
					case ".dll":
					case ".exe":
						try
						{
							//reader.ReadObjInfo(obj);
							reader.ReadPeInfo2(filePath, true);

						} catch(Win32Exception exc)
						{
							writer.ConsoleWriteError(exc, $"EXCEPTION IN FILE ({filePath})", true);
						} catch(Exception exc)
						{
							exc.Data.Add(nameof(filePath), filePath);
							throw;
						}
						break;
					}

				String[] EnumerateFiles(String path)
				{
					IEnumerator<String> enumerator = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).GetEnumerator();
					List<String> result = new List<String>();
					while(true)
					{
						try
						{
							if(enumerator != null && enumerator.MoveNext())
								result.Add(enumerator.Current);
							else
								break;
						}catch(UnauthorizedAccessException)
						{//To ingnore files and folders that we don't have access
							continue;
						}
					}

					return result.ToArray();
				}
			});
		}
	}
}