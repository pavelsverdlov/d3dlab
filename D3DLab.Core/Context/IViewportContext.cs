using D3DLab.Core.Render;

namespace D3DLab.Core.Context {
    public interface IViewportContext : D3DLab.Std.Engine.Core.IViewportContext {
        Graphics Graphics { get; }
        World World { get; }
    }

}
