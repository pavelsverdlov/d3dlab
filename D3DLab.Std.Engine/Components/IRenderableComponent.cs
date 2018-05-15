using D3DLab.Std.Engine.Core;

namespace D3DLab.Std.Engine.Components {
    public interface IRenderableComponent : IGraphicComponent {
        void Update(RenderState state);
        void Render(RenderState state);
    }
}
