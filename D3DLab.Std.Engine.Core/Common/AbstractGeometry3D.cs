using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using g3;

namespace D3DLab.Std.Engine.Core.Common {
    public abstract class ReadOnlyList<T> where T : struct {
        public T this[int index] {
            get { return array[index]; }
            set { array[index] = value; }
        }
        public int Count => array.Length;

        readonly T[] array;
        protected ReadOnlyList(T[] vectors) {
            this.array = vectors;
        }
        protected ReadOnlyList(int length) {
            this.array = new T[length];
        }

        public IEnumerator<T> GetEnumerator() {
            return array.ToList().GetEnumerator();
        }

        public static implicit operator List<T>(ReadOnlyList<T> d) {
            return d.array.ToList();
        }
    }
    public class Vector4Collection : ReadOnlyList<Vector4> {
        public Vector4Collection(List<Vector4> vectors) : base(vectors.ToArray()) {
        }
    }
    public class Vector3Collection : ReadOnlyList<Vector3> {
        public Vector3Collection(List<Vector3> vectors) : base(vectors.ToArray()) { }
        public Vector3Collection(int length) : base(length) { }
    }
    public class Vector2Collection : ReadOnlyList<Vector2> {
        public Vector2Collection(List<Vector2> vectors) : base(vectors.ToArray()) {
        }
    }
    public class IntCollection : ReadOnlyList<int> {
        public IntCollection(List<int> vectors) : base(vectors.ToArray()) {
        }
    }

    public class AbstractGeometry3D {
        public List<Vector3> Positions { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector3> Tangents { get; set; }
        public List<Vector3> BiTangents { get; set; }
        public List<Vector4> Colors { get; set; }
        public List<Vector2> TextureCoordinates { get; set; }
        public List<int> Indices { get; set; }

        public Vector4 Color { get; set; }

       

        public AbstractGeometry3D() {
            Colors = new List<Vector4>();
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            TextureCoordinates = new List<Vector2>();
            Indices = new List<int>();



            
        }

        

    }
}
