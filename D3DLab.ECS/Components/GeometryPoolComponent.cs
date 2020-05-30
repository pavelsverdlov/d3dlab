using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public struct GeometryPoolComponent : IGraphicComponent {
        public static GeometryPoolComponent Create(Guid index) {
            return new GeometryPoolComponent(index);
        }
        public readonly Guid Key;
        public GeometryPoolComponent(Guid index) : this() {
            Key = index;
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
