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
		/// <param name="index">Index in the heap</param>
		/// <returns>Object from heap by index</returns>
		public virtual T this[Int32 index] { get { return this.GetData()[index]; } }

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
		public virtual SortedList<Int32, T> GetData()
		{
			if(this._data == null)
				this._data = this.DataBind();
			return this._data;
		}

		/// <summary>Bind the stream data to the structured form</summary>
		/// <returns>Sorted list array</returns>
		protected abstract SortedList<Int32, T> DataBind();
	}
}