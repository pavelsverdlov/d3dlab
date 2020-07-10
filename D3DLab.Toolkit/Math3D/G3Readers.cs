using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;

using g3;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public static class G3Readers {
        class MeshData : IFileGeometry3D {
            public MeshData(string name, 
                ReadOnlyCollection<Vector3> positions, ReadOnlyCollection<Vector3> normals, 
                ReadOnlyCollection<int> indices, ReadOnlyCollection<Vector2> textureCoors, 
                ReadOnlyCollection<Vector3> colors) {
                Name = name;
                Positions = positions;
                Normals = normals;
                Indices = indices;
                TextureCoors = textureCoors;
                Colors = colors;
                Topology = GeometryPrimitiveTopologies.TriangleList;
            }

            public string Name { get; }
            public ReadOnlyCollection<Vector3> Positions { get; }
            public ReadOnlyCollection<Vector3> Normals { get; }
            public ReadOnlyCollection<int> Indices { get; }
            public ReadOnlyCollection<Vector2> TextureCoors { get; }
            public ReadOnlyCollection<Vector3> Colors { get; }
            public GeometryPrimitiveTopologies Topology { get; }
        }

        public static IEnumerable<IFileGeometry3D> ReadStl(FileInfo file) {
            var builder = new SimpleMeshBuilder();
            var stl = new STLReader();
            IOReadResult res;
            using (var fs = file.OpenRead()) {
                using(var br = new BinaryReader(fs)) {
                    res = stl.Read(br, new ReadOptions { }, builder);
                }
            }
            if(res.code == IOCode.Ok && builder.Meshes.Any()) {
                var mesh = builder.Meshes.Single();
                return new[] {
                    new MeshData("STL",
                    mesh.GetVertexArrayFloat().ToVector3List().AsReadOnly(),
                    mesh.HasVertexNormals ? 
                        mesh.GetVertexNormalArray().ToVector3List().AsReadOnly() 
                        : new ReadOnlyCollection<Vector3>(new List<Vector3>()),
                    mesh.GetTriangleArray().AsReadOnly(),
                    mesh.HasVertexUVs ? 
                        mesh.GetVertexUVArray().ToVector2List().AsReadOnly()
                        : new ReadOnlyCollection<Vector2>(new List<Vector2>()),
                    mesh.HasVertexColors ? 
                        mesh.GetVertexColorArray().ToVector3List().AsReadOnly()
                        :  new ReadOnlyCollection<Vector3>(new List<Vector3>())
                    )
                };
            }
            throw new Exception("Can't read STL.");
        }

        
    }
}
