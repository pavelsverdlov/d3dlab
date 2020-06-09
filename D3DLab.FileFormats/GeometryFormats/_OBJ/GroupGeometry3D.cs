using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeometryFormats._OBJ {
    public readonly struct Face {
        public readonly int Vi;
        public readonly int VTi;
        public readonly int VNi;
    }
    public class GroupGeometry3D : IFileGeometry3D {
        public IReadOnlyCollection<PartGeometry3D> Parts => groupsmap.Values;
        public IReadOnlyCollection<Vector3> Positions => positionsCache;
        public IReadOnlyCollection<Vector3> Normals => normalsCache;
        public IReadOnlyCollection<Vector3> Colors => colorsCache;
        public IReadOnlyCollection<int> Indices => indicesCache;
        public IReadOnlyCollection<Vector2> TextureCoors => textureCoorsCache;
        public List<Face> Faces { get; private set; }

        internal readonly Dictionary<Vector3, int> mapper;
        internal readonly Dictionary<int, PartGeometry3D> posIndexToPartMapper;
        readonly Dictionary<string, PartGeometry3D> groupsmap;

        internal readonly List<Vector2> textureCoorsCache;
        internal readonly List<Vector3> positionsCache;
        internal readonly List<int> indicesCache;
        internal readonly List<int> indicesFunCache;
        internal readonly List<Vector3> normalsCache;
        internal readonly List<Vector3> colorsCache;

        public GroupGeometry3D() {
            posIndexToPartMapper = new Dictionary<int, PartGeometry3D>();
            mapper = new Dictionary<Vector3, int>();
            positionsCache = new List<Vector3>();
            colorsCache = new List<Vector3>();
            normalsCache = new List<Vector3>();
            indicesCache = new List<int>();
            indicesFunCache = new List<int>();
            groupsmap = new Dictionary<string, PartGeometry3D>();
            textureCoorsCache = new List<Vector2>();

            Faces = new List<Face>();
        }

        public PartGeometry3D CreatePart(string fullName) {
            if (!groupsmap.TryGetValue(fullName, out var p)) {
                p = new PartGeometry3D(this, fullName);
                groupsmap.Add(fullName, p);
            }
            p.Groups.Add(new ObjGroup(fullName));
            return p;
        }

        internal void Build() {
            //if (!Positions.Any()) {
            //    Positions = positionsCache;
            //}
            ////if (!Indices.Any()) {
            ////    Indices = indicesCache;
            ////}
            //if (!TextureCoors.Any()) {
            //    TextureCoors = textureCoors;
            //}
            //if (!Normals.Any()) {
            //    Normals = normalsCache;
            //}
            //if (!Colors.Any()) {
            //    Colors = colorsCache;
            //}
        }
    }
    public class PartGeometry3D {
        public IReadOnlyCollection<Vector3> Positions => mapper.Keys;
        public IReadOnlyCollection<int> Indices => indices.AsReadOnly();

        public bool IsEmpty => mapper.Count == 0 && indices.Count == 0;

        readonly List<int> indices;

        public readonly Dictionary<Vector3, int> mapper;
        public readonly GroupGeometry3D full;
        public readonly string Name;

        public readonly List<ObjGroup> Groups;

        public PartGeometry3D(GroupGeometry3D group, string key) {
            mapper = new Dictionary<Vector3, int>();
            indices = new List<int>();
            this.full = group;
            this.Name = key;
            Groups = new List<ObjGroup>();
        }
        public void AddTextureCoor(ref Vector2 v) {
            full.textureCoorsCache.Add(v);
        }
        public void AddPosition(ref Vector3 v) {
            full.positionsCache.Add(v);
        }
        public void AddTriangle(int ind0, int ind1, int ind2) {
            full.indicesCache.Add(ind0);
            full.indicesCache.Add(ind1);
            full.indicesCache.Add(ind2);
        }
        public void AddNormal(ref Vector3 v) {
            full.normalsCache.Add(v);
        }
        public void AddColor(ref Vector3 v) {
            full.colorsCache.Add(v);
        }

        /// <summary>
        /// A triangle fan is similar to a triangle strip, keep that structure 
        /// to allow converting it to triangle strip for rendering (to improve render speed)
        /// </summary>
        /// <param name="ind"></param>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/windows/win32/direct3d9/triangle-fans
        /// </remarks>
        public void AddTrianglesFun(int ind0, int ind1, int ind2, int ind3) {
            full.indicesFunCache.Add(ind0);
            full.indicesFunCache.Add(ind1);
            full.indicesFunCache.Add(ind2);
            full.indicesFunCache.Add(ind3);
        }
        public void AddFace(Face face) {
            full.Faces.Add(face);
        }


        public void AddTriangles(int[] ind) {
            full.indicesCache.AddRange(ind);
        }
        public void AddVertexIndex(int v) {
            full.indicesCache.Add(v);
        }

        public void AddPosition1(ref Vector3 v) {
            var group = Groups.Last();
            var index = full.mapper.Count;

            if (full.mapper.ContainsKey(v)) {
                index = full.mapper[v];
            } else {
                full.mapper.Add(v, index);
            }
            full.positionsCache.Add(v);

            if (group.PosGroupInfo == null) {
                group.PosGroupInfo = new ObjGroupInfo {
                    Name = group.Name,
                    StartIndex = index,
                };
            }

            group.PosGroupInfo.Count += 1;
        }

        public void AddTriangle1(IList<int> ind) {
            var newindexes = new HashSet<int>();
            for (var i = 0; i < ind.Count; ++i) {
                var index = ind[i];
                var v = full.positionsCache[index];
                newindexes.Add(full.mapper[v]);
            }

            if (newindexes.Count < 3) {// triangle is not triangle ... it is line, skip it
                throw new Exception($"Triangle is not triangle ... it is line [{string.Join(",", ind)}]");
            }

            var group = Groups.Last();
            for (var i = 0; i < ind.Count; ++i) {
                try {
                    var index = ind[i];
                    var v = full.positionsCache[index];
                    index = full.mapper[v];

                    if (!mapper.TryGetValue(v, out var partIndex)) {
                        partIndex = mapper.Count;
                        mapper.Add(v, partIndex);
                    }

                    if (group.IndxGroupInfo == null) {
                        group.IndxGroupInfo = new ObjGroupInfo {
                            Name = group.Name,
                            StartIndex = index,
                            Count = 1
                        };
                    } else {
                        group.IndxGroupInfo.Count += 1;
                    }
                    full.indicesCache.Add(index);
                    indices.Add(partIndex);
                } catch (Exception ex) {
                    ex.ToString();
                }
            }
        }

       
    }

}
