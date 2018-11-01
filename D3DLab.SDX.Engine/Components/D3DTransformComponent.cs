using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using System;
using System.Numerics;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTransformComponent : TransformComponent {
        public SharpDX.Direct3D11.Buffer TransformBuffer { get; set; }

        public D3DTransformComponent() {
           

        }



        public override void Dispose() {
            TransformBuffer?.Dispose();
            base.Dispose();
        }
    }
}
