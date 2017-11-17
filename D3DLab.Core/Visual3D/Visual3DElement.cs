using System.Collections.Generic;
using System.Linq;
using D3DLab.Core.Render;
using SharpDX;

namespace D3DLab.Core.Visual3D {
    public sealed class GeometryModel  {
        //Vector4
        //W = 0 means a vector,
        //W != 0 means a point.
        //public Vector3[] 
        public int[] Indices { get; private set; }
        public PositionArray Vertices { get; private set; }
        public Color4Array Colors { get; private set; }
        private readonly VertexPositionColor[] renderBuffer;
        private readonly Dictionary<int, List<int>> mapper; 
        public GeometryModel(Vector3[] vertices, int[] indices, Color4[] colors) {
            Indices = indices;
            mapper = new Dictionary<int, List<int>>();
            var length = indices.Length;
            renderBuffer = new VertexPositionColor[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                renderBuffer[i] = new VertexPositionColor(vertices[i], colors[i]);
            }

//            for (int i = 0; i < indices.Length; i++) {
//                var vindex = indices[i];
//                renderBuffer[i] = new VertexPositionColor(vertices[vindex], colors[i]);
//                List<int> data;
//                if (!mapper.TryGetValue(vindex, out data)) {
//                    data = new List<int>();
//                    mapper.Add(vindex, data);
//                }
//                data.Add(i);
//            }

            Vertices = new PositionArray(renderBuffer, mapper);
            Colors = new Color4Array(renderBuffer, mapper);
        }

        public VertexPositionColor[] GetRenderData() {
            return renderBuffer;
        }
    }
    public sealed class Color4Array {
        public Color4 this[int index] {
            get { return rederBuffer[index].Color; }
            set {
                foreach (var vindex in mapper[index]) {
                    rederBuffer[vindex] = new VertexPositionColor(rederBuffer[vindex].Position, value);
                }
            }
        }

        private readonly VertexPositionColor[] rederBuffer;
        private readonly Dictionary<int, List<int>> mapper;
        public Color4Array(VertexPositionColor[] rederBuffer, Dictionary<int, List<int>> mapper) {
            this.rederBuffer = rederBuffer;
            this.mapper = mapper;
        }
    }
    public sealed class PositionArray {
        public Vector3 this[int index] {
            get { return rederBuffer[index].Position; }
            set {
                foreach (var vindex in mapper[index]) {
                    rederBuffer[vindex] = new VertexPositionColor(value, rederBuffer[vindex].Color);
                }
            }
        }
        private readonly VertexPositionColor[] rederBuffer;
        private readonly Dictionary<int, List<int>> mapper;
        public PositionArray(VertexPositionColor[] rederBuffer, Dictionary<int, List<int>> mapper) {
            this.rederBuffer = rederBuffer;
            this.mapper = mapper;
        }
    }
    public class Visual3DElement : IRenderable {
        public GeometryModel Geometry { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }

        public void Render(IRenderer renderer) {
            renderer.Draw(this);
        }

        public VertexPositionColor[] GetRenderData() {
            return Geometry.GetRenderData();
        }
        public ushort[] GetIndices() {
            return Geometry.Indices.Select(x => (ushort) x).ToArray();
        }
    }
}