using D3DLab.ECS;
using System;

namespace D3DLab.Toolkit.Components {
    public struct WireframeGeometryComponent : IGraphicComponent {

        public static WireframeGeometryComponent Create() => new WireframeGeometryComponent {
            Tag = new ElementTag(Guid.NewGuid().ToString()),
            IsValid = true,
        };

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; }

        public void Dispose() { }
    }
}
