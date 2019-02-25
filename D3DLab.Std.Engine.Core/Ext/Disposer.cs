using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Ext {
    public static class Disposer {
        public static void DisposeAll(params IDisposable[] source) {
            foreach (var d in source) {
                d?.Dispose();
            }
        }
    }
}
