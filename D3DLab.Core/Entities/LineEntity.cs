using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace D3DLab.Core.Entities {
    public struct LineData {
        public Color Color { get; set; }
        public float Thickness { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public RenderTechnique RenderTechnique { get; set; }
        public Matrix Transform { get; set; }

    }
    public sealed class LineEntity : Entity<LineData> {
        public LineEntity() : base("LineEntity") {
        }
    }
}
