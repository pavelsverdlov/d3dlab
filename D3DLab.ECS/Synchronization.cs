using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace D3DLab.ECS {
    public interface ISynchronizationContext {
        void Synchronize(int theadId);
    }

    public interface ISynchronizationContext<TOwner, TInput> {
        bool IsChanged { get; }
        void Synchronize(int theadId);
        void BeginSynchronize();
        void EndSynchronize(int theadId);
        void Add(Action<TOwner, TInput> action, TInput input);
        void AddRange(Action<TOwner, TInput> action, IEnumerable<TInput> inputs);
        void Dispose();
    }

    public static class SynchronizationContextBuilder {
        public static ISynchronizationContext<TOwner, TInput> Create<TOwner, TInput>(TOwner owner, bool isEnable) {
            return new SynchronizationContext<TOwner, TInput>(owner);
        }
    }

    public class SynchronizationContext<TOwner, TInput> : ISynchronizationContext<TOwner, TInput> {
        Queue<Tuple<Action<TOwner, TInput>, TInput>> queue;
        Queue<Tuple<Action<TOwner, TInput>, TInput>> queueSnapshot;
        readonly TOwner owner;
        readonly object _loker;
        int theadId;

        public bool IsChanged { get; private set; }

        public SynchronizationContext(TOwner owner) :this(owner, new object()){
            theadId = -1;
        }
        SynchronizationContext(TOwner owner, object _loker) {
            this.queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
            this.owner = owner;
            this._loker = _loker;
        }

        public void BeginSynchronize() {
            Monitor.Enter(_loker);
            queueSnapshot = new Queue<Tuple<Action<TOwner, TInput>, TInput>>(queue);
            queue = new Queue<Tuple<Action<TOwner, TInput>, TInput>>();
            IsChanged = false;
        }

        public void EndSynchronize(int theadId) {
            this.theadId = theadId;

            var local = queueSnapshot;
            queueSnapshot = null;
            Monitor.Exit(_loker);

            while (local.Any()) {
                var item = local.Dequeue();
                try {
                    item.Item1(owner, item.Item2);
                }catch(Exception ex) {
                    System.Diagnostics.Trace.WriteLine($"retry, move action to next render iteration [{ex.Message}]");
                    Add(item.Item1, item.Item2);
                }
            }
        }

        public void Synchronize(int theadId) {
            BeginSynchronize();
            EndSynchronize(theadId);
        }

        public void Add(Action<TOwner, TInput> action, TInput input) {
            //if(Thread.CurrentThread.ManagedThreadId == theadId) {
            //    action(owner, input);
            //    return;
            //}
            lock (_loker) {
                IsChanged = true;
                queue.Enqueue(Tuple.Create(action, input));
            }
        }
        public void AddRange(Action<TOwner, TInput> action, IEnumerable<TInput> inputs) {
            //if (Thread.CurrentThread.ManagedThreadId == theadId) {
            //    foreach (var input in inputs) {
            //        action(owner, input);
            //    }
            //    return;
            //}
            lock (_loker) {
                IsChanged = true;
                foreach (var input in inputs) {
                    queue.Enqueue(Tuple.Create(action, input));
                }
            }
        }

        public void Dispose() {
            queue.Clear();
            queueSnapshot?.Clear();
        }
    }
}
