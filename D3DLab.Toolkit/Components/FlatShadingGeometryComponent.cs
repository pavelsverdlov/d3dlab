using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct FlatShadingGeometryComponent : IGraphicComponent {

        public static FlatShadingGeometryComponent Create() => new FlatShadingGeometryComponent {
            Tag = new ElementTag(Guid.NewGuid().ToString()),
            IsValid = true
        };

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; }

        public void Dispose() {}
    }
}
