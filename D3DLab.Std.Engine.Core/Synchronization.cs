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

        public bool IsChanged { get; private set; }

        public SynchronizationContext(TOwner owner) :this(owner, new object()){
        }
        public SynchronizationContext(TOwner owner, object _loker) {
            this.queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
            this.owner = owner;
            this._loker = _loker;
        }

        public void Synchronize() {
            //copy to local
            Queue<Tuple<Action<TOwner, TInput>, TInput>> local;
            lock (_loker) { 
                local = new Queue<Tuple<Action<TOwner, TInput>, TInput>>(queue);
                queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
                IsChanged = false;
            }
            while (local.Any()) {
                var item = local.Dequeue();
                try {
                    item.Item1(owner, item.Item2);
                }catch(Exception ex) {
                    System.Diagnostics.Trace.WriteLine("retry, move action to next render iteration");
                    Add(item.Item1, item.Item2);
                }
            }
        }
        public void Add(Action<TOwner, TInput> action, TInput input) {
            lock (_loker) {
                IsChanged = true;
                queue.Enqueue(Tuple.Create(action, input));
            }
        }
        public void AddRange(Action<TOwner, TInput> action, IEnumerable<TInput> inputs) {
            lock (_loker) {
                IsChanged = true;
                foreach (var input in inputs) {
                    queue.Enqueue(Tuple.Create(action, input));
                }
            }
        }

        public void Dispose() {
            queue.Clear();
        }
    }
}
