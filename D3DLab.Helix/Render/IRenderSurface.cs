using System;

namespace D3DLab.Helix.Render {
    public interface IRenderSurface {
        event Action Resize;

        double Width { get; }
        double Height { get; }

        IntPtr Handle { get; }
    }
}
