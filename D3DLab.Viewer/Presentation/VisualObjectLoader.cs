using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.FileFormats.GeoFormats._OBJ;
using D3DLab.Toolkit.Math3D;
using D3DLab.Viewer.D3D;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Viewer.Presentation {
    class VisualObjectLoader {
        public LoadedVisualObject LoadFromFiles(string file, WFScene scene) {
            IEnumerable<IFileGeometry3D> meshes;
            FileInfo material = null;
            AxisAlignedBox box = AxisAlignedBox.Zero;
            var f = new FileInfo(file);
            switch (f.Extension.ToLower()) {
                case ".stl":
                    meshes = G3Readers.ReadStl(f);
                    box = AxisAlignedBox.CreateFrom(meshes.First().Positions);
                    break;
                case ".obj":
                    var parser = new Utf8ByteOBJParser();
                    using (var reader = new FileFormats.MemoryMappedFileReader(f)) {
                        parser.Read(reader.ReadSpan());
                    }

                    try {
                        material = parser.HasMTL ?
                            new FileInfo(parser.GetMaterialFilePath(f.Directory, f.Directory)) : null;
                    } catch { }

                    //var builder = new UnitedGroupsBulder(parser.GeometryCache);
                    var builder = new GroupGeoBuilder(parser.GeometryCache);

                    meshes = builder.Build();
                    box = AxisAlignedBox.CreateFrom(parser.GeometryCache.PositionsCache.AsReadOnly());

                    break;
                default:
                    throw new NotSupportedException($"'{f.Extension}' is not suppported format.");
            }
            var center = box.Center;

            var loaded = LoadedVisualObject.Create(scene.Context, meshes, material, f.Name);
            loaded.Transform(scene.Context.GetEntityManager(), Matrix4x4.CreateTranslation(Vector3.Zero - center));

            return loaded;
        }
    }
}
