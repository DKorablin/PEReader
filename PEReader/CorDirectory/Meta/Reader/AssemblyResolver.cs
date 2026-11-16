using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug.CorDirectory.Meta.Reader
{
	/// <summary>Default assembly resolver</summary>
	/// <remarks>
	/// Probes the folder of the requesting file (top level only) and then installed .NET shared frameworks (newest version first).
	/// All opened files are cached and owned by the resolver until it is disposed.
	/// </remarks>
	public class AssemblyResolver : IAssemblyResolver, IDisposable
	{
		/// <summary>Public key of the ECMA standard. Its token is a well-known constant and not computed from the key (ECMA-335 II.6.2.1.3)</summary>
		private static readonly Byte[] EcmaPublicKey = new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, };
		private static readonly Byte[] EcmaPublicKeyToken = new Byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89, };

		/// <summary>Opened files by full path (null for files that are not valid .NET assemblies)</summary>
		private readonly Dictionary<String, PEFile> _openedFiles = new Dictionary<String, PEFile>(StringComparer.OrdinalIgnoreCase);
		/// <summary>Resolution results by reference name and probing folder (null for not resolved references)</summary>
		private readonly Dictionary<String, PEFile> _resolved = new Dictionary<String, PEFile>(StringComparer.OrdinalIgnoreCase);
		private String[] _sharedFrameworkFolders;
		private Boolean _disposed;

		/// <summary>Installed .NET shared framework folders: <c>dotnet\shared\&lt;framework&gt;\&lt;version&gt;</c> sorted from the newest version</summary>
		protected String[] SharedFrameworkFolders
			=> this._sharedFrameworkFolders ?? (this._sharedFrameworkFolders = this.FindSharedFrameworkFolders());

		/// <inheritdoc/>
		public PEFile Resolve(AssemblyName reference, PEFile requestingFile)
		{
			_ = reference ?? throw new ArgumentNullException(nameof(reference));
			if(this._disposed)
				throw new ObjectDisposedException(nameof(AssemblyResolver));
			if(String.IsNullOrEmpty(reference.Name))
				return null;

			String key = reference.FullName + "|" + GetDirectory(requestingFile);
			if(this._resolved.TryGetValue(key, out PEFile result))
				return result;

			result = null;
			foreach(String filePath in this.GetProbingPaths(reference, requestingFile))
			{
				PEFile candidate = this.GetOrOpen(filePath);
				if(candidate != null && this.IsMatch(reference, candidate.ComDescriptor.MetaData.StreamTables.Assembly[0]))
				{
					result = candidate;
					break;
				}
			}

			this._resolved[key] = result;
			return result;
		}

		/// <summary>Gets list of files where the referenced assembly can be found</summary>
		/// <param name="reference">Name of the referenced assembly</param>
		/// <param name="requestingFile">File that references the assembly</param>
		/// <returns>Candidate file paths in the probing order. Paths can point to missing files.</returns>
		protected virtual IEnumerable<String> GetProbingPaths(AssemblyName reference, PEFile requestingFile)
		{
			String name = reference.Name;
			if(name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				yield break;

			String directory = GetDirectory(requestingFile);
			if(directory != null)
			{
				yield return Path.Combine(directory, name + ".dll");
				yield return Path.Combine(directory, name + ".exe");
			}

			foreach(String folder in this.SharedFrameworkFolders)
				yield return Path.Combine(folder, name + ".dll");
		}

		/// <summary>Checks that the found assembly satisfies the reference</summary>
		/// <param name="reference">Name of the referenced assembly</param>
		/// <param name="definition">Assembly definition from the found file</param>
		/// <returns>Names, cultures and public key tokens are equal and the found version is the same or newer</returns>
		protected virtual Boolean IsMatch(AssemblyName reference, AssemblyRow definition)
		{
			if(!String.Equals(reference.Name, definition.Name, StringComparison.OrdinalIgnoreCase))
				return false;

			String referenceCulture = reference.CultureInfo?.Name ?? String.Empty;
			String definitionCulture = definition.Locale ?? String.Empty;
			if(String.Equals(definitionCulture, "neutral", StringComparison.OrdinalIgnoreCase))
				definitionCulture = String.Empty;
			if(!String.Equals(referenceCulture, definitionCulture, StringComparison.OrdinalIgnoreCase))
				return false;

			if(reference.Version != null && definition.Version < reference.Version)
				return false;

			if((reference.Flags & AssemblyNameFlags.Retargetable) == 0)
			{// Retargetable references can be satisfied by an assembly with a different public key
				Byte[] referenceToken = GetPublicKeyToken(reference);
				if(referenceToken != null && referenceToken.Length > 0)
				{
					Byte[] definitionToken = ComputePublicKeyToken(definition.PublicKey);
					if(definitionToken != null && !AreEqual(referenceToken, definitionToken))
						return false;
				}
			}

			return true;
		}

		/// <summary>Opens a file that can contain the referenced assembly</summary>
		/// <param name="filePath">Path to the PE file</param>
		/// <returns>Opened PE file that resolves its own references using this resolver</returns>
		protected virtual PEFile Open(String filePath)
			=> new PEFile(filePath, StreamLoader.FromFile(filePath), new PEReaderOptions() { ResolveAssemblies = true, AssemblyResolver = this, });

		/// <summary>Gets installed .NET root folders (folders that contain the <c>shared</c> sub folder)</summary>
		/// <returns>Candidate .NET root folders. Folders can be missing or duplicated.</returns>
		protected virtual IEnumerable<String> GetDotNetRoots()
		{
			foreach(String variable in new String[] { "DOTNET_ROOT", "DOTNET_ROOT(x86)", })
			{
				String path = Environment.GetEnvironmentVariable(variable);
				if(!String.IsNullOrEmpty(path))
					yield return path;
			}

			foreach(String variable in new String[] { "ProgramFiles", "ProgramW6432", "ProgramFiles(x86)", })
			{
				String path = Environment.GetEnvironmentVariable(variable);
				if(!String.IsNullOrEmpty(path))
					yield return Path.Combine(path, "dotnet");
			}

			yield return "/usr/share/dotnet";
			yield return "/usr/lib/dotnet";
			yield return "/usr/local/share/dotnet";
		}

		private PEFile GetOrOpen(String filePath)
		{
			String fullPath;
			try
			{
				fullPath = Path.GetFullPath(filePath);
			} catch(Exception)
			{// Invalid path
				return null;
			}

			if(this._openedFiles.TryGetValue(fullPath, out PEFile result))
				return result;
			if(!File.Exists(fullPath))
				return null;

			result = null;
			try
			{
				result = this.Open(fullPath);
				if(!result.Header.IsValid
					|| result.ComDescriptor?.MetaData?.StreamTables == null
					|| result.ComDescriptor.MetaData.StreamTables.Assembly.Table.RowsCount == 0)
				{// Not a .NET assembly (or a netmodule without the Assembly row)
					result.Dispose();
					result = null;
				}
			} catch(Exception)
			{// Unreadable or broken file is skipped as not found
				result?.Dispose();
				result = null;
			}

			this._openedFiles[fullPath] = result;
			return result;
		}

		private String[] FindSharedFrameworkFolders()
		{
			List<String> roots = new List<String>();
			List<KeyValuePair<Version, String>> folders = new List<KeyValuePair<Version, String>>();
			foreach(String root in this.GetDotNetRoots())
			{
				String sharedPath;
				try
				{
					sharedPath = Path.GetFullPath(Path.Combine(root, "shared"));
				} catch(Exception)
				{
					continue;
				}

				if(roots.Exists(r => String.Equals(r, sharedPath, StringComparison.OrdinalIgnoreCase)) || !Directory.Exists(sharedPath))
					continue;
				roots.Add(sharedPath);

				try
				{
					foreach(String frameworkPath in Directory.GetDirectories(sharedPath))
						foreach(String versionPath in Directory.GetDirectories(frameworkPath))
						{
							Version version = ParseFrameworkVersion(Path.GetFileName(versionPath));
							if(version != null)
								folders.Add(new KeyValuePair<Version, String>(version, versionPath));
						}
				} catch(UnauthorizedAccessException)
				{
				} catch(IOException)
				{
				}
			}

			folders.Sort((a, b) =>
			{
				Int32 compare = b.Key.CompareTo(a.Key);
				return compare == 0 ? String.CompareOrdinal(a.Value, b.Value) : compare;
			});

			String[] result = new String[folders.Count];
			for(Int32 loop = 0; loop < result.Length; loop++)
				result[loop] = folders[loop].Value;
			return result;
		}

		/// <summary>Parses shared framework folder name: <c>9.0.11</c>, <c>10.0.0-preview.7.25380.108</c></summary>
		private static Version ParseFrameworkVersion(String folderName)
		{
			Int32 index = folderName.IndexOfAny(new Char[] { '-', '+', });
			if(index >= 0)
				folderName = folderName.Substring(0, index);

			try
			{
				return new Version(folderName);
			} catch(Exception)
			{// Not a version folder
				return null;
			}
		}

		private static String GetDirectory(PEFile file)
		{
			String source = file?.Source;
			if(String.IsNullOrEmpty(source))
				return null;

			try
			{
				String result = Path.GetDirectoryName(Path.GetFullPath(source));
				return String.IsNullOrEmpty(result) || !Directory.Exists(result)
					? null
					: result;
			} catch(Exception)
			{// Source is not a file path (memory image, etc.)
				return null;
			}
		}

		private static Byte[] GetPublicKeyToken(AssemblyName name)
		{
			Byte[] publicKey = name.GetPublicKey();
			return publicKey != null && publicKey.Length > 0
				? ComputePublicKeyToken(publicKey)
				: name.GetPublicKeyToken();
		}

		/// <summary>Public key token is the last 8 bytes of SHA-1 hash of the public key in reverse order (ECMA-335 II.6.2.1.3)</summary>
		/// <returns>Token, empty array for not signed assemblies or null if the token can't be computed</returns>
		private static Byte[] ComputePublicKeyToken(Byte[] publicKey)
		{
			if(publicKey == null || publicKey.Length == 0)
				return new Byte[] { };
			if(AreEqual(publicKey, EcmaPublicKey))
				return EcmaPublicKeyToken;

			try
			{
				Byte[] hash;
				using(SHA1 sha1 = SHA1.Create())
					hash = sha1.ComputeHash(publicKey);

				Byte[] result = new Byte[8];
				for(Int32 loop = 0; loop < result.Length; loop++)
					result[loop] = hash[hash.Length - 1 - loop];
				return result;
			} catch(Exception)
			{// SHA-1 is not available (FIPS policy, etc.)
				return null;
			}
		}

		private static Boolean AreEqual(Byte[] a, Byte[] b)
		{
			if(a.Length != b.Length)
				return false;
			for(Int32 loop = 0; loop < a.Length; loop++)
				if(a[loop] != b[loop])
					return false;
			return true;
		}

		/// <summary>Closes all opened files</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary><see cref="IDisposable"/> pattern implementation</summary>
		/// <param name="disposing">Is object disposing or called from destructor</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(this._disposed)
				return;

			if(disposing)
			{
				foreach(PEFile file in this._openedFiles.Values)
					file?.Dispose();
				this._openedFiles.Clear();
				this._resolved.Clear();
			}
			this._disposed = true;
		}
	}
}
