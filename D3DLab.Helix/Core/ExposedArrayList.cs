using System.Collections.Generic;
using System.Diagnostics;
using HelixToolkit.Wpf.SharpDX.Extensions;

namespace HelixToolkit.Wpf.SharpDX.Core
{
	public class ExposedArrayList<T> : ObservableList<T>
	{
		public ExposedArrayList()
		{
		}

		public ExposedArrayList(int capacity)
			: base(capacity)
		{
		}

		public ExposedArrayList(IEnumerable<T> collection)
			: base(collection)
		{
		}

		internal T[] Array
		{
			get
			{
				if (ArrayInternal.Length != this.Count)
				{
#if DEBUG
					int dif = ArrayInternal.Length - this.Count;
					int len = ArrayInternal.Length;
					int cnt = this.Count;
					System.Threading.ThreadPool.QueueUserWorkItem(i => Debug.WriteLine(string.Format("!!! Tranc array. Dif={0} (L:{1} / C:{2})", dif, len, cnt)));
#endif
					this.Capacity = this.Count;
				}
				return ArrayInternal;
			}
		}
	}
}
