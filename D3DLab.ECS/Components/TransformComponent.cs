using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct TransformComponent : IGraphicComponent {
       

        public static TransformComponent Create(Matrix4x4 matrixWorld) {
            return new TransformComponent(matrixWorld);
        }

        public static TransformComponent Identity() {
            return new TransformComponent(Matrix4x4.Identity);
        }

        public void Dispose() {
        }

        public Matrix4x4 MatrixWorld { get; }
        public ElementTag Tag { get;  }
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get;  }

        TransformComponent(Matrix4x4 matrixWorld) : this() {
            MatrixWorld = matrixWorld;
            Tag = ElementTag.New();
        }
    }
}
