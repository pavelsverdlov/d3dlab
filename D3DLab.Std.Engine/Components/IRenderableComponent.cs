using D3DLab.Std.Engine.Core;

namespace D3DLab.Std.Engine.Components {
    public interface IRenderableComponent : ID3DComponent {
        void Update(RenderState state);
        void Render(RenderState state);
    }
}
