using D3DLab.Std.Engine.Core.Ext;
using System;
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
        /// <summary>
        /// MOve to colot component
        /// </summary>
        public Vector4 Color { get; set; }

        public Veldrid.Utilities.BoundingBox Box => box.Value;

        ImmutableArray<Vector4> colors;
        readonly Lazy<Veldrid.Utilities.BoundingBox> box;

        public GeometryComponent() {
            colors = ImmutableArray<Vector4>.Empty;
            TextureCoordinates = ImmutableArray<Vector2>.Empty;
            IsModified = true;
            //
            box = new Lazy<Veldrid.Utilities.BoundingBox>(CalcuateBox);
        }

        Veldrid.Utilities.BoundingBox CalcuateBox() {
            return Veldrid.Utilities.BoundingBox.CreateFromVertices(Positions.ToArray());
        }

        public void MarkAsRendered() {
            IsModified = false;
        }
    }
}
