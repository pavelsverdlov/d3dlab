using D3DLab.Core.Input;
using System.Collections.Generic;

namespace D3DLab.Core.Context {
    public interface IInputManager {
        List<InputEventState> Events { get; }
        void AddEvent(InputEventState inputEventState);
        void RemoveEvent(InputEventState inputEventState);
    }

}
