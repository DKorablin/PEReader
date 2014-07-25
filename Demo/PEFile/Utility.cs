#region Licence Information
/*       
 * http://www.codeplex.com/NetMassDownloader To Get The Latest Version
 *     
 * Copyright 2008 Kerem Kusmezer(keremskusmezer@gmail.com) And John Robbins (john@wintellect.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * 
*/
#endregion

#region Comments
/*------------------------------------------------------------------------------
 * SourceCodeDownloader - Kerem Kusmezer (keremskusmezer@gmail.com)
 *                        
 * 
 * Download all the .NET Reference Sources and PDBs to pre-populate your 
 * symbol server! No more waiting as each file downloads individually while
 * debugging.
 * ---------------------------------------------------------------------------*/
#endregion

#region Imported Libraries
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;
#endregion

namespace DownloadLibrary.Classes
{
	public static class Utility
	{
		/// <summary>
		/// Deserializes The Given Xml String To Given Type
		/// </summary>
		/// <param name="type">Target Type To The Object Should Be Deserialized</param>
		/// <param name="data">Xml Data Containing The Serialized Object</param>
		/// <returns>Null If The Object Cannot Be Deserialized, If success returns the deserialized object</returns>
		public static object Deserialize(Type type, string data)
		{
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(type);
				return xmlSerializer.Deserialize(new XmlTextReader(new StringReader(data)));
			} catch(Exception exc)
			{
				Debug.WriteLine(exc.ToString());
				return null;
			}
		}

		public static string PreparePath(string path)
		{
			string[] pathWithoutDrive = path.Split(System.IO.Path.VolumeSeparatorChar);
			if(pathWithoutDrive.Length == 2)
			{
				if(pathWithoutDrive[1].StartsWith("\\"))
				{
					return pathWithoutDrive[1].Substring(1);
				}
			}
			return String.Empty;
		}
		public static string CurrentDirectoryPath()
		{
			return CleanupPath(Environment.CurrentDirectory);
		}
		public static string CleanupPath(string path)
		{
			if(!path.EndsWith("\\"))
			{
				path += "\\";
			}
			return path;
		}
		public class SearchLocation
		{

			private byte[] m_foundContent;

			public byte[] FoundContent
			{
				get { return m_foundContent; }
			}
			private int m_beginPosition;

			public int BeginPosition
			{
				get { return m_beginPosition; }
			}

			public override string ToString()
			{
				if(m_foundContent != null)
				{
					return System.Text.Encoding.ASCII.GetString(m_foundContent);
				} else
				{
					return String.Empty;
				}
			}

			public SearchLocation(byte[] foundContent, int beginPosition)
			{
				m_foundContent = foundContent;
				m_beginPosition = beginPosition;
			}

		}
		public static byte[] HexToByte(string hexaDecimalString)
		{
			int NumberChars = hexaDecimalString.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for(int i = 0;i < NumberChars;i += 2)
			{
				bytes[i / 2] = Convert.ToByte(hexaDecimalString.Substring(i, 2), 16);
			}
			return bytes;
		}
		public static String ByteArrayToHex(byte[] sourceArray)
		{
			StringBuilder byteStorageForSearch = new StringBuilder();

			for(long i = 0;i < sourceArray.LongLength;i++)
			{
				byteStorageForSearch.AppendFormat("{0:x2}", sourceArray[i]);
			}
			return byteStorageForSearch.ToString();
		}

		/*public static PDBWebClient GetWebClientWithCookie(Match ProxyMatch)
		{

			PDBWebClient specialWebClient =
				(ProxyMatch != null ?
				new PDBWebClient(ProxyMatch.Groups["proxyAddress"].Value,
								 ProxyMatch.Groups["username"].Value,
								 ProxyMatch.Groups["password"].Value,
								 (ProxyMatch.Groups["domain"] != null ? ProxyMatch.Groups["domain"].Value : String.Empty)) : new PDBWebClient());

			AddHeadersIfNecessary(specialWebClient);
			return specialWebClient;
		}*/

		/*
		public static PDBWebClient GetWebClientWithCookie()
		{
			PDBWebClient specialWebClient = new PDBWebClient();
			if (specialWebClient.Headers["User-Agent"] == null)
			{
				specialWebClient.Headers.Add("User-Agent", Constants.userAgentHeader);
			}
			if (!String.IsNullOrEmpty(EulaContents.GetEulaCookie()))
			{
				if (specialWebClient.Headers["Cookie"] == null)
				{
					specialWebClient.Headers.Add("Cookie", EulaContents.GetEulaCookie());
				}
			}        
			return specialWebClient;
		}
		*/

		public static SearchLocation ExtractFromByteArray(byte[] searchArray,
												  string beginHexaDecimalString,
												  string endHexaDecimalString,
												  int searchBeginPosition)
		{
			SearchLocation resultLocation;

			String searchArrayAsHex = ByteArrayToHex(searchArray);
			int beginLocation =
				searchArrayAsHex.IndexOf(beginHexaDecimalString, searchBeginPosition);
			int endLocation =
				searchArrayAsHex.IndexOf(endHexaDecimalString, beginLocation);
			if(beginLocation < endLocation)
			{
				resultLocation = new SearchLocation(
				HexToByte(searchArrayAsHex.Substring(beginLocation, endLocation - beginLocation)), beginLocation);
			} else
			{
				resultLocation = new SearchLocation(null, -1);
			}
			return resultLocation;
		}

