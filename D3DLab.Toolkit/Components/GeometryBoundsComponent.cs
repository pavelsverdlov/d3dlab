using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    readonly struct GeometryBoundsComponent : IGraphicComponent {
        public static GeometryBoundsComponent Create(AxisAlignedBox bounds) {
            return new GeometryBoundsComponent(bounds);
        }


        public AxisAlignedBox Bounds { get; }

        public GeometryBoundsComponent(AxisAlignedBox bounds) : this() {
            Tag = ElementTag.New();
            Bounds = bounds;
            IsValid = true;
        }

        public ElementTag Tag { get; }
        public bool IsModified { get;  }
        public bool IsValid { get;  }
        public bool IsDisposed { get; }

        public void Dispose() {
        }
    }
}
