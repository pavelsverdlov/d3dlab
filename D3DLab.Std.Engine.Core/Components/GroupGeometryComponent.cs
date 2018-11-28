using D3DLab.Std.Engine.Core.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components {
    public interface IGeometryComponent : IGraphicComponent {
        void MarkAsRendered();

        ImmutableArray<Vector3> Positions { get; }
        ImmutableArray<Vector3> Normals { get; }
        ImmutableArray<int> Indices { get; }
        ImmutableArray<Vector4> Colors { get; }

        ImmutableArray<Vector2> TextureCoordinates { get; }
    }

    public class GroupGeometryComponent : GraphicComponent, ICollection<GeometryComponent>, IGeometryComponent {

        public ImmutableArray<Vector3> Positions {get;private set;}
        public ImmutableArray<int>     Indices       {get;private set;}
        public ImmutableArray<Vector4> Colors    {get;private set;}
        public ImmutableArray<Vector3> Normals   { get; private set; }
        public ImmutableArray<Vector2> TextureCoordinates { get; private set; }

        readonly List<GeometryComponent> groups;

        public int Count => groups.Count;

        public bool IsReadOnly => false;
        
        public GroupGeometryComponent() {
            IsModified = true;
            groups = new List<GeometryComponent>();
        }

        public void Combine() {
            var value = new Utilities.Helix.MeshBuilder(true, false);
            groups.ForEach(x => value.Append(x));
            var combined = value.ToGeometry3D();
            Positions= combined.Positions.ToImmutableArray();
            Indices  = combined.Indices.ToImmutableArray();
            Colors   = combined.Colors.ToImmutableArray();
            Normals  = combined.Normals.ToImmutableArray();
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
