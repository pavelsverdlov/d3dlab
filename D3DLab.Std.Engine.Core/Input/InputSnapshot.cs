using System.Collections.Generic;
using D3DLab.Std.Engine.Core.Input;

namespace D3DLab.Std.Engine.Core.Input {
    public class InputSnapshot {
        public List<IInputCommand> Events { get; }
        public void AddEvent(IInputCommand ev) {
            Events.Add(ev);
        }
        public void RemoveEvent(IInputCommand ev) {
            Events.Remove(ev);
        }
        public InputSnapshot() {
            Events = new List<IInputCommand>();
        }
    }    
}
