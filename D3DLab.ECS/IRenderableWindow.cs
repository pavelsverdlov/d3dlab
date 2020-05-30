using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace D3DLab.ECS {
    public interface IRenderableSurface {
        float Width { get; }
        float Height { get; }

        event Action Resized;
        event Action Invalidated;
    }
    public interface IRenderableWindow : IRenderableSurface {
        bool IsActive { get; }
        IntPtr Handle { get; }
        [Obsolete("Remove from this interface")]
        IInputManager InputManager { get; }

        WaitHandle BeginInvoke(Action action);
    }
}