		public static byte[] ReplaceInByteArray(byte[] sourceArray, byte[] statementToFind, byte[] statementToReplace)
		{
			StringBuilder byteStorageForSearch = new StringBuilder();

			for(long i = 0;i < sourceArray.LongLength;i++)
			{
				byteStorageForSearch.AppendFormat("{0:x2}", sourceArray[i]);
			}

			StringBuilder findForSearch = new StringBuilder();
			for(long x = 0;x < statementToFind.LongLength;x++)
			{
				findForSearch.AppendFormat("{0:x2}", statementToFind[x]);
			}

			StringBuilder replaceForSearch = new StringBuilder();
			for(long y = 0;y < statementToReplace.LongLength;y++)
			{
				replaceForSearch.AppendFormat("{0:x2}", statementToReplace[y]);
			}

			string byteStorageContainer = byteStorageForSearch.ToString();
			byteStorageContainer = byteStorageContainer.Replace(findForSearch.ToString(), replaceForSearch.ToString());
			return ToByteArray(byteStorageContainer);

		}

		//public static byte[] ReplaceInByteArray(byte[] sourceArray, byte[] statementToFind, byte[] statementToReplace)
		//{
		//    StringBuilder byteStorageForSearch = new StringBuilder();

		//    for (long i = 0; i < sourceArray.LongLength; i++)
		//    {
		//        byteStorageForSearch.AppendFormat("{0:x2}", sourceArray[i]);
		//    }

		//    StringBuilder findForSearch = new StringBuilder();
		//    for (long x = 0; x < statementToFind.LongLength; x++)
		//    {
		//        findForSearch.AppendFormat("{0:x2}", statementToFind[x]);
		//    }

		//    StringBuilder replaceForSearch = new StringBuilder();
		//    for (long y = 0; y < statementToReplace.LongLength; y++)
		//    {
		//        replaceForSearch.AppendFormat("{0:x2}", statementToReplace[y]);
		//    }

		//    string byteStorageContainer = byteStorageForSearch.ToString();
		//    byteStorageContainer = byteStorageContainer.Replace(findForSearch.ToString(), replaceForSearch.ToString());
		//    return Utility.ToByteArray(byteStorageContainer);

		//}

		public static String ToStringHexFromString(String SourceString)
		{
			return Utility.ToStringHex(System.Text.Encoding.ASCII.GetBytes(SourceString));
		}
		public static String ToStringHex(byte[] sourceArray)
		{
			StringBuilder byteStorageForSearch = new StringBuilder();

			for(long i = 0;i < sourceArray.LongLength;i++)
			{
				byteStorageForSearch.AppendFormat("{0:x2}", sourceArray[i]);
			}
			return byteStorageForSearch.ToString();
		}
		public static byte[] ToByteArray(String HexString)
		{
			int NumberChars = HexString.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for(int i = 0;i < NumberChars;i += 2)
			{
				bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
			}
			return bytes;
		}

		public static void AddHeadersIfNecessary(WebClient wc)
		{
			AddUserAgentHeaderIfNecessary(wc);
			AddEulaCookieHeaderIfNecessary(wc);
		}

		public static void AddUserAgentHeaderIfNecessary(WebClient wc)
		{
			/*const string header = "User-Agent";
			if(wc.Headers[header] == null)
			{
				wc.Headers.Add(header, FrameworkVersionData.RelevantVersionData.GetUserAgentVersion());
			}*/
		}
		public static void AddEulaCookieHeaderIfNecessary(WebClient wc)
		{
			/*const string header = "Cookie";
			if(wc.Headers[header] == null)
			{
				wc.Headers.Add(header, EulaContents.GetEulaCookie());
			}*/
		}

		public static string GetProxyAddressFrom(Match m)
		{
			return GetMatchGroupValue(m, "proxyAddress");
		}
		public static string GetUserNameFrom(Match m)
		{
			return GetMatchGroupValue(m, "username");
		}
		public static string GetPasswordFrom(Match m)
		{
			return GetMatchGroupValue(m, "password");
		}
		public static string GetDomainFrom(Match m)
		{
			return GetMatchGroupValue(m, "domain");
		}
		private static string GetMatchGroupValue(Match m, string groupName)
		{
			Group g = m.Groups[groupName];
			return (g != null) ? g.Value : string.Empty;
		}


		public static void EnsureDir(string path)
		{
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		public static void EnsureParentDir(string path)
		{
			string parentDirPath = Path.GetDirectoryName(path);
			if(!Directory.Exists(parentDirPath))
			{
				Directory.CreateDirectory(parentDirPath);
			}
		}
		public static bool MoveFileIfExists(string srcPath, string targetPath)
		{
			bool exists = File.Exists(srcPath);
			if(exists)
			{
				File.Move(srcPath, targetPath);
			}
			return exists;
		}
	}
}