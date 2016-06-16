using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;
using AlphaOmega.Debug.CorDirectory;

namespace AlphaOmega.Debug
{
	class Program
	{
		static void Main(String[] args)
		{
			//String dll = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
			String dll = @"C:\Windows\Microsoft.NET\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll";
			//String dll = @"C:\Visual Studio Projects\C#\Shared.Classes\AlphaOmega.Debug\DeviceIoControl\DeviceIoControl\bin\Release\DeviceIoControl.dll";
			//String dll = @"C:\WINDOWS\System32\Mpegc32f.dll";//TODO: Не получается прочитать PE файл через стандартный WinApi
			//String dll = @"C:\Visual Studio Projects\C++\SeQueL Explorer\bin\ManagedFlatbed.dll";
			//TODO: Не правильно читается MSDOS файл. (Вылетает с ошибкой при поптыке чтения по адресу e_lfanew)
			//String dll = @"C:\Program Files\HOMM_3.5\Data\zvs\h3blade.exe";
			//String dll = @"C:\WINDOWS\WinSxS\x86_Microsoft.Tools.VisualCPlusPlus.Runtime-Libraries_6595b64144ccf1df_6.0.9792.0_x-ww_08a6620a\atl.dll";

			/*using(FileStream stream = new FileStream(dll, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader reader = new BinaryReader(stream))
				{
					ImageHlp.IMAGE_DOS_HEADER dosHeader = PtrToStructure<ImageHlp.IMAGE_DOS_HEADER>(reader, 0);
					if(dosHeader.IsValid)
					{
						ImageHlp.IMAGE_NT_HEADERS32 hdr32 = PtrToStructure<ImageHlp.IMAGE_NT_HEADERS32>(reader, dosHeader.e_lfanew);
						ImageHlp.IMAGE_NT_HEADERS64 hdr64 = PtrToStructure<ImageHlp.IMAGE_NT_HEADERS64>(reader, dosHeader.e_lfanew);
					}
				}
			}
			return;*/

			/*DynamicDllLoader loader = new DynamicDllLoader();
			Byte[] bytes = File.ReadAllBytes(dll);
			loader.LoadLibrary(bytes);
			//Console.WriteLine(String.Format("Module count: {0}", loader.BuildImportTable()));
			//foreach(String procName in loader.GetProcedures())
			//{
			//	UInt32? procAddress = loader.GetProcAddress(procName);
			//	String text = String.Format("Proc: {0} Addr: 0x{1:X}", procName, procAddress);
			//	Console.WriteLine(text);
			//}
			return;*/

			/*PEFile file = new PEFile(dll);
			return;*/

			//foreach(String fileName in Directory.GetFiles(@"C:\Program Files\", "*.exe", SearchOption.AllDirectories))
				try
				{
					//dll = fileName;
					ReadPeInfo2(dll, true, false);
					
				} catch(Win32Exception exc)
				{
					Console.WriteLine("EXCEPTION IN FILE: {0}", dll);
					Console.WriteLine("===");
					Console.WriteLine("Message: {0}", exc.Message);
					Console.WriteLine(exc.StackTrace);
					Console.WriteLine("===");
					Console.WriteLine("Do yow want to continue? (Y/N)");
					/*switch(Console.ReadKey().KeyChar)
					{
					case 'y':
					case 'Y':
					default:
						break;
					case 'n':
					case 'N':
						return;
					}*/
				}
			return;
		}
		static void ReadPeInfo2(String dll, Boolean showDllName, Boolean pauseOnDir)
		{
			if(showDllName)
				Console.WriteLine("Reading file: {0}", dll);

			using(PEDirectory info = new PEDirectory(StreamLoader.FromFile(dll)))
			{
				if(info.Header.IsValidHeader)//Проверка на валидность загруженного файла
				{
					foreach(var section in info.Header.Sections)
						Utils.ConsoleWriteMembers(section);

					if(info.Header.SymbolTable.HasValue)
						Utils.ConsoleWriteMembers(info.Header.SymbolTable.Value);
					if(!info.Resource.IsEmpty)
					{
						Console.WriteLine("===Resources===");
						Int32 directoriesCount = 0;

						foreach(var dir in info.Resource)
						{
							directoriesCount++;
							//Console.WriteLine("dir: {0}", dir.NameAddress);
							Console.WriteLine("Resource dir: {0}", dir.Name);
							foreach(var dir1 in dir)
							{
								Console.WriteLine("----- {0}", dir1.Name);
								foreach(var dir2 in dir1)
								{
									Console.WriteLine("-------- {0}", dir2.Name);
									if(dir2.DirectoryEntry.IsDataEntry)
									{
										switch(dir.DirectoryEntry.NameType)
										{
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION:
											var version1 = new AlphaOmega.Debug.NTDirectory.ResourceVersion(dir2);
											var strFileInfo = version1.GetFileInfo();
											Utils.ConsoleWriteMembers(version1.FileInfo.Value);
											
											//WinNT.StringFileInfo fInfo = NativeMethods.BytesToStructure<WinNT.StringFileInfo>(bytesV, ptr);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING:
											var strings = new AlphaOmega.Debug.NTDirectory.ResourceString(dir2);
											foreach(var entry in strings)
												Utils.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR:
											var acc = new AlphaOmega.Debug.NTDirectory.ResourceAccelerator(dir2).ToArray();
											String testAcc=String.Empty;
											foreach(var a in acc)
												Utils.ConsoleWriteMembers(a);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST:
											Byte[] bytesM = dir2.GetData();//http://msdn.microsoft.com/ru-ru/library/eew13bza.aspx
											String xml = System.Text.Encoding.GetEncoding((Int32)dir2.DataEntry.Value.CodePage).GetString(bytesM);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MESSAGETABLE:
											var messageTable = new AlphaOmega.Debug.NTDirectory.ResourceMessageTable(dir2);
											foreach(var entry in messageTable)
												Utils.ConsoleWriteMembers(messageTable);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU:
											var resMenu = new AlphaOmega.Debug.NTDirectory.ResourceMenu(dir2);
											var menu = resMenu.GetMenuTemplate();
											Utils.ConsoleWriteMembers(menu);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR:
											var resFontDir = new AlphaOmega.Debug.NTDirectory.ResourceFontDir(dir2);
											foreach(var fontItem in resFontDir)
												Utils.ConsoleWriteMembers(fontItem);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT:
											var resFont = new AlphaOmega.Debug.NTDirectory.ResourceFont(dir2);
											Utils.ConsoleWriteMembers(resFont.Font);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP:
											//TODO:
											// http://www.vbdotnetforums.com/graphics-gdi/49563-i-need-help-converting-bitmap-dib-intptr.html
											// http://snipplr.com/view/36593/
											// http://www.codeproject.com/Articles/16268/DIB-to-System-Bitmap
											// http://ebersys.blogspot.com/2009/06/how-to-convert-dib-to-bitmap.html
											// http://hecgeek.blogspot.com/2007/04/converting-from-dib-to.html
											// http://objectmix.com/dotnet/101391-dib-bitmap-system-drawing-bitmap.html
											var resBitmap = new AlphaOmega.Debug.NTDirectory.ResourceBitmap(dir2);
											try
											{
												Utils.ConsoleWriteMembers(resBitmap.Header);
											} catch(ArgumentOutOfRangeException exc)
											{
												Console.WriteLine("!!!ArgumentOutOfRangeException (Corrupt bitmap): {0}", exc.Message);
												Console.ReadKey();
											}
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG:
											var dialog = new AlphaOmega.Debug.NTDirectory.ResourceDialog(dir2);
											try
											{
												var template = dialog.GetDialogTemplate();
												foreach(var ctrl in template.Controls)
													if(ctrl.CX < 0 || ctrl.CY < 0 || ctrl.X < 0 || ctrl.Y < 0)
													{
														Console.WriteLine("???Invalid position? ({0}) CX: {1} CY: {2} X: {3} Y: {4}", template.Title, ctrl.CX, ctrl.CY, ctrl.X, ctrl.Y);
														Console.ReadKey();
													} else
														Utils.ConsoleWriteMembers(ctrl);
											} catch(IndexOutOfRangeException exc)
											{
												Console.WriteLine("!!!IndexOutOfRangeException (Corrupt dialog): {0}", exc.Message);
												Console.ReadKey();
											} catch(ArgumentException exc)
											{
												Console.WriteLine("!!!ArgumentException (Corrupt dialog): {0}", exc.Message);
												Console.ReadKey();
											}
											break;
										}
									}
								}
							}
						}
						Console.WriteLine("Total dirs: {0}", directoriesCount);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(info.ComDescriptor != null)
					{//.NET Framework
						if(info.ComDescriptor.Resources != null)
						{
							if(info.ComDescriptor.Resources.Header.IsValid)
							{
								foreach(var item in info.ComDescriptor.Resources)
								{
									Console.WriteLine("Resource Item: {0}", item.Name);
									if(item.CanRead)
										foreach(var row in item)
											Console.WriteLine("\tResource row: {0} {1}", row.Name, row.Type);
									else
										Console.WriteLine("\t---Some object---");
								}
							} else
							{
								Console.WriteLine("INVALID. MetaData Address: {0} Resources Address: {1}", info.ComDescriptor.MetaData.Directory.VirtualAddress, info.ComDescriptor.Resources.Directory.VirtualAddress);
								Console.ReadKey();
							}
						}

						var meta = info.ComDescriptor.MetaData;
						Utils.ConsoleWriteMembers("MetaData", meta.Header.Value);

						foreach(var header in meta)
						{
							Console.WriteLine(Utils.GetReflectedMembers(header.Header));
							switch(header.Header.Type)
							{
							case Cor.StreamHeaderType.StreamTable:
								var table = (StreamTables)header;

								Console.WriteLine(Utils.GetReflectedMembers(table.StreamTableHeader));

								Array enums = Enum.GetValues(typeof(Cor.MetaTableType));
								foreach(Cor.MetaTableType type in enums)
								{
									//Пробежка по всем именованным таблицам
									PropertyInfo property = table.GetType().GetProperty(type.ToString(), BindingFlags.Instance | BindingFlags.Public);
									foreach(var row in ((IEnumerable)property.GetValue(table, null)))
									{
										switch(type)
										{
										case Cor.MetaTableType.NestedClass:
											{
												NestedClassRow nestedClassRow = (NestedClassRow)row;
												TypeDefRow parentTypeRow = nestedClassRow.EnclosingClass;
												TypeDefRow childTypeRow = nestedClassRow.NestedClass;
												String typeName = parentTypeRow.TypeNamespace + "." + parentTypeRow.TypeName + " {\r\n";
												typeName += "\tclass " + childTypeRow.TypeName + " {...}\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.FieldRVA:
											{
												FieldRVARow fieldRVARow = (FieldRVARow)row;
												FieldRow fieldRow = fieldRVARow.Field;
												String fieldName = fieldRow.Name + " -> 0x" + fieldRVARow.RVA.ToString("X8");
												Console.WriteLine("{0}: {1}", type, fieldName);
											}
											break;
										case Cor.MetaTableType.ImplMap:
											{
												ImplMapRow implMapRow = (ImplMapRow)row;
												ModuleRefRow moduleRow = implMapRow.ImportScope;
												String moduleName = moduleRow.Name + "-> " + implMapRow.ImportName;
												Console.WriteLine("{0}: {1}", type, moduleName);
											}
											break;
										case Cor.MetaTableType.MethodImpl:
											{
												MethodImplRow methodImplRow = (MethodImplRow)row;
												TypeDefRow typeRow = methodImplRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.PropertyMap:
											{
												PropertyMapRow propertyMapRow = (PropertyMapRow)row;
												TypeDefRow typeRow = propertyMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(PropertyRow propertyRow in propertyMapRow.Properties)
													typeName += "\t" + propertyRow.Name+";\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.EventMap:
											{//TODO: Не тестировано
												EventMapRow eventMapRow = (EventMapRow)row;
												TypeDefRow typeRow = eventMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(EventRow eventRow in eventMapRow.EventList)
													typeName += "\tevent " + eventRow.Name+";\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.FieldLayout:
											{
												FieldLayoutRow fieldLayoutRow = (FieldLayoutRow)row;
												FieldRow fieldRow = fieldLayoutRow.Field;
												String fieldName = fieldRow.Name;
												Console.WriteLine("{0}: {1}", type, fieldName);
											}
											break;
										case Cor.MetaTableType.ClassLayout:
											{
												ClassLayoutRow classLayoutRow = (ClassLayoutRow)row;
												TypeDefRow typeRow = classLayoutRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.InterfaceImpl:
											{
												InterfaceImplRow interfaceRow = (InterfaceImplRow)row;
												TypeDefRow typeRow = interfaceRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.TypeDef:
											{
												TypeDefRow typeRow = (TypeDefRow)row;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(var fieldRow in typeRow.FieldList)
													typeName += "\t" + fieldRow.Name + ";\r\n";
												foreach(var methodRow in typeRow.MethodList)
												{
													typeName += "\t" + methodRow.Name + "(";
													foreach(var paramRow in methodRow.ParamList)
														typeName += paramRow.Name + ", ";
													typeName = typeName.TrimEnd(',', '"', ' ') + ");\r\n";
												}
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.MethodDef:
											{
												MethodDefRow methodRow = (MethodDefRow)row;
												String methodName = methodRow.Name + "(";
												foreach(var paramRow in methodRow.ParamList)
													methodName += paramRow.Name + ", ";
												methodName = methodName.TrimEnd(',', '"', ' ') + ")";
												Console.WriteLine("{0}: {1}", type, methodName);

												//TODO: Быстрый набросок чтения тела метода
												UInt32 methodLength = 0;

												UInt32 rva = methodRow.RVA;
												Byte flags = info.Header.PtrToStructure<Byte>(rva);
												rva += sizeof(Byte);
												Boolean CorILMethod_FatFormat = (UInt16)(flags & 0x3) == 0x3;
												Boolean CorILMethod_TinyFormat = (UInt16)(flags & 0x2) == 0x2;
												Boolean CorILMethod_MoreSects = (UInt16)(flags & 0x8) == 0x8;
												Boolean CorILMethod_InitLocals = (UInt16)(flags & 0x10) == 0x10;

												if(CorILMethod_FatFormat)
												{
													Byte flags2 = info.Header.PtrToStructure<Byte>(rva);
													rva += sizeof(Byte);

													Byte headerSize = Convert.ToByte((flags2 >> 4) * 4);
													UInt16 maxStack = info.Header.PtrToStructure<UInt16>(rva);
													rva += sizeof(UInt16);

													methodLength = info.Header.PtrToStructure<UInt32>(rva);
													rva += sizeof(UInt32);

													UInt32 localVarSigTok = info.Header.PtrToStructure<UInt32>(rva);
													rva += sizeof(UInt32);
												} else
													methodLength = ((UInt32)flags >> 2);

												try
												{
													Byte[] methodBody = info.Header.ReadBytes(rva, methodLength);
													Byte[] methodBody2 = methodRow.Body.GetMethodBody();

													for(Int32 loop = 0; loop < methodLength; loop++)
														if(methodBody[loop] != methodBody2[loop])
															throw new ArgumentException("Method not equals");
												} catch(Exception exc)
												{
													Console.WriteLine("!!!Error while reading method body!!!");
													Console.WriteLine(exc.Message);
													Console.ReadLine();
												}
											}
											break;
										default:
											Utils.ConsoleWriteMembers(row);
											break;
										}
									}

									//Пробежка по всем таблицам
									MetaTable moduleTable = table[type];

									Console.WriteLine(String.Format("==MetaTableType.{0} Contents:", type));
									foreach(MetaRow row in moduleTable.Rows)
									{
										StringBuilder result =new StringBuilder();
										foreach(MetaCell cell in row)
											result.AppendFormat("{0}:\t{1}", cell.Column.Name, cell.Value);
										result.AppendLine();
										Console.WriteLine(result.ToString());
									}
									Console.WriteLine(String.Format("==MetaTableType.{0} End", type));
								}
								break;
							case Cor.StreamHeaderType.Guid:
								var gHeap = (GuidHeap)header;
								Guid[] guids = gHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.Blob:
								var bHeap = (BlobHeap)header;
								Byte[][] bytes = bHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.String:
								var sHeap = (StringHeap)header;
								String[] strings = sHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.UnicodeSting:
								var usHeap = (USHeap)header;
								String[] usStrings = usHeap.DataString;
								break;
							}
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.ExceptionTable.IsEmpty)
					{//TODO: Ошибка при чтении
						Console.WriteLine("===Exception Table===");
						try
						{
							foreach(var entry in info.ExceptionTable)
								Utils.ConsoleWriteMembers(entry);
						} catch(ArgumentOutOfRangeException exc)
						{
							Console.WriteLine("Exception: "+exc.Message);
							Console.WriteLine("========================");
							Console.WriteLine(exc.StackTrace);
							Console.ReadKey();
						}
						if(pauseOnDir)
							Console.ReadKey();
					}
					if(!info.Iat.IsEmpty)
					{
						Console.WriteLine("===Import Address Table===");
						foreach(UInt32 addr in info.Iat)
							Console.WriteLine("Addr: {0:X8}", addr);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Tls.IsEmpty)
					{
						Console.WriteLine("===Thread Local Storage===");
						if(pauseOnDir)
							Console.ReadKey();
					}
					if(!info.Certificate.IsEmpty)
					{
						try
						{
							var cert = info.Certificate.Certificate.Value;
							var x509 = info.Certificate.X509;
							Console.WriteLine("===Security===");
							Utils.ConsoleWriteMembers(cert);
							Console.WriteLine("Certificate: {0}", x509 == null ? "NULL" : x509.ToString());
						} catch(ArgumentOutOfRangeException exc)
						{
							Console.WriteLine("!!!OverflowException (Corrupted section): {0}", exc.Message);
							Console.ReadKey();
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.DelayImport.IsEmpty)
					{
						Console.WriteLine("===Delay Import===");
						foreach(var module in info.DelayImport)
							Console.WriteLine(String.Format("Module Name: {0}\tCount: {1}", module.ModuleName, module.Count()));
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Relocations.IsEmpty)
					{//File contains relocation table
						Console.WriteLine("===Relocations===");
						foreach(var block in info.Relocations)
						{
							Utils.ConsoleWriteMembers(block.Block);
							foreach(var section in block)
							{
								Utils.ConsoleWriteMembers(section);
								/*if(!Enum.IsDefined(typeof(WinNT.IMAGE_REL_BASED), section.Type))
								{
									Console.WriteLine(String.Format("Enum {0} not defined", section.Type));
									Console.ReadKey();
								}*/
							}
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Debug.IsEmpty)
					{//В файле есть инормация для дебага
						Console.WriteLine("===Debug info===");
						foreach(var debug in info.Debug)
							Utils.ConsoleWriteMembers(debug);
						var pdb7 = info.Debug.Pdb7CodeView;
						if(pdb7.HasValue)
							Utils.ConsoleWriteMembers(pdb7.Value);
						var pdb2 = info.Debug.Pdb2CodeView;
						if(pdb2.HasValue)
							Utils.ConsoleWriteMembers(pdb2.Value);
						var misc = info.Debug.Misc;
						if(misc.HasValue)
							Utils.ConsoleWriteMembers(misc.Value);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.LoadConfig.IsEmpty)
					{
						Console.WriteLine("===Load Config===");
						if(info.LoadConfig.Directory32.HasValue)
						{
							var directory = info.LoadConfig.Directory32.Value;
							Utils.ConsoleWriteMembers(directory);
						} else if(info.LoadConfig.Directory64.HasValue)
						{
							var directory = info.LoadConfig.Directory64.Value;
							Utils.ConsoleWriteMembers(directory);
						} else
							throw new NotImplementedException();
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.BoundImport.IsEmpty)
					{
						Console.WriteLine("===Bound Import===");
						Console.WriteLine("ModuleName: {0}", info.BoundImport.ModuleName);
						foreach(var ffRef in info.BoundImport)
							Utils.ConsoleWriteMembers(ffRef);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Export.IsEmpty)
					{//В файле есть информация о экспортируемых функциях
						Console.WriteLine("===Export Functions===");
						Console.WriteLine("Module name: {0}", info.Export.DllName);

						foreach(var func in info.Export.GetExportFunctions())
							Utils.ConsoleWriteMembers(func);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Import.IsEmpty)
					{//В файле есть информация о импортиуемых модулях
						Console.WriteLine("===Import Modules===");
						foreach(var module in info.Import)
						{
							Console.WriteLine("Module name: {0}", module.ModuleName);
							foreach(var func in module)
								Utils.ConsoleWriteMembers(func);
						}
						if(pauseOnDir)
							Console.ReadKey();
					}
				}
			}
		}
	}
}