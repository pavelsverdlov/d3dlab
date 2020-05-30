using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct CollidedWithEntityByRayComponent : IGraphicComponent {
        public static CollidedWithEntityByRayComponent Create(ElementTag with, Vector3 intersectionPositionWorld)
           => new CollidedWithEntityByRayComponent(with, intersectionPositionWorld);

        public ElementTag With { get; }
        public Vector3 IntersectionPositionWorld { get; }

        CollidedWithEntityByRayComponent(ElementTag with, Vector3 intersectionPositionWorld) : this() {
            Tag = ElementTag.New("CollidedWith");
            With = with;
            IsValid = true;
            IntersectionPositionWorld = intersectionPositionWorld;
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
