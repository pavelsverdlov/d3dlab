using D3DLab.SDX.Engine.Rendering;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;

namespace D3DLab.SDX.Engine {
    internal interface ID3DRenderableComponent : IRenderableComponent {
        void Accept(RenderFrameStrategiesVisitor visitor);
    }
}
