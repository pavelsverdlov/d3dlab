using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTransformableComponent : GraphicComponent {
        public Matrix4x4 TramsformWorld { get; set; }
    }
}
