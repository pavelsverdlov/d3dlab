using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.Std.Engine.Core {
    public interface ISynchronizationContext {
        void Synchronize();
    }

    public class SynchronizationContext<TOwner, TInput> {
        Queue<Tuple<Action<TOwner, TInput>, TInput>> queue;
        readonly TOwner owner;
        readonly object _loker;

        public SynchronizationContext(TOwner owner) {
            this.queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
            this.owner = owner;
            _loker = new object();
        }

        public void Synchronize() {
            //copy to local
            Queue<Tuple<Action<TOwner, TInput>, TInput>> local;
            lock (_loker) {
                local = queue;
                queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
            }
            while (local.Any()) {
                var item = local.Dequeue();
                item.Item1(owner, item.Item2);
            }
        }
        public void Add(Action<TOwner, TInput> action, TInput input) {
            lock (_loker) {
                queue.Enqueue(Tuple.Create(action, input));
            }
        }

        public void Dispose() {
            queue.Clear();
        }
    }
}
