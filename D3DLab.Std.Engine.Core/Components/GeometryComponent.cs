using D3DLab.Std.Engine.Core.Ext;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components {
    public class ColorComponent : GraphicComponent {
        public Vector4 Color { get; set; }
    }

    public class GeometryComponent : GraphicComponent, IGeometryComponent {
        public virtual List<Vector3> Positions { get; set; }
        public virtual List<Vector3> Normals { get; set; }
        public virtual List<Vector4> Colors {
            get {
                if (!colors.Any()) {
                    return Positions.Count.SelectToList(() => Color);
                }
                return colors;
            }
            set {
                colors = value;
            }
        }
        public List<Vector2> TextureCoordinates { get; set; }
        public List<int> Indices { get; set; }
        public Vector4 Color { get; set; }

        List<Vector4> colors;

        public GeometryComponent() {
            colors = new List<Vector4>();
            IsModified = true;
        }

        public void MarkAsRendered() {
            IsModified = false;
        }
    }
}
