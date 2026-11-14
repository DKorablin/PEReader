using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlphaOmega.Debug.NTDirectory.Resources
{
	/// <summary>Resource Message table class</summary>
	[DefaultProperty(nameof(NumberOfBlocks))]
	public class ResourceMessageTable : ResourceBase, IEnumerable<ResourceMessageTable.MessageResourceEntry>
	{
		/// <summary>Message descriptor</summary>
		public struct MessageResourceEntry
		{
			/// <summary>ID of entry that used by eventLog</summary>
			public UInt32 EntryId;

			/// <summary>Message for eventLog</summary>
			public String EntryName;
		}

		/// <summary>Each block contains array of messages</summary>
		public UInt32 NumberOfBlocks => PinnedBufferReader.BytesToStructure<UInt32>(base.Directory.GetData(), 0);

		/// <summary>Create instance of message table resource class</summary>
		/// <param name="directory">Resource directory</param>
		public ResourceMessageTable(ResourceDirectory directory)
			: base(directory, WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MESSAGETABLE) { }

		/// <summary>Get message blocks</summary>
		/// <returns>Array of message blocks</returns>
		public IEnumerable<WinNT.Resource.MESSAGE_RESOURCE_BLOCK> GetMessageBlocks()
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
				foreach(WinNT.Resource.MESSAGE_RESOURCE_BLOCK block in this.GetMessageBlocks(reader))
					yield return block;
		}

		private IEnumerable<WinNT.Resource.MESSAGE_RESOURCE_BLOCK> GetMessageBlocks(PinnedBufferReader reader)
		{
			UInt32 numberOfBlocks = this.NumberOfBlocks;

			UInt32 padding = sizeof(UInt32);//Skipping NumberOfBlocks size
			for(Int32 loop = 0; loop < numberOfBlocks; loop++)
				yield return reader.BytesToStructure<WinNT.Resource.MESSAGE_RESOURCE_BLOCK>(ref padding);
		}

		/// <summary>Get all messages from block</summary>
		/// <param name="block">Message block from which read all messages</param>
		/// <returns>Messages array</returns>
		public IEnumerable<ResourceMessageTable.MessageResourceEntry> GetMessageBlockEntries(WinNT.Resource.MESSAGE_RESOURCE_BLOCK block)
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
				foreach(ResourceMessageTable.MessageResourceEntry entry in this.GetMessageBlockEntries(reader, block))
					yield return entry;
		}

		/// <summary>Get message block entries from starting address</summary>
		/// <param name="reader">Mapped bytes</param>
		/// <param name="block">message block header</param>
		/// <remarks>Trailing NUL terminator and DWORD alignment padding are removed from the message text</remarks>
		/// <exception cref="NotSupportedException">Unknown string encoding specified</exception>
		/// <returns>Messages from the block</returns>
		private IEnumerable<ResourceMessageTable.MessageResourceEntry> GetMessageBlockEntries(PinnedBufferReader reader, WinNT.Resource.MESSAGE_RESOURCE_BLOCK block)
		{
			if(block.LowId > block.HighId)
				yield break;

			System.Text.Encoding ansiEncoding = null;
			UInt32 padding = block.OffsetToEntries;
			UInt32 entryId = block.LowId;

			while(true)
			{
				WinNT.Resource.MESSAGE_RESOURCE_ENTRY entry = reader.BytesToStructure<WinNT.Resource.MESSAGE_RESOURCE_ENTRY>(ref padding);
				Byte[] bytes = reader.GetBytes(padding, entry.MessageLength);

				String message;
				switch(entry.Flags)
				{
				case WinNT.Resource.ResourceEncodingType.Ansi:
					if(ansiEncoding == null)
						ansiEncoding = this.GetAnsiEncoding();
					message = ansiEncoding.GetString(bytes);
					break;
				case WinNT.Resource.ResourceEncodingType.Unicode:
					message = System.Text.Encoding.Unicode.GetString(bytes);
					break;
				case WinNT.Resource.ResourceEncodingType.Utf8:
					message = System.Text.Encoding.UTF8.GetString(bytes);
					break;
				default:
					throw new NotSupportedException($"Unknown message resource encoding 0x{(UInt16)entry.Flags:X4} for message ID 0x{entryId:X}");
				}

				yield return new ResourceMessageTable.MessageResourceEntry() { EntryId = entryId, EntryName = message.TrimEnd('\0'), };
				padding += entry.MessageLength;

				if(entryId == block.HighId)
					break;//Checked before increment to avoid wrap-around when HighId == UInt32.MaxValue
				entryId++;
			}
		}

		/// <summary>Get encoding for ANSI messages using the resource code page</summary>
		/// <remarks>Falls back to the system default ANSI code page when the resource code page is 0 or not supported</remarks>
		/// <returns>Encoding for ANSI message entries</returns>
		private System.Text.Encoding GetAnsiEncoding()
		{
			UInt32 codePage = base.Directory.DataEntry.Value.CodePage;
			if(codePage == 0)
				return System.Text.Encoding.Default;

			try
			{
				return System.Text.Encoding.GetEncoding((Int32)codePage);
			} catch(ArgumentException)
			{//Invalid code page
				return System.Text.Encoding.Default;
			} catch(NotSupportedException)
			{//TODO: Code page is not installed
				return System.Text.Encoding.Default;
			}
		}

		/// <summary>Get all messages from resource directory</summary>
		/// <returns>Messages array</returns>
		public IEnumerator<ResourceMessageTable.MessageResourceEntry> GetEnumerator()
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
				foreach(var block in this.GetMessageBlocks(reader))
					foreach(var entry in this.GetMessageBlockEntries(reader, block))
						yield return entry;
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> this.GetEnumerator();
	}
}