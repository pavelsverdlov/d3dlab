using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct GeometryFlatShadingComponent : IGraphicComponent {

        public static GeometryFlatShadingComponent Create() => new GeometryFlatShadingComponent { Tag = new ElementTag(Guid.NewGuid().ToString()) };

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {}
    }
    public struct WireframeGeometryComponent : IGraphicComponent {

        public static WireframeGeometryComponent Create() => new WireframeGeometryComponent { Tag = new ElementTag(Guid.NewGuid().ToString()) };

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() { }
    }
}
