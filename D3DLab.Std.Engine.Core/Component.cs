using D3DLab.Std.Engine.Core.Shaders;
using System;

namespace D3DLab.Std.Engine.Core {
    public interface IGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        ElementTag EntityTag { get; set; }
        bool IsModified { get; }
        bool IsValid { get; }
    }

    public abstract class GraphicComponent : IGraphicComponent {
        public bool IsModified { get; set; }
        public virtual bool IsValid => true;
        public ElementTag Tag { get; }
        public ElementTag EntityTag { get; set; }

        protected GraphicComponent() {
            Tag = new ElementTag(Guid.NewGuid().ToString());
        }

        public virtual void Dispose() {

        }
    }
}
