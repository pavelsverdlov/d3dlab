using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://devblogs.microsoft.com/dotnet/please-welcome-immutablearrayt/
    /// https://www.infoq.com/articles/For-Each-Performance/
    /// 
    /// Use ReadOnlyCollection, because creating a ReadOnlyCollection<T> wrapper is an O(1) operation, 
    /// and does not incur any performance cost for array or list
    /// 
    /// use ImmutableArray because iteration perfomance is almost best :) and allow to expand to new ImmutableArray
    /// </remarks>
    public class ImmutableGeometryData : IGeometryData {
        public ImmutableArray<Vector3> Positions { get; private set; }
        public ImmutableArray<int> Indices { get; private set; }
        public ImmutableArray<Vector3> Normals { get; private set; }
        public ImmutableArray<Vector2> TexCoor { get; private set; }
        public ImmutableArray<Vector4> Colors { get; private set; }
        public bool IsModified { get; set; }
        public bool IsDisposed { get; private set; }

        public ImmutableGeometryData(ReadOnlyCollection<Vector3> positions, ReadOnlyCollection<int> indices) 
            : this(positions, null, indices, null) {
        }
        public ImmutableGeometryData(ReadOnlyCollection<Vector3> positions, ReadOnlyCollection<int> indices,
            ReadOnlyCollection<Vector4> colors) : this(positions, null, indices, null, colors) {
        }
        public ImmutableGeometryData(ReadOnlyCollection<Vector3> positions,
           ReadOnlyCollection<Vector3> normals, ReadOnlyCollection<int> indices) 
            : this(positions, normals, indices,null) {
        }
        public ImmutableGeometryData(ReadOnlyCollection<Vector3> positions,
            ReadOnlyCollection<Vector3> normals, ReadOnlyCollection<int> indices,
            ReadOnlyCollection<Vector2> texCoor) 
            :this(positions, normals, indices, texCoor, null){
        }
        ImmutableGeometryData(ReadOnlyCollection<Vector3> positions,
            ReadOnlyCollection<Vector3> normals, ReadOnlyCollection<int> indices,
            ReadOnlyCollection<Vector2> texCoor, ReadOnlyCollection<Vector4> colors) {

            Positions = positions == null ? ImmutableArray<Vector3>.Empty : positions.ToImmutableArray();
            Normals = normals == null ? ImmutableArray<Vector3>.Empty : positions.ToImmutableArray();
            Indices = indices == null ? ImmutableArray<int>.Empty : indices.ToImmutableArray();
            TexCoor = texCoor == null ? ImmutableArray<Vector2>.Empty : texCoor.ToImmutableArray();
            Colors = colors == null ? ImmutableArray<Vector4>.Empty : colors.ToImmutableArray();
            IsModified = true;
        }

        public virtual void Dispose() {
            IsDisposed = true;
            Positions = ImmutableArray<Vector3>.Empty;
            Normals = ImmutableArray<Vector3>.Empty;
            Indices = ImmutableArray<int>.Empty;
            TexCoor = ImmutableArray<Vector2>.Empty;
            Colors = ImmutableArray<Vector4>.Empty;
        }
    }
}
