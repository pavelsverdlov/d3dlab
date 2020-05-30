using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    struct GeometryBoundsComponent : IGraphicComponent {
        public static GeometryBoundsComponent Create(AxisAlignedBox bounds) {
            return new GeometryBoundsComponent(bounds);
        }


        public AxisAlignedBox Bounds { get; }

        public GeometryBoundsComponent(AxisAlignedBox bounds) : this() {
            Bounds = bounds;
            Tag = ElementTag.New();
            IsValid = true;
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
        }
    }
}
