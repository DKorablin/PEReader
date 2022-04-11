using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>Image loader from file or stream</summary>
	[DefaultProperty("Source")]
	[DebuggerDisplay("Source={Source}")]
	public class StreamLoader : IDisposable, IImageLoader
	{
		private BinaryReader _reader;

		/// <summary>File reader</summary>
		private BinaryReader Reader { get { return this._reader; } }

		/// <summary>Module mapped to memory</summary>
		public Boolean IsModuleMapped { get { return false; } }

		/// <summary>Base PE file address</summary>
		public Int64 BaseAddress { get { return 0; } }

		/// <summary>Image source</summary>
		public String Source { get; private set; }

		/// <summary>Required endianness</summary>
		public EndianHelper.Endian Endianness { get; set; }

		/// <summary>Read image from stream</summary>
		/// <param name="input">Stream with image</param>
		/// <param name="source">Source of image</param>
		/// <exception cref="T:ArgumentNullException">input stream is null</exception>
		/// <exception cref="T:ArgumentNullException">souce is null</exception>
		/// <exception cref="T:ArgumentException">stream must be seakable and readable</exception>
		public StreamLoader(Stream input, String source)
		{
			if(input == null)
				throw new ArgumentNullException("input");
			if(String.IsNullOrEmpty(source))
				throw new ArgumentNullException("source");
			if(!input.CanSeek || !input.CanRead)
				throw new ArgumentException("The stream does not support reading and/or seeking");

			this.Source = source;
			this._reader = new BinaryReader(input);
		}

		/// <summary>Read PE image from file</summary>
		/// <param name="filePath">Path to the file</param>
		/// <exception cref="T:ArgumentNullException">filePath is null or empty string</exception>
		/// <exception cref="T:FileNotFoundException">file not found</exception>
		/// <returns>PE loader</returns>
		public static StreamLoader FromFile(String filePath)
		{
			if(String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");
			else if(!File.Exists(filePath))
				throw new FileNotFoundException(String.Format("File {0} not found", filePath));

			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return new StreamLoader(stream, filePath);
		}

		/// <summary>Read PE image from memory</summary>
		/// <param name="input">Array of bytes</param>
		/// <param name="sourceName">Custom source name</param>
		/// <returns>PE loader</returns>
		public static StreamLoader FromMemory(Byte[] input, String sourceName)
		{
			if(input == null || input.Length == 0)
				throw new ArgumentNullException("input");

			MemoryStream stream = new MemoryStream(input, false);
			return new StreamLoader(stream, sourceName);
		}

		/// <summary>Get bytes from specific padding and specific length</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <param name="length">Length of bytes to read</param>
		/// <exception cref="T:ArgumentOutOfRangeException">padding + length more than size of image</exception>
		/// <returns>Readed bytes</returns>
		public virtual Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			Stream stream = this.Reader.BaseStream;
			if(padding + length > stream.Length)
				throw new ArgumentOutOfRangeException("padding");

			stream.Seek(checked((Int64)padding), SeekOrigin.Begin);
			return this.Reader.ReadBytes((Int32)length);
		}

		/// <summary>Get structure from specific padding from the beginning of the image</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>Readed structure from image</returns>
		public virtual T PtrToStructure<T>(UInt32 padding) where T : struct
		{
			Byte[] bytes = this.ReadBytes(padding, (UInt32)Marshal.SizeOf(typeof(T)));

			EndianHelper.AdjustEndianness(typeof(T), bytes, this.Endianness);

			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			try
			{
				T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
				return result;
			} finally
			{
				handle.Free();
			}
		}

		/// <summary>Get ACSII string from specific padding from the beginning of the image</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <exception cref="T:ArgumentOutOfRangeException">padding more than size of image</exception>
		/// <returns>String from pointer</returns>
		public virtual String PtrToStringAnsi(UInt32 padding)
		{
			Stream stream = this.Reader.BaseStream;
			if(padding > stream.Length)
				throw new ArgumentOutOfRangeException("padding");

			stream.Seek(checked((Int64)padding), SeekOrigin.Begin);
			List<Byte> result = new List<Byte>();

			Byte b = this.Reader.ReadByte();
			while(b != 0x00)
			{
				result.Add(b);
				b = this.Reader.ReadByte();
			}
			return Encoding.ASCII.GetString(result.ToArray());
		}

		/// <summary>Close PE reader</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Dispose managed objects</summary>
		/// <param name="disposing">Dispose managed objects</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this._reader != null)
			{
				this._reader.Close();
				this._reader = null;
			}
		}
	}
}