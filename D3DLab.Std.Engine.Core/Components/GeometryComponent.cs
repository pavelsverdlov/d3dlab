using D3DLab.Std.Engine.Core.Ext;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components {
    public class GeometryComponent : GraphicComponent, IGeometryComponent {
        public virtual ImmutableArray<Vector3> Positions { get; set; }
        public virtual ImmutableArray<Vector3> Normals { get; set; }
        public virtual ImmutableArray<Vector4> Colors {
            get {
                if (!colors.Any()) {
                    colors = Positions.Length.SelectToList(() => Color).ToImmutableArray();
                }
                return colors;
            }
            set {
                colors = value;
            }
        }
        public ImmutableArray<Vector2> TextureCoordinates { get; set; }
        public ImmutableArray<int> Indices { get; set; }
        public Vector4 Color { get; set; }

        ImmutableArray<Vector4> colors;

        public GeometryComponent() {
            colors = ImmutableArray<Vector4>.Empty;
            TextureCoordinates = ImmutableArray<Vector2>.Empty;
            IsModified = true;
        }

        public void MarkAsRendered() {
            IsModified = false;
        }
    }
}
