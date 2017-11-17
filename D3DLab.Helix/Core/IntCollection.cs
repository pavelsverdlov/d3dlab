namespace HelixToolkit.Wpf.SharpDX.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    using HelixToolkit.Wpf.SharpDX.Utilities;

    [Serializable]
    [TypeConverter(typeof(IntCollectionConverter))]
    public sealed class IntCollection : ObservableList<int>
    {
        public IntCollection()
        {    
        }

        public IntCollection(int capacity)
            : base(capacity)
        {            
        }

        public IntCollection(IEnumerable<int> items)
            : base(items)
        {
        }

        public static IntCollection Parse(string source)
        {
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            var th = new TokenizerHelper(source, formatProvider);
            var resource = new IntCollection();
            while (th.NextToken())
            {
                var value = Convert.ToInt32(th.GetCurrentToken(), formatProvider);
                resource.Add(value);
            }

            return resource;
        }

        public string ConvertToString(string format, IFormatProvider provider)
        {
            if (this.Count == 0)
            {
                return String.Empty;
            }

            var str = new StringBuilder();
            for (int i = 0; i < this.Count; i++)
            {
                str.AppendFormat(provider, "{0:" + format + "}", this[i]);
                if (i != this.Count - 1)
                {
                    str.Append(" ");
                }
            }

            return str.ToString();
        }

		public IntCollection Copy()
		{
			return new IntCollection(this);
		}

		public static IntCollection FromArraySafe(int[] list)
		{
			if (list == null)
				return null;
			return FromArray<IntCollection>(list);
		}

		new public static IntCollection FromArray(int[] list)
		{
			if (list == null || list.Length == 0)
				throw new ArgumentException("list is null or empty.", "list");
			return FromArray<IntCollection>(list);
		}
    }
}
