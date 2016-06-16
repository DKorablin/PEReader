using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.CorDirectory.Meta
{
	/// <summary>Typed stream header</summary>
	/// <typeparam name="T">Type of the stream data</typeparam>
	public abstract class StreamHeaderTyped<T> : StreamHeader
	{
		private SortedList<Int32, T> _data;

		/// <summary>Gets object by index</summary>
		/// <param name="pointer">Index in the heap</param>
		/// <returns>Object from heap by index</returns>
		public virtual T this[Int32 pointer]
		{
			get
			{
				T result;
				SortedList<Int32, T> data = this.GetData();
				if(data.TryGetValue(pointer, out result))
				{
					return result;
				} else
				{
					return this.GetDataByPointer(pointer);
				}
			}
		}

		/// <summary>heap data</summary>
		public IEnumerable<T> Data { get { return this.GetData().Values; } }

		/// <summary>Create instance of the typed stream header</summary>
		/// <param name="loader">MetaData</param>
		/// <param name="header">Stream header</param>
		public StreamHeaderTyped(MetaData loader, Cor.STREAM_HEADER header)
			: base(loader, header)
		{
		}

		/// <summary>Gets data in the sorted array form</summary>
		/// <returns>Sorted array from the stream</returns>
		protected virtual SortedList<Int32, T> GetData()
		{
			return this._data == null
				? this._data = this.DataBind()
				: this._data;
		}

		/// <summary>
		/// The .NET specification allows a string reference to point anywhere in the string heap, not just to thestart of a string.
		/// Therefore, it is possible (although probably not very useful) to create an assembly in which some strings overlap with each other.
		/// </summary>
		/// <param name="pointer">Pointer in the heap</param>
		/// <returns>Data by pointer</returns>
		protected abstract T GetDataByPointer(Int32 pointer);

		/// <summary>Bind the stream data to the structured form</summary>
		/// <returns>Sorted list array</returns>
		protected abstract SortedList<Int32, T> DataBind();
	}
}