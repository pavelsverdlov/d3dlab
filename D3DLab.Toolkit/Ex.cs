using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace D3DLab.Toolkit {
    internal static class EnumerableEx {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }
    }
    internal static class ArrayEx {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] source) => Array.AsReadOnly(source);
    }
}
