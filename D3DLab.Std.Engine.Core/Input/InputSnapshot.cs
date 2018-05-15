using System;
using System.Collections.Generic;
using System.Linq;
using D3DLab.Std.Engine.Core.Input;

namespace D3DLab.Std.Engine.Core.Input {
    public class InputSnapshot {
        private readonly object _loker;
        private Dictionary<Type, IInputCommand> cache;
        public InputSnapshot() {
            _loker = new object();
            cache = new Dictionary<Type, IInputCommand>();
        }

        public List<IInputCommand> Events {
            get {
                IInputCommand[] values = null;
                lock (_loker) {
                    values = cache.Values.ToArray();
                }
                var res = new List<IInputCommand>();
                foreach (var cmd in values) {
                    res.Add(cmd);
                }
                return res;
            }
        }
        public void AddEvent<TCommand>(TCommand ev) where TCommand : IInputCommand {
            //Events.Add(ev);
            //lock (_loker) {
            var type = ev.GetType();
            if (cache.ContainsKey(type)) {
                cache[type] = ev;
            } else {
                cache.Add(type, ev);
            }
            //  }
        }
        public void RemoveEvent<TCommand>(TCommand ev) where TCommand : IInputCommand {
            lock (_loker) {
                var type = ev.GetType();
                if (cache.ContainsKey(type)) {
                    cache.Remove(type);
                }
            }
            // Events.Remove(ev);
        }

        public InputSnapshot CloneAndClear() {
            var temp = cache;
            lock (_loker) {
                cache = new Dictionary<Type, IInputCommand>();
            }
            var cloned = new InputSnapshot();
            foreach (var cmd in temp) {
                cloned.cache.Add(cmd.Key, cmd.Value);
            }
            //Console.WriteLine($"CloneAndClear {cloned.cache.Count}");
            return cloned;
        }

        internal void Dispose() {
            lock (_loker) {
                cache.Clear();
            }
        }
    }
}
