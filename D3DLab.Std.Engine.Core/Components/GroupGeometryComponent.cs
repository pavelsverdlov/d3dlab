using D3DLab.Std.Engine.Core.Common;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components {
    public interface IGeometryComponent : IGraphicComponent {
        void MarkAsRendered();

        List<Vector3> Positions { get; }
        List<Vector3> Normals { get; }
        List<int> Indices { get; }
        List<Vector4> Colors { get; }
    }

    public class GroupGeometryComponent : GraphicComponent, ICollection<GeometryComponent>, IGeometryComponent {

        public List<Vector3> Positions => combined.Positions;
        public List<int> Indices => combined.Indices;
        public List<Vector4> Colors => combined.Colors;
        public List<Vector3> Normals => combined.Normals;

        readonly List<GeometryComponent> groups;

        public int Count => groups.Count;

        public bool IsReadOnly => false;
        
        AbstractGeometry3D combined;

        public GroupGeometryComponent() {
            IsModified = true;
            groups = new List<GeometryComponent>();
        }

        public void Combine() {
            var value = new Utilities.Helix.MeshBuilder(true, false);
            groups.ForEach(x => value.Append(x));
            combined = value.ToGeometry3D();
        }

        public void Add(GeometryComponent item) {
            groups.Add(item);
        }

        public void Clear() {
            throw new System.NotImplementedException();
        }

        public bool Contains(GeometryComponent item) {
            throw new System.NotImplementedException();
        }

        public void CopyTo(GeometryComponent[] array, int arrayIndex) {
            throw new System.NotImplementedException();
        }

        public bool Remove(GeometryComponent item) {
            throw new System.NotImplementedException();
        }

        public IEnumerator<GeometryComponent> GetEnumerator() {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new System.NotImplementedException();
        }

        public void MarkAsRendered() {
            IsModified = false;
        }
    }
}
