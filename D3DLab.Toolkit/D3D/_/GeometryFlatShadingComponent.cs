using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.D3D {
    public struct GeometryFlatShadingComponent : IGraphicComponent {

        public static GeometryFlatShadingComponent Create() => new GeometryFlatShadingComponent { Tag = new ElementTag(Guid.NewGuid().ToString()) };

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {}
    }
}
