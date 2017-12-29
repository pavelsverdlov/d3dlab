using D3DLab.Core.Render;

namespace D3DLab.Core.Context {
    public interface IViewportContext {
        Graphics Graphics { get; }
        World World { get; }
    }

}
