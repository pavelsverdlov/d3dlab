using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;

namespace D3DLab.SDX.Engine {
    internal interface ID3DRenderableComponent : IGraphicComponent {
        void Update(RenderState state);
        void Render(RenderState state);

        //bool IsRendered { get; set; }
        IRenderTechniquePass Pass { get; }
        void Accept(RenderFrameStrategiesVisitor visitor);
    }
}
