using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Ext {
    public class DisposableSetter<T> : IDisposable where T : IDisposable {
        T disposable;
        public void Dispose() {
            disposable?.Dispose();
        }
        public T Get() => disposable;
        public void Set(T b) {
            disposable?.Dispose();
            disposable = b;
        }
    }

    public class EnumerableDisposableSetter<T> : IDisposable where T : IEnumerable<IDisposable> {
        T disposable;
        public void Dispose() {
            Disposer.DisposeAll(disposable);
        }
        public T Get() => disposable;
        public void Set(T b) {
            Disposer.DisposeAll(disposable);
            disposable = b;
        }
    }
}
