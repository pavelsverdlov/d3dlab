using D3DLab.Std.Engine.Core.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core {
    public interface IAppWindow {
        float Width { get; }
        float Height { get; }
        bool IsActive { get; }

        IntPtr Handle { get; }
        IInputManager InputManager { get; }
    }
}
