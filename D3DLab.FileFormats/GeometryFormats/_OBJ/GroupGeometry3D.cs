using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeometryFormats._OBJ {

    public class GroupGeometry3D : IFileGeometry3D {
        public IReadOnlyCollection<PartGeometry3D> Parts => groupsmap.Values;
        public IReadOnlyCollection<Vector3> Positions => positions;// mapper.Keys;
        public IReadOnlyCollection<int> Indices => indices.AsReadOnly();
        public IReadOnlyCollection<Vector2> TextureCoors => textureCoors;

        internal readonly List<Vector2> textureCoors;
        internal readonly Dictionary<Vector3, int> mapper;
        internal readonly Dictionary<int, PartGeometry3D> posIndexToPartMapper;
        readonly Dictionary<string, PartGeometry3D> groupsmap;
        public readonly List<Vector3> positions;
        public readonly List<int> indices;

        public GroupGeometry3D() {
            posIndexToPartMapper = new Dictionary<int, PartGeometry3D>();
            mapper = new Dictionary<Vector3, int>();
            positions = new List<Vector3>();
            indices = new List<int>();
            groupsmap = new Dictionary<string, PartGeometry3D>();
            textureCoors = new List<Vector2>();
        }

        public PartGeometry3D CreatePart(string fullName) {
            if (!groupsmap.TryGetValue(fullName, out var p)) {
                p = new PartGeometry3D(this, fullName);
                groupsmap.Add(fullName, p);
            }
            p.Groups.Add(new ObjGroup(fullName));
            return p;
        }

        public void Fixed() {
            posIndexToPartMapper.Clear();
            mapper.Clear();
            foreach (var kv in groupsmap.Values.ToList()) {
                if (kv.IsEmpty) {
                    groupsmap.Remove(kv.Name);
                }
            }
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
            full.textureCoors.Add(v);
        }
        public void AddPosition(ref Vector3 v) {
            full.positions.Add(v);
        }
        public void AddTriangle(IList<int> ind) {
            full.indices.AddRange(ind);
        }

        public void AddPosition1(ref Vector3 v) {
            var group = Groups.Last();
            var index = full.mapper.Count;

            if (full.mapper.ContainsKey(v)) {
                index = full.mapper[v];
            } else {
                full.mapper.Add(v, index);
            }
            full.positions.Add(v);

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
                var v = full.positions[index];
                newindexes.Add(full.mapper[v]);
            }

            if (newindexes.Count < 3) {// triangle is not triangle ... it is line, skip it
                throw new Exception($"Triangle is not triangle ... it is line [{string.Join(",", ind)}]");
            }

            var group = Groups.Last();
            for (var i = 0; i < ind.Count; ++i) {
                try {
                    var index = ind[i];
                    var v = full.positions[index];
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
                    full.indices.Add(index);
                    indices.Add(partIndex);
                } catch (Exception ex) {
                    ex.ToString();
                }
            }
        }
    }

}
