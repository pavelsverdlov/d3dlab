using D3DLab.Core.Common;

namespace D3DLab.Core.Components {
    public sealed class GeometryComponent : D3DComponent {
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Geometry { get; set; }

        public override string ToString() {
            return $"[Bounds:{Geometry.Bounds};Positions:{Geometry.Positions.Count};Indices:{Geometry.Indices.Count}]";
        }
    }


}
