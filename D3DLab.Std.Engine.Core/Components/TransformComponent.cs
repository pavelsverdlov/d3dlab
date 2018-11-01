using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components {
    public class TransformComponent : GraphicComponent {
        public Matrix4x4 MatrixWorld { get; set; }
        public TransformComponent() {
            MatrixWorld = Matrix4x4.Identity;
        }
    }
}
