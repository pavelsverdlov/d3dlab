using D3DLab.ECS;
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
    class VisualObjectImporter {
        public void Reload(FileInfo f, LoadedVisualObject visual, WFScene scene) {
            Import(f.FullName, out var meshes, out var material, out var box);
            visual.ReCreate(scene.Context, meshes, material, f.Name);
        }
        public LoadedVisualObject ImportFromFiles(string file, WFScene scene) {
            var f = new FileInfo(file);
            Import(file,out var meshes, out var material, out var box);
            var center = box.Center;

            var loaded = LoadedVisualObject.Create(scene.Context, meshes, material, f.Name);
            //loaded.Transform(scene.Context.GetEntityManager(), Matrix4x4.CreateTranslation(Vector3.Zero - center));

            return loaded;
        }

        static void Import(string file, out IEnumerable<IFileGeometry3D> meshes, out FileInfo material, out AxisAlignedBox box) {
            box = AxisAlignedBox.Zero;
            material = null;
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
        }
    }
}
