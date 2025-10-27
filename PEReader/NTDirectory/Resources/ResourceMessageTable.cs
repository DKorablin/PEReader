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
				return this.GetMessageBlocks(reader);
		}

		private IEnumerable<WinNT.Resource.MESSAGE_RESOURCE_BLOCK> GetMessageBlocks(PinnedBufferReader reader)
		{
			UInt32 padding = sizeof(UInt32);//Skipping NumberOfBlocks size
			for(Int32 loop = 0;loop < this.NumberOfBlocks;loop++)
				yield return reader.BytesToStructure<WinNT.Resource.MESSAGE_RESOURCE_BLOCK>(ref padding);
		}

		/// <summary>Get all messages from block</summary>
		/// <param name="block">Message block from witch read all messages</param>
		/// <returns>Messages array</returns>
		public IEnumerable<ResourceMessageTable.MessageResourceEntry> GetMessageBlockEntries(WinNT.Resource.MESSAGE_RESOURCE_BLOCK block)
		{
			using(PinnedBufferReader reader = base.CreateDataReader())
				return this.GetMessageBlockEntries(reader, block);
		}

		/// <summary>Get message block entries from starting address</summary>
		/// <param name="reader">Mapped bytes</param>
		/// <param name="block">message block header</param>
		/// <exception cref="NotImplementedException">Unknown string encoding specified</exception>
		/// <returns></returns>
		private IEnumerable<ResourceMessageTable.MessageResourceEntry> GetMessageBlockEntries(PinnedBufferReader reader, WinNT.Resource.MESSAGE_RESOURCE_BLOCK block)
		{
			UInt32 padding = block.OffsetToEntries;
			UInt32 entryId = block.LowId;

			while(entryId <= block.HighId)
			{
				WinNT.Resource.MESSAGE_RESOURCE_ENTRY entry = reader.BytesToStructure<WinNT.Resource.MESSAGE_RESOURCE_ENTRY>(ref padding);

				String message;
				switch(entry.Flags)
				{
				case WinNT.Resource.ResourceEncodingType.Ansi:
					message = System.Text.Encoding.ASCII.GetString(reader.GetBytes(padding, entry.MessageLength));
					break;
				case WinNT.Resource.ResourceEncodingType.Unicode:
					message = System.Text.Encoding.Unicode.GetString(reader.GetBytes(padding, entry.MessageLength));
					break;
				default:
					throw new NotImplementedException();
				}
				yield return new ResourceMessageTable.MessageResourceEntry() { EntryId = entryId, EntryName = message, };
				padding += entry.MessageLength;
				entryId++;
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