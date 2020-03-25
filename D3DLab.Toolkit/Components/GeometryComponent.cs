using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Components {
    class LineGeometryComponent : GraphicComponent {
        public readonly Vector3[] Positions;
        public readonly int[] Indices;
        public LineGeometryComponent(Vector3[] positions, int[] indices) {
            Positions = positions;
            Indices = indices;
            IsModified = true;
        }
    }
    class GeometryComponent : GraphicComponent {
        public readonly Vector3[] Positions;
        public readonly Vector3[] Normals;
        public readonly int[] Indices;
        public readonly Vector2[] TexCoor;

        public GeometryComponent(Vector3[] positions, Vector3[] normals, int[] indices) {
            Positions = positions;
            Normals = normals;
            Indices = indices;
            TexCoor = new Vector2[positions.Length];
            IsModified = true;
        }
        public GeometryComponent(Vector3[] positions, Vector3[] normals, int[] indices, Vector2[] texCoor) {
            Positions = positions;
            Normals = normals;
            Indices = indices;
            TexCoor = texCoor;

            IsModified = true;
        }
    }
}
