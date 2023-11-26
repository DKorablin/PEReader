using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.CorDirectory.Meta.Reader;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug
{
	internal class Reader
	{
		private readonly ConsoleWriter _console;

		public Reader(ConsoleWriter console)
		{
			this._console = console;
		}

		public void ReadPeInfo2(String dll, Boolean showDllName)
		{
			if(showDllName)
				_console.WriteLine($"Reading file: {dll}");

			using(PEFile info = new PEFile(dll, StreamLoader.FromFile(dll)))
			{
				if(info.Header.IsValid)//Проверка на валидность загруженного файла
				{
					WinNT.IMAGE_FILE_HEADER fileHeader = info.Header.Is64Bit
					? info.Header.HeaderNT64.FileHeader
					: info.Header.HeaderNT32.FileHeader;
					_console.ConsoleWriteMembers(fileHeader);

					foreach(var section in info.Sections)
					{
						if(section.Header.Section != null && section.Description == null)
							_console.WriteLine($"Unknown section name: {section.Header.Section}");

						_console.ConsoleWriteMembers(section);
					}

					if(info.Header.SymbolTable != null)
						_console.ConsoleWriteMembers(info.Header.SymbolTable.Value);

					if(!info.Resource.IsEmpty)
					{
						_console.WriteLine("===Resources===");
						Int32 directoriesCount = 0;

						foreach(var dir in info.Resource)
						{
							directoriesCount++;
							//_console.WriteLine($"dir: {dir.NameAddress}");
							_console.WriteLine($"Resource dir: {dir.Name}");
							foreach(var dir1 in dir)
							{
								_console.WriteLine($"----- {dir1.Name}");
								foreach(var dir2 in dir1)
								{
									_console.WriteLine($"-------- {dir2.Name}");
									if(dir2.DirectoryEntry.IsDataEntry)
									{
										switch(dir.DirectoryEntry.NameType)
										{
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION:
											var version1 = new AlphaOmega.Debug.NTDirectory.Resources.ResourceVersion(dir2);
											var strFileInfo = version1.GetFileInfo();
											_console.ConsoleWriteMembers(version1.FileInfo.Value);

											//WinNT.StringFileInfo fInfo = NativeMethods.BytesToStructure<WinNT.StringFileInfo>(bytesV, ptr);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING:
											var strings = new AlphaOmega.Debug.NTDirectory.Resources.ResourceString(dir2);
											foreach(var entry in strings)
												_console.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR:
											var acc = new AlphaOmega.Debug.NTDirectory.Resources.ResourceAccelerator(dir2).ToArray();
											String testAcc = String.Empty;
											foreach(var a in acc)
												_console.ConsoleWriteMembers(a);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST:
											Byte[] bytesM = dir2.GetData();//http://msdn.microsoft.com/ru-ru/library/eew13bza.aspx
											String xml = System.Text.Encoding.GetEncoding((Int32)dir2.DataEntry.Value.CodePage).GetString(bytesM);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MESSAGETABLE:
											var messageTable = new AlphaOmega.Debug.NTDirectory.Resources.ResourceMessageTable(dir2);
											foreach(var entry in messageTable)
												_console.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU:
											var resMenu = new AlphaOmega.Debug.NTDirectory.Resources.ResourceMenu(dir2);
											foreach(var entry in resMenu.GetMenuTemplate())
												_console.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_TOOLBAR:
											var resToolbar = new AlphaOmega.Debug.NTDirectory.Resources.ResourceToolBar(dir2);
											_console.ConsoleWriteMembers(resToolbar.Header);

											foreach(var entry in resToolbar.GetToolBarTemplate())
												_console.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR:
											var resFontDir = new AlphaOmega.Debug.NTDirectory.Resources.ResourceFontDir(dir2);
											foreach(var fontItem in resFontDir)
												_console.ConsoleWriteMembers(fontItem);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT:
											var resFont = new AlphaOmega.Debug.NTDirectory.Resources.ResourceFont(dir2);
											_console.ConsoleWriteMembers(resFont.Font);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP:
											//TODO:
											// http://www.vbdotnetforums.com/graphics-gdi/49563-i-need-help-converting-bitmap-dib-intptr.html
											// http://snipplr.com/view/36593/
											// http://www.codeproject.com/Articles/16268/DIB-to-System-Bitmap
											// http://ebersys.blogspot.com/2009/06/how-to-convert-dib-to-bitmap.html
											// http://hecgeek.blogspot.com/2007/04/converting-from-dib-to.html
											// http://objectmix.com/dotnet/101391-dib-bitmap-system-drawing-bitmap.html
											var resBitmap = new AlphaOmega.Debug.NTDirectory.Resources.ResourceBitmap(dir2);
											try
											{
												_console.ConsoleWriteMembers(resBitmap.Header);
											} catch(ArgumentOutOfRangeException exc)
											{
												_console.ConsoleWriteError(exc, "ArgumentOutOfRangeException (Corrupt bitmap)", true);
											}
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON:
											var resIcon = new AlphaOmega.Debug.NTDirectory.Resources.ResourceIcon(dir2);
											/*WinGdi.ICONDIR icoHeader = new WinGdi.ICONDIR() { idReserved = 0, idType = 1, idCount = 1 };
											List<Byte> bytes = new List<Byte>();
											bytes.AddRange(PinnedBufferReader.StructureToArray<WinGdi.ICONDIR>(icoHeader));

											var resHeader=resIcon.Header;
											Byte[] payload = resIcon.Directory.GetData().Skip(Marshal.SizeOf(typeof(WinGdi.GRPICONDIRENTRY))).ToArray();
											WinGdi.ICONDIRENTRY icoEntry = new WinGdi.ICONDIRENTRY()
											{
												bWidth = resHeader.bWidth,
												bHeight = resHeader.bHeight,
												bColorCount = resHeader.bColorCount,
												bReserved = resHeader.bReserved,
												wPlanes = resHeader.wPlanes,
												wBitCount = resHeader.wBitCount,
												dwBytesInRes = (UInt32)payload.Length,
												dwImageOffset = (UInt32)(Marshal.SizeOf(typeof(WinGdi.ICONDIR)) + Marshal.SizeOf(typeof(WinGdi.ICONDIRENTRY))),
											};
											bytes.AddRange(PinnedBufferReader.StructureToArray<WinGdi.ICONDIRENTRY>(icoEntry));
											bytes.AddRange(payload);
											File.WriteAllBytes(@"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_src\res\RT_ICON.ico", bytes.ToArray());*/
											_console.ConsoleWriteMembers(resIcon.Header);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DLGINIT:
											var dlgInit = new AlphaOmega.Debug.NTDirectory.Resources.ResourceDialogInit(dir2);
											foreach(var initData in dlgInit)
												_console.ConsoleWriteMembers(initData);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG:
											var dialog = new AlphaOmega.Debug.NTDirectory.Resources.ResourceDialog(dir2);
											try
											{
												var template = dialog.GetDialogTemplate();
												foreach(var ctrl in template.Controls)
													if(ctrl.CX < 0 || ctrl.CY < 0 || ctrl.X < 0 || ctrl.Y < 0)
														_console.ConsoleWriteError(true, $"???Invalid position? ({template.Title}) CX: {ctrl.CX} CY: {ctrl.CY} X: {ctrl.X} Y: {ctrl.Y}");
													else
														_console.ConsoleWriteMembers(ctrl);
											} catch(IndexOutOfRangeException exc)
											{
												_console.ConsoleWriteError(exc, "IndexOutOfRangeException (Corrupt dialog)", true);
											} catch(ArgumentException exc)
											{
												_console.ConsoleWriteError(exc, "ArgumentException (Corrupt dialog)", true);
											}
											break;
										}
									}
								}
							}
						}
						_console.WriteLine($"Total dirs: {directoriesCount}");
						_console.PauseOnDir();
					}

					if(info.ComDescriptor != null)
					{//.NET Framework
						if(info.ComDescriptor.Resources != null)
						{
							if(info.ComDescriptor.Resources.Header.IsValid)
							{
								foreach(var item in info.ComDescriptor.Resources)
								{
									_console.WriteLine($"Resource Item: {item.Name}");
									if(item.CanRead)
										foreach(var row in item)
											_console.WriteLine($"\tResource row: {row.Name} {row.Type}");
									else
										_console.WriteLine("\t---Some object---");
								}
							} else
							{//TODO: AjaxControlToolkit.dll
								_console.ConsoleWriteError(true,
									$"INVALID. MetaData Address: 0x{info.ComDescriptor.MetaData.Directory.VirtualAddress:X} Resources Address: 0x{info.ComDescriptor.Resources.Directory.VirtualAddress:X}");
							}
						}

						var meta = info.ComDescriptor.MetaData;
						_console.ConsoleWriteMembers("MetaData", meta.Header.Value);

						foreach(var header in meta)
						{
							_console.WriteLine(Utils.GetReflectedMembers(header.Header));
							switch(header.Header.Type)
							{
							case Cor.StreamHeaderType.StreamTable:
								var table = (StreamTables)header;

								_console.WriteLine(Utils.GetReflectedMembers(table.StreamTableHeader));

								Array enums = Enum.GetValues(typeof(Cor.MetaTableType));
								foreach(Cor.MetaTableType type in enums)
								{
									//Пробежка по всем именованным таблицам
									PropertyInfo property = table.GetType().GetProperty(type.ToString(), BindingFlags.Instance | BindingFlags.Public);
									foreach(var row in (IEnumerable)property.GetValue(table, null))
									{
										switch(type)
										{
										case Cor.MetaTableType.CustomAttribute:
											{
												AttributeReader attrReader = new AttributeReader((CustomAttributeRow)row);
												List<String> arguments = new List<String>();
												foreach(MemberArgument value in attrReader.Attribute.FixedArgs)
													arguments.Add($"{Utils.ElementTypeToString(value.Type)} {value.Name} = {Utils.ValueToString(value.Value)}");
												_console.WriteLine($"{attrReader.FullName}({String.Join(", ", arguments.ToArray())})");
												
												arguments.Clear();
												foreach(MemberElement value in attrReader.Attribute.NamedArgs)
													arguments.Add($"\t{Utils.ElementTypeToString(value.Type)} ({value.ElementType}) {value.Name} = {Utils.ValueToString(value.Value)}");
												if(arguments.Count > 0)
													_console.WriteLine("{\r\n" + String.Join(Environment.NewLine, arguments.ToArray()) + "\r\n}");
												break;
											}
										case Cor.MetaTableType.NestedClass:
											{
												NestedClassRow nestedClassRow = (NestedClassRow)row;
												TypeDefRow parentTypeRow = nestedClassRow.EnclosingClass;
												TypeDefRow childTypeRow = nestedClassRow.NestedClass;
												String typeName = parentTypeRow.TypeNamespace + "." + parentTypeRow.TypeName + " {\r\n";
												typeName += "\tclass " + childTypeRow.TypeName + " {...}\r\n";
												typeName += "}";
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.FieldRVA:
											{
												FieldRVARow fieldRVARow = (FieldRVARow)row;
												FieldRow fieldRow = fieldRVARow.Field;
												String fieldName = fieldRow.Name + " -> 0x" + fieldRVARow.RVA.ToString("X8");
												_console.WriteLine($"{type}: {fieldName}");
											}
											break;
										case Cor.MetaTableType.ImplMap:
											{
												ImplMapRow implMapRow = (ImplMapRow)row;
												ModuleRefRow moduleRow = implMapRow.ImportScope;
												String moduleName = moduleRow.Name + "-> " + implMapRow.ImportName;
												_console.WriteLine($"{type}: {moduleName}");
											}
											break;
										case Cor.MetaTableType.MethodImpl:
											{
												MethodImplRow methodImplRow = (MethodImplRow)row;
												TypeDefRow typeRow = methodImplRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.PropertyMap:
											{
												PropertyMapRow propertyMapRow = (PropertyMapRow)row;
												TypeDefRow typeRow = propertyMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(PropertyRow propertyRow in propertyMapRow.PropertyList)
													typeName += "\t" + propertyRow.Name + ";\r\n";
												typeName += "}";
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.EventMap:
											{//TODO: Не тестировано
												EventMapRow eventMapRow = (EventMapRow)row;
												TypeDefRow typeRow = eventMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(EventRow eventRow in eventMapRow.EventList)
													typeName += "\tevent " + eventRow.Name + ";\r\n";
												typeName += "}";
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.FieldLayout:
											{
												FieldLayoutRow fieldLayoutRow = (FieldLayoutRow)row;
												FieldRow fieldRow = fieldLayoutRow.Field;
												String fieldName = fieldRow.Name;
												_console.WriteLine($"{type}: {fieldName}");
											}
											break;
										case Cor.MetaTableType.ClassLayout:
											{
												ClassLayoutRow classLayoutRow = (ClassLayoutRow)row;
												TypeDefRow typeRow = classLayoutRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.InterfaceImpl:
											{
												InterfaceImplRow interfaceRow = (InterfaceImplRow)row;
												TypeDefRow typeRow = interfaceRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												_console.WriteLine($"{type}: {typeName}");
											}
											break;
										case Cor.MetaTableType.TypeDef:
											{
												TypeReader typeRow = new TypeReader((TypeDefRow)row);
												_console.WriteLine($"{type}: {typeRow.FullName} {{");
												
												foreach(FieldRow fieldRow in typeRow.GetFields())
													_console.WriteLine("\t" + Utils.ElementTypeToString(fieldRow.ReturnType) + " " + fieldRow.Name + ";");

												foreach(PropertyReader propertyRow in typeRow.GetProperties())
													_console.WriteLine($"\t{Utils.ElementTypeToString(propertyRow.Return)} {propertyRow.Name} {{ {(propertyRow.CanRead ? "get; " : String.Empty)}{(propertyRow.CanWrite ? "set; " : String.Empty)}}}");

												foreach(MethodReader methodRow in typeRow.GetMembers().Where(m => m.IsProperty == false))
													_console.WriteLine("\t" + Utils.ElementTypeToString(methodRow.Return) + " " + methodRow.Name
														+ "(" + String.Join(", ", methodRow.GetArguments().Select(a => Utils.ElementTypeToString(a.Type) + " " + a.Name).ToArray()) + ");");

												_console.WriteLine("}");
												break;
											}
										case Cor.MetaTableType.MethodDef:
											{
												MethodReader reader = new MethodReader((MethodDefRow)row);
												String methodName = Utils.ElementTypeToString(reader.Return)
													+ " " + reader.DeclaringType.FullName
													+ "." + reader.Name + "(";
												Int32 index = 0;
												foreach(MemberArgument paramRow2 in reader.GetArguments())
												{
													ElementType typeEnum = reader.MethodDef.ArgsType[index++];
													String typeString = Utils.ElementTypeToString(typeEnum);
													methodName = methodName + typeString + " " + paramRow2.Name + ", ";
												}

												methodName = methodName.TrimEnd(',', '"', ' ') + ")";
												_console.WriteLine($"{type}: {methodName}");

												UInt32 rva = reader.MethodDef.RVA;
												Byte flags = info.Header.PtrToStructure<Byte>(rva++);
												Boolean CorILMethod_FatFormat = (UInt16)(flags & 3) == 3;
												Boolean CorILMethod_TinyFormat = (UInt16)(flags & 2) == 2;
												Boolean CorILMethod_MoreSects = (UInt16)(flags & 8) == 8;
												Boolean CorILMethod_InitLocals = (UInt16)(flags & 0x10) == 16;
												Cor.CorILMethodHeader methodHeader = reader.MethodDef.Body.Header;
												Cor.CorILMethod methodHeaderFlags = methodHeader.Format;
												if(CorILMethod_FatFormat != ((methodHeaderFlags & Cor.CorILMethod.FatFormat) == Cor.CorILMethod.FatFormat))
													throw new InvalidOperationException("Bit FAT format set to TRUE but flag marked as FALSE");
												if(CorILMethod_FatFormat == false && CorILMethod_TinyFormat != ((methodHeaderFlags & Cor.CorILMethod.TinyFormat) == Cor.CorILMethod.TinyFormat))
													throw new InvalidOperationException("Bit TINY format set to TRUE but flag marked as FALSE");
												if(CorILMethod_FatFormat == true)
												{
													if(CorILMethod_MoreSects != ((methodHeaderFlags & Cor.CorILMethod.MoreSects) == Cor.CorILMethod.MoreSects))
														throw new InvalidOperationException("Bit MoreSects set to TRUE but flag marked as FALSE");//Available only for FatFormat
													if(CorILMethod_InitLocals != ((methodHeaderFlags & Cor.CorILMethod.InitLocals) == Cor.CorILMethod.InitLocals))
														throw new InvalidOperationException("Bit InitLocals set to TRUE but flag marked as FALSE");
												}

												_console.WriteLine($"Method begins at RVA: 0x{reader.MethodDef.RVA:X}");
												_console.WriteLine($"Code size: 0x{methodHeader.CodeSize:X}");
												try
												{
													foreach(MethodLine ilLine in reader.MethodDef.Body.GetMethodBody2())
													{
														StringBuilder line = new StringBuilder();
														if(ilLine.Offset != null)
														{
															if(ilLine.Offset.Length == 1)
																line.Append(" IL_" + ilLine.Offset[0].ToString("X").PadLeft(4, '0'));
															else
																line.Append(" (" + String.Join(", ", ilLine.Offset.Select(o => "IL_" + o.ToString("X").PadLeft(4, '0')).ToArray()) + ")");
														} else if(ilLine.ParamIndexRow != null)
															line.Append(" " + ilLine.ParamIndexRow.Name);
														else if(ilLine.Token != null)
														{
															MetaRow memberRow = ilLine.Token.TargetRow;
															switch(ilLine.Token.TableType)
															{
															case Cor.MetaTableType.Field:
																{
																	FieldRow fieldRow = ilLine.Token.GetTargetRowTyped<FieldRow>();
																	line.Append(" " + Utils.ElementTypeToString(fieldRow.ReturnType) + " " + fieldRow.Name);
																	break;
																}
															case Cor.MetaTableType.MemberRef:
																{
																	MemberRefRow memberRefRow = ilLine.Token.GetTargetRowTyped<MemberRefRow>();
																	switch(memberRefRow.Class.TableType)
																	{
																	case Cor.MetaTableType.TypeRef:
																		TypeRefRow classRefRow = memberRefRow.Class.GetTargetRowTyped<TypeRefRow>();
																		String fullName = classRefRow.TypeNamespace == String.Empty
																			? classRefRow.TypeName
																			: classRefRow.TypeNamespace + "." + classRefRow.TypeName;
																		line.Append(" " + fullName + "::" + memberRefRow.Name);
																		break;
																	case Cor.MetaTableType.TypeSpec:
																		TypeSpecRow typeSpecRow1 = memberRefRow.Class.GetTargetRowTyped<TypeSpecRow>();
																		line.Append(" " + Utils.ElementTypeToString(typeSpecRow1.SignatureParsed) + "::" + memberRefRow.Name);
																		break;
																	default:
																		throw new NotImplementedException($"Unknown reference from MemberRef table: {ilLine.Token.TableType}");
																	}
																	break;
																}
															case Cor.MetaTableType.TypeRef:
																TypeRefRow typeRefRow = ilLine.Token.GetTargetRowTyped<TypeRefRow>();
																line.Append(" " + String.Join(".", new String[] { typeRefRow.TypeNamespace, typeRefRow.TypeName }));
																break;
															case Cor.MetaTableType.MethodDef:
																{
																	MethodDefRow methodDefRow = ilLine.Token.GetTargetRowTyped<MethodDefRow>();
																	line.Append(" " + Utils.ElementTypeToString(methodDefRow.ReturnType) + " " + methodDefRow.Name);
																	line.Append("(" + String.Join(", ", methodDefRow.ParamList.Select(p => Utils.ElementTypeToString(p.Type) + " " + p.Name).ToArray()) + ")");
																	break;
																}
															case Cor.MetaTableType.TypeDef:
																TypeDefRow typeDefRow = ilLine.Token.GetTargetRowTyped<TypeDefRow>();
																line.Append(" " + typeDefRow.TypeNamespace + "." + typeDefRow.TypeName);
																break;
															case Cor.MetaTableType.TypeSpec:
																TypeSpecRow typeSpecRow = ilLine.Token.GetTargetRowTyped<TypeSpecRow>();
																line.Append(" " + Utils.ElementTypeToString(typeSpecRow.SignatureParsed));
																break;
															case Cor.MetaTableType.MethodSpec:
																{
																	MethodSpecRow methodSpecRow = ilLine.Token.GetTargetRowTyped<MethodSpecRow>();
																	switch(methodSpecRow.Method.TableType)
																	{
																	case Cor.MetaTableType.MethodDef:
																		MethodDefRow methodDefRow = methodSpecRow.Method.GetTargetRowTyped<MethodDefRow>();
																		line.Append(" " + Utils.ElementTypeToString(methodDefRow.ReturnType) + " " + methodDefRow.Name);
																		break;
																	case Cor.MetaTableType.MemberRef:
																		MemberRefRow memberRef = methodSpecRow.Method.GetTargetRowTyped<MemberRefRow>();
																		line.Append(" " + Utils.ElementTypeToString(memberRef.ReturnType) + " " + memberRef.Name);
																		break;
																	default:
																		throw new NotImplementedException();
																	}
																	line.Append("<" + String.Join(", ", methodSpecRow.GenArgs.Select(a => Utils.ElementTypeToString(a))) + ">");
																	break;
																}
															default:
																throw new NotImplementedException($"Unknown reference {ilLine.Token.TableType}");
															}
														} else if(ilLine.ConstantValue != null)
															line.Append(" " + ilLine.ConstantValue);

														_console.ConsolWriteInstruction(ilLine.Line, ilLine.IL, line.ToString());
													}

													Byte[] methodBody = info.Header.ReadBytes(rva, methodHeader.CodeSize);
													Byte[] methodBody2 = reader.MethodDef.Body.GetMethodBody();
													for(Int32 loop = 0; loop < methodHeader.CodeSize; loop++)
														if(methodBody[loop] != methodBody2[loop])
															throw new ArgumentException("Methods not equals");
												} catch(Exception exc)
												{
													_console.ConsoleWriteError(exc, "Error reading method body");
												}
												break;
											}
										default:
											_console.ConsoleWriteMembers(row);
											break;
										}
									}

									//Пробежка по всем таблицам
									MetaTable moduleTable = table[type];

									_console.WriteLine($"==MetaTableType.{type} Contents:");
									foreach(MetaRow row in moduleTable.Rows)
									{
										StringBuilder result = new StringBuilder();
										foreach(MetaCell cell in row)
											result.AppendFormat("{0}:\t{1}", cell.Column.Name, cell.Value);
										result.AppendLine();
										_console.WriteLine(result.ToString());
									}
									_console.WriteLine($"==MetaTableType.{type} End");
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
								Dictionary<Int32, String> usStrings = usHeap.GetDataString().ToDictionary(p => p.Key, p => p.Value);
								break;
							}
						}
						_console.PauseOnDir();
					}

					if(!info.ExceptionTable.IsEmpty)
					{//TODO: Ошибка при чтении
						_console.WriteLine("===Exception Table===");
						try
						{
							foreach(var entry in info.ExceptionTable)
								_console.ConsoleWriteMembers(entry);
						} catch(ArgumentOutOfRangeException exc)
						{
							_console.ConsoleWriteError(exc, "Exception", true);
						}
						_console.PauseOnDir();
					}
					if(!info.Iat.IsEmpty)
					{
						_console.WriteLine("===Import Address Table===");
						foreach(UInt32 addr in info.Iat)
							_console.WriteLine($"Addr: {addr:X8}");
						_console.PauseOnDir();
					}

					if(!info.Tls.IsEmpty)
					{
						_console.WriteLine("===Thread Local Storage===");
						_console.PauseOnDir();
					}
					if(!info.Certificate.IsEmpty)
					{
						try
						{
							var cert = info.Certificate.Certificate.Value;
							var x509 = info.Certificate.X509;
							_console.WriteLine("===Security===");
							_console.ConsoleWriteMembers(cert);
							_console.WriteLine($"Certificate: {(x509 == null ? "NULL" : x509.ToString())}");
						} catch(ArgumentOutOfRangeException exc)
						{
							_console.ConsoleWriteError(exc, "OverflowException (Corrupted section)", true);
						}
						_console.PauseOnDir();
					}

					if(!info.DelayImport.IsEmpty)
					{
						_console.WriteLine("===Delay Import===");
						foreach(var module in info.DelayImport)
							_console.WriteLine($"Module Name: {module.ModuleName}\tCount: {module.Count()}");
						_console.PauseOnDir();
					}

					if(!info.Relocations.IsEmpty)
					{//File contains relocation table
						_console.WriteLine("===Relocations===");
						foreach(var block in info.Relocations)
						{
							_console.ConsoleWriteMembers(block.Block);
							foreach(var section in block)
							{
								_console.ConsoleWriteMembers(section);
								/*if(!Enum.IsDefined(typeof(WinNT.IMAGE_REL_BASED), section.Type))
								{
									_console.WriteLine($"Enum {section.Type} not defined");
									Console.ReadKey();
								}*/
							}
						}
						_console.PauseOnDir();
					}

					if(!info.Debug.IsEmpty)
					{//В файле есть инормация для дебага
						_console.WriteLine("===Debug info===");
						foreach(var debug in info.Debug)
							_console.ConsoleWriteMembers(debug);
						var pdb7 = info.Debug.Pdb7CodeView;
						if(pdb7.HasValue)
							_console.ConsoleWriteMembers(pdb7.Value);
						var pdb2 = info.Debug.Pdb2CodeView;
						if(pdb2.HasValue)
							_console.ConsoleWriteMembers(pdb2.Value);
						var misc = info.Debug.Misc;
						if(misc.HasValue)
							_console.ConsoleWriteMembers(misc.Value);
						_console.PauseOnDir();
					}

					if(!info.LoadConfig.IsEmpty)
					{
						_console.WriteLine("===Load Config===");
						if(info.LoadConfig.Directory32.HasValue)
						{
							var directory = info.LoadConfig.Directory32.Value;
							_console.ConsoleWriteMembers(directory);
						} else if(info.LoadConfig.Directory64.HasValue)
						{
							var directory = info.LoadConfig.Directory64.Value;
							_console.ConsoleWriteMembers(directory);
						} else
							throw new NotImplementedException();
						_console.PauseOnDir();
					}

					if(!info.BoundImport.IsEmpty)
					{
						_console.WriteLine("===Bound Import===");
						_console.WriteLine($"ModuleName: {info.BoundImport.ModuleName}");
						foreach(var ffRef in info.BoundImport)
							_console.ConsoleWriteMembers(ffRef);
						_console.PauseOnDir();
					}

					if(!info.Export.IsEmpty)
					{//В файле есть информация о экспортируемых функциях
						_console.WriteLine("===Export Functions===");
						_console.WriteLine($"Module name: {info.Export.DllName}");

						foreach(var func in info.Export.GetExportFunctions())
							_console.ConsoleWriteMembers(func);
						_console.PauseOnDir();
					}

					if(!info.Import.IsEmpty)
					{//В файле есть информация о импортиуемых модулях
						_console.WriteLine("===Import Modules===");
						foreach(var module in info.Import)
						{
							_console.WriteLine($"Module name: {module.ModuleName}");
							foreach(var func in module)
								_console.ConsoleWriteMembers(func);
						}
						_console.PauseOnDir();
					}
				}
			}
		}

		private void ReadObjInfo(String obj)
		{
			using(ObjFile info = new ObjFile(StreamLoader.FromFile(obj)))
			{
				if(info.IsValid)//Проверка на валидность загруженного файла
				{
					_console.ConsoleWriteMembers(info.FileHeader);

					foreach(var section in info.Sections)
						_console.ConsoleWriteMembers(section);

					foreach(var symbol in info.Symbols)
					{
						_console.ConsoleWriteMembers(symbol);
						/*if(symbol.Name.Short == 0 && symbol.Name.Long != 0)
							Console.WriteLine(info.StringTable[symbol.Name.Long]);*/
					}

					foreach(var str in info.StringTable)
						Console.WriteLine(str);
				}
			}
		}
	}
}