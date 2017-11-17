using System;

namespace D3DLab.Core.Visual3D {
    public static class DisposeEx {
        public static void CreateInstance<T>(ref T source, T newobj) where T : IDisposable{
            if (source != null) {
                source.Dispose();
            }
            source = newobj;
        }
    }
}