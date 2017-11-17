using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
	public interface IReadOnlySupport
	{
		void SwitchToReadOnly();
		void SwitchToWritable();

		void Truncate();
	}

	[DebuggerDisplay("Count = {Count}")]
	public class ObservableList<T> : IList<T>, ICollection<T>, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlySupport
	{
		private class SynchronizedList : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			private ObservableList<T> _list;
			private object _root;
			internal SynchronizedList(ObservableList<T> list)
			{
				this._list = list;
				this._root = ((ICollection)list).SyncRoot;
			}
			public int Count
			{
				get
				{
					int count;
					lock (this._root)
						count = this._list.Count;
					return count;
				}
			}
			public bool IsReadOnly
			{
				get { return ((ICollection<T>)this._list).IsReadOnly; }
			}
			public T this[int index]
			{
				get
				{
					lock (this._root)
						return this._list[index];
				}
				set
				{
					lock (this._root)
						this._list[index] = value;
				}
			}
			public void Add(T item)
			{
				lock (this._root)
					this._list.Add(item);
			}
			public void Clear()
			{
				lock (this._root)
					this._list.Clear();
			}
			public bool Contains(T item)
			{
				bool result;
				lock (this._root)
					result = this._list.Contains(item);
				return result;
			}
			public void CopyTo(T[] array, int arrayIndex)
			{
				lock (this._root)
					this._list.CopyTo(array, arrayIndex);
			}
			public bool Remove(T item)
			{
				bool result;
				lock (this._root)
					result = this._list.Remove(item);
				return result;
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				IEnumerator result;
				lock (this._root)
					result = this._list.GetEnumerator();
				return result;
			}
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				IEnumerator<T> enumerator;
				lock (this._root)
					enumerator = ((IEnumerable<T>)this._list).GetEnumerator();
				return enumerator;
			}
			public int IndexOf(T item)
			{
				int result;
				lock (this._root)
					result = this._list.IndexOf(item);
				return result;
			}
			public void Insert(int index, T item)
			{
				lock (this._root)
					this._list.Insert(index, item);
			}
			public void RemoveAt(int index)
			{
				lock (this._root)
					this._list.RemoveAt(index);
			}
		}

		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private ObservableList<T> list;
			private int index;
			private int version;
			private T current;

			public T Current
			{

				get { return this.current; }
			}

			object IEnumerator.Current
			{
				get
				{
					if (this.index == 0 || this.index == this.list._size + 1)
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					return this.Current;
				}
			}

			internal Enumerator(ObservableList<T> list)
			{
				this.list = list;
				this.index = 0;
				this.version = list._version;
				this.current = default(T);
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				ObservableList<T> list = this.list;
				if (this.version == list._version && this.index < list._size)
				{
					this.current = list._items[this.index];
					this.index++;
					return true;
				}
				return this.MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (this.version != this.list._version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				this.index = this.list._size + 1;
				this.current = default(T);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (this.version != this.list._version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				this.index = 0;
				this.current = default(T);
			}
		}

		public ObservableList()
		{
			this._items = ObservableList<T>._emptyArray;
			this._size = 0;
		}

		public ObservableList(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity", capacity, "Non-negative number required.");

			if (capacity == 0)
			{
				this._items = ObservableList<T>._emptyArray;
				return;
			}
			this._items = new T[capacity];
			this._size = 0;
		}

		public ObservableList(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection", "collection is null.");

			this._size = 0;
			this._items = ObservableList<T>._emptyArray;

			ICollection<T> collection2 = collection as ICollection<T>;
			if (collection2 == null)
			{
				foreach (T item in collection)
					this.Add(item);
				return;
			}
			int count = collection2.Count;
			if (count == 0)
				return;

			this._items = new T[count];
			collection2.CopyTo(this._items, 0);
			this._size = count;
		}

		public static ObservableList<T> FromArray(T[] array)
		{
			return FromArray<ObservableList<T>>(array);
		}

		public static TArray FromArray<TArray>(T[] array) where TArray : ObservableList<T>, new()
		{
			var result = new TArray();
			if (array != null && array.Length > 0)
			{
				result._items = array;
				result._size = array.Length;
			}
			return result;
		}

		private T[] _items;
		private int _size;
		[NonSerialized]
		private int _version;
		[NonSerialized]
		private object _syncRoot;
		private static readonly T[] _emptyArray = new T[0];
		private const int _defaultCapacity = 4;

		private bool suppressListChanged;
		public bool SuppressListChanged
		{
			get { return suppressListChanged; }
			set { suppressListChanged = value; }
		}

		public int Capacity
		{
			get { return this._items.Length; }
			set
			{
				if (value < this._size)
					throw new ArgumentOutOfRangeException("value", value, "capacity was less than the current size.");

				if (value != this._items.Length)
				{
					if (value > 0)
					{
						T[] array = new T[value];
						if (this._size > 0)
							Array.Copy(this._items, 0, array, 0, this._size);
						this._items = array;
						return;
					}
					this._items = ObservableList<T>._emptyArray;
				}
			}
		}

		public int Count
		{
			get { return this._size; }
		}

		protected T[] ArrayInternal
		{
			get { return _items; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}
		bool ICollection<T>.IsReadOnly
		{
			get { return isReadOnly; }
		}

		bool IList.IsReadOnly
		{

			get { return isReadOnly; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get
			{
				if (this._syncRoot == null)
					Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
				return this._syncRoot;
			}
		}

		public T this[int index]
		{
			get
			{
				if (index >= this._size)
				{
					throw new ArgumentOutOfRangeException("Index and count must refer to a location within the string.");
				}
				return this._items[index];
			}
			set
			{
				CheckForReadOnly();
				if (index >= this._size)
				{
					throw new ArgumentOutOfRangeException("Index and count must refer to a location within the string.");
				}
				var oldValue = this._items[index];
				this._items[index] = value;
				this._version++;
				if (!suppressListChanged)
					OnListChanged(ObservableListAction.Replace, index, oldValue);
			}
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				try
				{
					this[index] = (T)((object)value);
				}
				catch (InvalidCastException)
				{
					throw new ArgumentException(string.Format("The value \"{0}\" is not of type \"{1}\" and cannot be used in this generic collection.", value, typeof(T)));
				}
			}
		}

		private static bool IsCompatibleObject(object value)
		{
			return value is T || (value == null && default(T) == null);
		}

		public void SetSize(int size)
		{
			if (Capacity < size)
				Capacity = size;
			this._size = size;
		}

		public void Add(T item)
		{
			CheckForReadOnly();
			if (this._size == this._items.Length)
			{
				this.EnsureCapacity(this._size + 1);
			}
			this._items[this._size++] = item;
			this._version++;
			if (!suppressListChanged)
				OnListChanged(ObservableListAction.Add, this._size - 1, item);
		}

		int IList.Add(object item)
		{
			if (item == null && default(T) != null)
				throw new ArgumentNullException("item", "item is null");
			try
			{
				this.Add((T)((object)item));
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(string.Format("The value \"{0}\" is not of type \"{1}\" and cannot be used in this generic collection.", item, typeof(T)));
			}
			return this.Count - 1;
		}

		public void AddRange(IEnumerable<T> collection)
		{
			this.InsertRange(this._size, collection);
		}

		public ReadOnlyCollection<T> AsReadOnly()
		{
			return new ReadOnlyCollection<T>(this);
		}

		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			if (index < 0)
				throw new ArgumentException("Index is less than 0.", "index");
			if (count < 0)
				throw new ArgumentException("Count is less than 0.", "count");
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			return Array.BinarySearch<T>(this._items, index, count, item, comparer);
		}

		public int BinarySearch(T item)
		{
			return this.BinarySearch(0, this.Count, item, null);
		}

		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return this.BinarySearch(0, this.Count, item, comparer);
		}

		public void Clear()
		{
			CheckForReadOnly();
			if (this._size > 0)
			{
				if (!suppressListChanged)
					OnListChanged(ObservableListAction.Clearing);
				Array.Clear(this._items, 0, this._size);
				this._size = 0;
				if (!suppressListChanged)
					OnListChanged(ObservableListAction.Clear);
			}
			this._version++;
		}

		public bool Contains(T item)
		{
			if (item == null)
			{
				for (int i = 0; i < this._size; i++)
				{
					if (this._items[i] == null)
					{
						return true;
					}
				}
				return false;
			}
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int j = 0; j < this._size; j++)
			{
				if (@default.Equals(this._items[j], item))
				{
					return true;
				}
			}
			return false;
		}

		bool IList.Contains(object item)
		{
			return ObservableList<T>.IsCompatibleObject(item) && this.Contains((T)((object)item));
		}

		public ObservableList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		{
			if (converter == null)
			{
				throw new ArgumentNullException("converter");
			}
			ObservableList<TOutput> list = new ObservableList<TOutput>(this._size);
			for (int i = 0; i < this._size; i++)
			{
				list._items[i] = converter(this._items[i]);
			}
			list._size = this._size;
			return list;
		}

		public void CopyTo(T[] array)
		{
			this.CopyTo(array, 0);
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");
			}
			try
			{
				Array.Copy(this._items, 0, array, arrayIndex, this._size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.");
			}
		}

		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			Array.Copy(this._items, index, array, arrayIndex, count);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(this._items, 0, array, arrayIndex, this._size);
		}
		private void EnsureCapacity(int min)
		{
			if (this._items.Length < min)
			{
				int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
				if (num > 2146435071)
				{
					num = 2146435071;
				}
				if (num < min)
				{
					num = min;
				}
				this.Capacity = num;
			}
		}

		public bool Exists(Predicate<T> match)
		{
			return this.FindIndex(match) != -1;
		}

		public T Find(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			for (int i = 0; i < this._size; i++)
			{
				if (match(this._items[i]))
				{
					return this._items[i];
				}
			}
			return default(T);
		}

		public ObservableList<T> FindAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			ObservableList<T> list = new ObservableList<T>();
			for (int i = 0; i < this._size; i++)
			{
				if (match(this._items[i]))
				{
					list.Add(this._items[i]);
				}
			}
			return list;
		}

		public int FindIndex(Predicate<T> match)
		{
			return this.FindIndex(0, this._size, match);
		}

		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return this.FindIndex(startIndex, this._size - startIndex, match);
		}

		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			if (startIndex > this._size)
				throw new ArgumentOutOfRangeException("startIndex", startIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
			if (count < 0 || startIndex > this._size - count)
				throw new ArgumentOutOfRangeException("count", count, "Count must be positive and count must refer to a location within the string/array/collection.");
			if (match == null)
				throw new ArgumentNullException("match");

			int num = startIndex + count;
			for (int i = startIndex; i < num; i++)
			{
				if (match(this._items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public T FindLast(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException("match");

			for (int i = this._size - 1; i >= 0; i--)
			{
				if (match(this._items[i]))
				{
					return this._items[i];
				}
			}
			return default(T);
		}

		public int FindLastIndex(Predicate<T> match)
		{
			return this.FindLastIndex(this._size - 1, this._size, match);
		}

		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return this.FindLastIndex(startIndex, startIndex + 1, match);
		}

		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException("match");

			if (this._size == 0)
			{
				if (startIndex != -1)
					throw new ArgumentOutOfRangeException("startIndex", startIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			else
			{
				if (startIndex >= this._size)
					throw new ArgumentOutOfRangeException("startIndex", startIndex, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}

			if (count < 0 || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException("count", count, "Count must be positive and count must refer to a location within the string/array/collection.");

			int num = startIndex - count;
			for (int i = startIndex; i > num; i--)
			{
				if (match(this._items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public void ForEach(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException("match");

			int version = this._version;
			int num = 0;
			while (num < this._size && (version == this._version))
			{
				action(this._items[num]);
				num++;
			}

			if (version != this._version)
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
		}

		public ObservableList<T>.Enumerator GetEnumerator()
		{
			return new ObservableList<T>.Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new ObservableList<T>.Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ObservableList<T>.Enumerator(this);
		}

		public ObservableList<T> GetRange(int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

			ObservableList<T> list = new ObservableList<T>(count);
			Array.Copy(this._items, index, list._items, 0, count);
			list._size = count;
			return list;
		}

		public int IndexOf(T item)
		{
			return Array.IndexOf<T>(this._items, item, 0, this._size);
		}

		int IList.IndexOf(object item)
		{
			if (ObservableList<T>.IsCompatibleObject(item))
			{
				return this.IndexOf((T)((object)item));
			}
			return -1;
		}

		public int IndexOf(T item, int index)
		{
			if (index > this._size)
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");

			return Array.IndexOf<T>(this._items, item, index, this._size - index);
		}

		public int IndexOf(T item, int index, int count)
		{
			if (index > this._size)
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
			if (count < 0 || index > this._size - count)
				throw new ArgumentOutOfRangeException("count", count, "Count must be positive and count must refer to a location within the string/array/collection.");

			return Array.IndexOf<T>(this._items, item, index, count);
		}

		public void Insert(int index, T item)
		{
			CheckForReadOnly();
			if (index > this._size)
				throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the List.");

			if (this._size == this._items.Length)
				this.EnsureCapacity(this._size + 1);

			if (index < this._size)
				Array.Copy(this._items, index, this._items, index + 1, this._size - index);

			this._items[index] = item;
			this._size++;
			this._version++;
			if (!suppressListChanged)
				OnListChanged(ObservableListAction.Add, index, item);
		}

		void IList.Insert(int index, object item)
		{
			if (item == null && default(T) != null)
				throw new ArgumentNullException("item", "item is null");

			try
			{
				this.Insert(index, (T)((object)item));
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(string.Format("The value \"{0}\" is not of type \"{1}\" and cannot be used in this generic collection.", item, typeof(T)));
			}
		}

		public void InsertRange(int index, ObservableList<T> collection, int indexFrom, int countForCopy = -1)
		{
			if (indexFrom >= collection.Count)
				throw new ArgumentOutOfRangeException("indexFrom", "indexFrom is out of source collection's range");

			if (countForCopy < 0)
				countForCopy = collection.Count - indexFrom;
			else if (countForCopy > collection.Count - indexFrom)
				throw new ArgumentOutOfRangeException("countForCopy", "countForCopy is more than source collection has items");

			if (countForCopy == 0)
				return;

			this.EnsureCapacity(this._size + countForCopy);

			var srcItems = collection._items;
			Array.Copy(this._items, index, this._items, index + countForCopy, this._size - index);
			Array.Copy(srcItems, indexFrom, this._items, index, countForCopy);
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			CheckForReadOnly();
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (index > this._size)
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");

			int prevSize = _size;
			var prevSuppressListChanged = suppressListChanged;
			suppressListChanged = true;
			try
			{
				ICollection<T> collection2 = collection as ICollection<T>;
				if (collection2 != null)
				{
					int count = collection2.Count;
					if (count > 0)
					{
						this.EnsureCapacity(this._size + count);
						if (index < this._size)
						{
							Array.Copy(this._items, index, this._items, index + count, this._size - index);
						}
						if (this == collection2)
						{
							Array.Copy(this._items, 0, this._items, index, index);
							Array.Copy(this._items, index + count, this._items, index * 2, this._size - index);
						}
						else
						{
							T[] array = new T[count];
							collection2.CopyTo(array, 0);
							array.CopyTo(this._items, index);
						}
						this._size += count;
					}
				}
				else
				{
					using (IEnumerator<T> enumerator = collection.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							this.Insert(index++, enumerator.Current);
						}
					}
				}
				this._version++;
			}
			finally
			{
				suppressListChanged = prevSuppressListChanged;
			}
			var cnt = _size - prevSize;
			if (!suppressListChanged)
				OnListChangedRange(ObservableListRangeAction.InsertRange, index, cnt);
		}

		public int LastIndexOf(T item)
		{
			if (this._size == 0)
				return -1;
			return this.LastIndexOf(item, this._size - 1, this._size);
		}

		public int LastIndexOf(T item, int index)
		{
			if (index >= this._size)
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");

			return this.LastIndexOf(item, index, index + 1);
		}

		public int LastIndexOf(T item, int index, int count)
		{
			if (this.Count != 0 && index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			if (this.Count != 0 && count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
			if (this._size == 0)
				return -1;
			if (index >= this._size)
				throw new ArgumentOutOfRangeException("index", index, "Larger than collection size.");
			if (count > index + 1)
				throw new ArgumentOutOfRangeException("count", count, "Larger than collection size.");

			return Array.LastIndexOf<T>(this._items, item, index, count);
		}

		public bool Remove(T item)
		{
			int num = this.IndexOf(item);
			if (num >= 0)
			{
				this.RemoveAt(num);
				return true;
			}
			return false;
		}

		void IList.Remove(object item)
		{
			if (ObservableList<T>.IsCompatibleObject(item))
			{
				this.Remove((T)((object)item));
			}
		}

		public int RemoveAll(Predicate<T> match)
		{
			CheckForReadOnly();
			if (match == null)
				throw new ArgumentNullException("match");

			int num = 0;
			while (num < this._size && !match(this._items[num]))
			{
				num++;
			}
			if (num >= this._size)
			{
				return 0;
			}
			int i = num + 1;
			while (i < this._size)
			{
				while (i < this._size && match(this._items[i]))
				{
					i++;
				}
				if (i < this._size)
				{
					this._items[num++] = this._items[i++];
				}
			}

			if (!suppressListChanged)
				for (int n = num; n < _size; n++)
					OnListChanged(ObservableListAction.Remove, n, _items[n]);

			Array.Clear(this._items, num, this._size - num);
			int result = this._size - num;
			this._size = num;
			this._version++;

			return result;
		}

		public int RemoveAll(Func<T, int, bool> match)
		{
			CheckForReadOnly();
			if (match == null)
				throw new ArgumentNullException("match");

			int num = 0;
			while (num < this._size && !match(this._items[num], num))
			{
				num++;
			}
			if (num >= this._size)
			{
				return 0;
			}
			int i = num + 1;
			while (i < this._size)
			{
				while (i < this._size && match(this._items[i], num))
				{
					i++;
				}
				if (i < this._size)
				{
					this._items[num++] = this._items[i++];
				}
			}

			if (!suppressListChanged)
				for (int n = num; n < _size; n++)
					OnListChanged(ObservableListAction.Remove, n, _items[n]);

			Array.Clear(this._items, num, this._size - num);
			int result = this._size - num;
			this._size = num;
			this._version++;

			return result;
		}

		public void RemoveAt(int index)
		{
			CheckForReadOnly();
			if (index < 0 || index >= this._size)
				throw new ArgumentOutOfRangeException("Index and count must refer to a location within the string.");

			T removedValue = _items[index];

			this._size--;
			if (index < this._size)
			{
				Array.Copy(this._items, index + 1, this._items, index, this._size - index);
			}
			this._items[this._size] = default(T);
			this._version++;

			if (!suppressListChanged)
				OnListChanged(ObservableListAction.Remove, index, removedValue);
		}

		public void RemoveRange(int index, int count)
		{
			CheckForReadOnly();
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

			if (count > 0)
			{
				if (!suppressListChanged)
					for (int i = index; i < index + count; i++)
						OnListChanged(ObservableListAction.Remove, i, _items[i]);

				this._size -= count;
				if (index < this._size)
					Array.Copy(this._items, index + count, this._items, index, this._size - index);

				Array.Clear(this._items, this._size, count);
				this._version++;
			}
		}

		public void Reverse()
		{
			this.Reverse(0, this.Count);
		}

		public void Reverse(int index, int count)
		{
			CheckForReadOnly();
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

			Array.Reverse(this._items, index, count);
			this._version++;
			if (!suppressListChanged)
				OnListChanged(ObservableListAction.Revers);
		}

		public void Sort()
		{
			this.Sort(0, this.Count, null);
		}

		public void Sort(IComparer<T> comparer)
		{
			this.Sort(0, this.Count, comparer);
		}

		public void Sort(int index, int count, IComparer<T> comparer)
		{
			CheckForReadOnly();
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");
			if (this._size - index < count)
				throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

			if (count > 0)
			{
				Array.Sort<T>(this._items, index, count, comparer);
				this._version++;
				if (!suppressListChanged)
					OnListChanged(ObservableListAction.Sort);
			}
		}

		private sealed class FunctorComparer : IComparer<T>
		{
			private Comparison<T> comparison;
			public FunctorComparer(Comparison<T> comparison)
			{
				this.comparison = comparison;
			}
			public int Compare(T x, T y)
			{
				return this.comparison(x, y);
			}
		}

		public void Sort(Comparison<T> comparison)
		{
			CheckForReadOnly();
			if (comparison == null)
				throw new ArgumentNullException("match");

			if (this._size > 0)
			{
				IComparer<T> comparer = new FunctorComparer(comparison);
				Array.Sort<T>(this._items, 0, this._size, comparer);
				if (!suppressListChanged)
					OnListChanged(ObservableListAction.Sort);
			}
		}

		public T[] ToArray()
		{
			T[] array = new T[this._size];
			Array.Copy(this._items, 0, array, 0, this._size);
			return array;
		}

		public T[] ToArrayFast()
		{
			if (Count == 0)
				return new T[0];

			Truncate();
			return ArrayInternal;
		}

		public void TrimExcess()
		{
			int num = (int)((double)this._items.Length * 0.9);
			if (this._size < num)
			{
				this.Capacity = this._size;
			}
		}

		public bool TrueForAll(Predicate<T> match)
		{
			if (match == null)
				throw new ArgumentNullException("match");
			for (int i = 0; i < this._size; i++)
			{
				if (!match(this._items[i]))
				{
					return false;
				}
			}
			return true;
		}
		internal static IList<T> Synchronized(ObservableList<T> list)
		{
			return new ObservableList<T>.SynchronizedList(list);
		}

		private bool isReadOnly;
		public bool IsReadOnly
		{
			get { return isReadOnly; }
		}

		public void SwitchToReadOnly()
		{
			isReadOnly = true;
		}

		public void SwitchToWritable()
		{
			isReadOnly = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckForReadOnly()
		{
			if (isReadOnly)
                Contract.Assert(isReadOnly);
			//	throw new NotSupportedException("Attempt to modify the read-only collection");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void OnListChanged(ObservableListAction action, int index = -1, T value = default(T))
		{
			if (suppressListChanged)
				return;

			var handler = ListChanged;
			if (handler != null)
				handler(action, index, value);
		}
		public event ObservableListEvent<T> ListChanged;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void OnListChangedRange(ObservableListRangeAction action, int index, int count)
		{
			if (suppressListChanged)
				return;

			var handler = ListChangedRange;
			if (handler != null)
				handler(action, index, count);
		}
		public event ObservableListRangeEvent<T> ListChangedRange;

		public void Truncate()
		{
			if (Count == 0)
				return;

			if (Capacity > Count)
				Capacity = Count;
		}
	}

	public delegate void ObservableListEvent<T>(ObservableListAction action, int index, T value);
	public delegate void ObservableListRangeEvent<T>(ObservableListRangeAction action, int index, int count);

	public enum ObservableListAction
	{
		Add,
		Remove,
		Replace,
		Clearing,
		Reset,
		Clear,
		Move,
		Sort,
		Revers,
	}

	public enum ObservableListRangeAction
	{
		InsertRange,
	}
}
