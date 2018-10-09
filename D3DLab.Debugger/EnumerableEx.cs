using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Debugger {
    public static class EnumerableEx {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }
    }
}
