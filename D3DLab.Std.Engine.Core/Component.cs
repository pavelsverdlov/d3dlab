using System;

namespace D3DLab.Std.Engine.Core {
    public interface IGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        ElementTag EntityTag { get; set; }
    }

    public abstract class GraphicComponent : IGraphicComponent {
        public ElementTag Tag { get; }
        public ElementTag EntityTag { get; set; }

        protected GraphicComponent() {
            Tag = new ElementTag(Guid.NewGuid().ToString());
        }

        public virtual void Dispose() {

        }
    }
}
