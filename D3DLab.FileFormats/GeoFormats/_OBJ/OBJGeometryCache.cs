using D3DLab.ECS;
using D3DLab.ECS.Ext;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats._OBJ {
    public enum OBJVertexType {
        Single,
        Triple
    }
    public readonly struct OBJVertex {
        public readonly int V;
        public readonly int VT;
        public readonly int VN;

        public readonly OBJVertexType Type;

        public OBJVertex(int vi, int vTi, int vNi) {
            V = vi;
            VT = vTi;
            VN = vNi;
            Type = OBJVertexType.Triple;
        }
        public OBJVertex(int vi) {
            V = vi;
            VT = -1;
            VN = -1;
            Type = OBJVertexType.Single;
        }
        public override string ToString() {
            return $"{V}/{VN}/{VT}";
        }
    }

    public class OBJGeometryCache {
        public List<OBJGroupGeometryCache> Groups { get; }
        public List<OBJVertex> Vertices { get; }

        public readonly List<Vector2> TextureCoorsCache;
        public readonly List<Vector3> PositionsCache;
        public readonly List<Vector3> NormalsCache;
        public readonly List<Vector3> ColorsCache;
        
        public OBJGeometryCache() {
            Groups = new List<OBJGroupGeometryCache>();
            PositionsCache = new List<Vector3>();
            ColorsCache = new List<Vector3>();
            NormalsCache = new List<Vector3>();
            TextureCoorsCache = new List<Vector2>();

            Vertices = new List<OBJVertex>();
        }

        internal OBJGroupGeometryCache CreatePart(string fullName) {
            var group = new OBJGroupGeometryCache(this, fullName);
            Groups.Add(group);
            return group;
        }
    }
    internal class ListRange {
        public int Start;
        public int Count;
        public override string ToString() {
            return $"{Start}/{Count}";
        }
    }
    public class OBJGroupGeometryCache {
        public bool IsEmpty => VerticesRange.Count == 0;
        public readonly string Name;
        
        internal readonly ListRange VerticesRange;
        readonly OBJGeometryCache builder;

        public OBJGroupGeometryCache(OBJGeometryCache builder, string key) {
            this.builder = builder;
            this.Name = key;
            VerticesRange = new ListRange {
                Start = this.builder.Vertices.Count
            };
        }

        public void AddTextureCoor(ref Vector2 v) {
            builder.TextureCoorsCache.Add(v);
        }
        public void AddPosition(ref Vector3 v) {
            builder.PositionsCache.Add(v);
        }
        public void AddNormal(ref Vector3 v) {
            builder.NormalsCache.Add(v);
        }
        public void AddColor(ref Vector3 v) {
            builder.ColorsCache.Add(v);
        }

        public void AddVertices(OBJVertex[] vertex, GeometryPrimitiveTopologies topology) {
            //one group can have several topologies
            //example:
            //f 74513/2367/55537 74560/2366/55584 74514/2368/55538 
            //f 74514/2275/55538 74561/2369/55585 74562/2370/55586 74515/2276/55539 
            //convert to one topology to simplify post processing
            switch (topology) {
                case GeometryPrimitiveTopologies.TriangleList:
                    builder.Vertices.Add(vertex[0]);
                    builder.Vertices.Add(vertex[1]);
                    builder.Vertices.Add(vertex[2]);
                    VerticesRange.Count += 3;
                    break;
                case GeometryPrimitiveTopologies.TriangleFan:
                    //suport triangle fan format https://en.wikipedia.org/wiki/Triangle_fan
                    //https://docs.microsoft.com/en-us/windows/win32/direct3d9/triangle-fans
                    builder.Vertices.Add(vertex[0]);
                    builder.Vertices.Add(vertex[1]);
                    builder.Vertices.Add(vertex[2]);

                    builder.Vertices.Add(vertex[0]);
                    builder.Vertices.Add(vertex[2]);
                    builder.Vertices.Add(vertex[3]);
                    VerticesRange.Count += 6;
                    break;
            }
        }


    }

}
