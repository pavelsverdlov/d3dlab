using D3DLab.ECS;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats._OBJ {
    class GeometryData : IFileGeometry3D {
        public GeometryData(
            string group,
            List<Vector3> positions,
            List<Vector3> normals,
            List<Vector3> colors,
            List<int> indices,
            List<Vector2> textureCoors,
            GeometryPrimitiveTopologies topology) {
            Positions = positions.AsReadOnly();
            Normals = normals.AsReadOnly();
            Colors = colors.AsReadOnly();
            Indices = indices.AsReadOnly();
            TextureCoors = textureCoors.AsReadOnly();
            GroupName = group;
            Topology = topology;
        }

        public string GroupName { get; }
        public GeometryPrimitiveTopologies Topology { get; }
        public ReadOnlyCollection<Vector3> Positions { get; }
        public ReadOnlyCollection<Vector3> Normals { get; }
        public ReadOnlyCollection<Vector3> Colors { get; }
        public ReadOnlyCollection<int> Indices { get; }
        public ReadOnlyCollection<Vector2> TextureCoors { get; }
    }

    public class BaseGroupsBulder {
        protected readonly OBJGeometryCache cache;
        protected List<int> Indices;
        protected List<Vector3> Positions;
        protected Vector3[] Normals;
        protected Vector3[] Colors;
        protected Vector2[] TextureCoors;

        protected BaseGroupsBulder(OBJGeometryCache _base) {
            this.cache = _base;
        }
        protected void Add(in OBJVertex v0, in OBJVertex v1, in OBJVertex v2, int ind0, int ind1, int ind2) {
            Indices.Add(ind0);
            Indices.Add(ind1);
            Indices.Add(ind2);

            switch (v0.Type) {
                case OBJVertexType.Triple: //example: f 1/1/1 2/2/2 3/3/3  
                    if (v0.VN >= 0 && v1.VN >= 0 && v2.VN >= 0) {
                        Normals[ind0] = cache.NormalsCache[v0.VN];
                        Normals[ind1] = cache.NormalsCache[v1.VN];
                        Normals[ind2] = cache.NormalsCache[v2.VN];
                    }
                    if (v0.VT >= 0 && v1.VT >= 0 && v2.VT >= 0) {
                        TextureCoors[ind0] = cache.TextureCoorsCache[v0.VT];
                        TextureCoors[ind1] = cache.TextureCoorsCache[v1.VT];
                        TextureCoors[ind2] = cache.TextureCoorsCache[v2.VT];
                    }
                    //unexpect color in triple face format
                    break;
                case OBJVertexType.Single: //example: f 1 2 3 
                    //TODO: checkin by Count > 0 not handle all cases, to test more OBJ
                    if (cache.NormalsCache.Count > 0) {
                        Normals[ind0] = cache.NormalsCache[v0.V];
                        Normals[ind1] = cache.NormalsCache[v1.V];
                        Normals[ind2] = cache.NormalsCache[v2.V];
                    }
                    if (cache.TextureCoorsCache.Count > 0) {
                        TextureCoors[ind0] = cache.TextureCoorsCache[v0.V];
                        TextureCoors[ind1] = cache.TextureCoorsCache[v1.V];
                        TextureCoors[ind2] = cache.TextureCoorsCache[v2.V];
                    }
                    if (cache.ColorsCache.Count > 0) {
                        Colors[ind0] = cache.ColorsCache[v0.V];
                        Colors[ind1] = cache.ColorsCache[v1.V];
                        Colors[ind2] = cache.ColorsCache[v2.V];
                    }
                    break;
            }
        }
    }
    public class UnitedGroupsBulder : BaseGroupsBulder {
        public UnitedGroupsBulder(OBJGeometryCache _base) : base(_base) {
            Positions = _base.PositionsCache;
            Indices = new List<int>(_base.Vertices.Count);
            Normals = new Vector3[Positions.Count];
            Colors = new Vector3[Positions.Count];
            TextureCoors = new Vector2[Positions.Count];
        }

        public IFileGeometry3D Build() => Build(GeometryPrimitiveTopologies.TriangleList);
        public IFileGeometry3D Build(GeometryPrimitiveTopologies newTopology) {
            //TODO: support conveting to other topology

            for (var index = 0; index < cache.Vertices.Count; index += 3) {
                Add(cache.Vertices[index], cache.Vertices[index + 1], cache.Vertices[index + 2]);
            }

            return new GeometryData(string.Join('|', cache.Groups), Positions,
                Normals.ToList(), Colors.ToList(), Indices,
                TextureCoors.ToList(), newTopology);
        }

        void Add(in OBJVertex v0, in OBJVertex v1, in OBJVertex v2) {
            base.Add(v0, v1, v2, v0.V, v1.V, v2.V);
        }
    }
    public class GroupGeoBuilder : BaseGroupsBulder {
        readonly Dictionary<int, int> indxToPos;
        public GroupGeoBuilder(OBJGeometryCache _base) : base(_base) {
            Positions = new List<Vector3>();
            Indices = new List<int>();
            indxToPos = new Dictionary<int, int>();
        }

        public IEnumerable<IFileGeometry3D> Build() => Build(GeometryPrimitiveTopologies.TriangleList);

        public IEnumerable<IFileGeometry3D> Build(GeometryPrimitiveTopologies newTopology) {
            var groups = new List<IFileGeometry3D>();
            foreach (var group in cache.Groups) {
                if (group.IsEmpty) { continue; }
                Positions.Clear();
                Indices.Clear();
                indxToPos.Clear();
                switch (newTopology) {//TODO: support conveting to other topology
                    case GeometryPrimitiveTopologies.TriangleList:
                        BuildGroup(group);
                        groups.Add(new GeometryData(group.Name, Positions.ToList(),
                              Normals.ToList(), Colors.ToList(), Indices.ToList(),
                              TextureCoors.ToList(), newTopology));
                        break;
                }
            }
            return groups;
        }

        void BuildGroup(OBJGroupGeometryCache group) {
            var count = group.VerticesRange.Start + group.VerticesRange.Count;
            //first add all points 
            for (var index = group.VerticesRange.Start; index < count; index++) {
                AddPosition(cache.Vertices[index]);
            }
            Normals = cache.NormalsCache.Any() ? new Vector3[Positions.Count] : Array.Empty<Vector3>();
            Colors = cache.ColorsCache.Any() ? new Vector3[Positions.Count] : Array.Empty<Vector3>();
            TextureCoors = cache.TextureCoorsCache.Any() ? new Vector2[Positions.Count] : Array.Empty<Vector2>();
            //add the rest of data
            for (var index = group.VerticesRange.Start; index < count; index += 3) {
                Add(cache.Vertices[index], cache.Vertices[index + 1], cache.Vertices[index + 2]);
            }
        }
        void AddPosition(in OBJVertex v) {
            if (!indxToPos.ContainsKey(v.V)) {
                int indx = Positions.Count;
                indxToPos.Add(v.V, indx);
                Positions.Add(cache.PositionsCache[v.V]);
            }
        }
        void Add(in OBJVertex v0, in OBJVertex v1, in OBJVertex v2) {
            base.Add(v0, v1, v2, indxToPos[v0.V], indxToPos[v1.V], indxToPos[v2.V]);
        }

    }
}
