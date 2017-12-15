using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace D3DLab.Core.Components {
    public sealed class LineGeometryComponent : GeometryComponent {
        public float Thickness { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
        public double HeadLength { get; set; }
        public double Diameter { get; set; }
        public int ThetaDiv { get; set; }
        public float HeadWidth { get; set; }

        public LineGeometryComponent() {
          
        }

        public void RefreshGeometry() {
            var builder = new MeshBuilder(true, true);


            HeadLength = 3 * Diameter/6;
            ThetaDiv = 36;
            HeadWidth = (float)(2f * Diameter /7);

            builder.AddArrow(this.Start, this.End, this.Diameter, this.HeadLength, this.ThetaDiv, this.HeadWidth);
            this.Geometry = builder.ToMeshGeometry3D();
        }

    }    
}
