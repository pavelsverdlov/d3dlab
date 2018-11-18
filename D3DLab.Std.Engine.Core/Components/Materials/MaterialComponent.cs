using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components.Materials {
    public class ColorComponent : GraphicComponent {
        public Vector4 Color { get; set; }
    }

    public class MaterialComponent : GraphicComponent {
        public float Specular { get; set; } = -1; // -1 not specular

    }
}
