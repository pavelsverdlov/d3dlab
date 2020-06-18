using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.FileFormats.GeoFormats._OBJ;
using D3DLab.Viewer.D3D;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Viewer.Presentation {
    class VisualObjectLoader {
        public IEnumerable<LoadedVisualObject> LoadFromFiles(IEnumerable<string> files, WFScene scene) {
            var loads = new List<LoadedVisualObject>();
            foreach (var file in files) {
                var f = new FileInfo(file);
                switch (f.Extension) {
                    case ".obj":
                        var parser = new Utf8ByteOBJParser();
                        using (var reader = new FileFormats.MemoryMappedFileReader(f)) {
                            parser.Read(reader.ReadSpan());
                        }

                        FileInfo material = null;
                        try {
                            material = parser.HasMTL ?
                                new FileInfo(parser.GetMaterialFilePath(f.Directory, f.Directory)) : null;
                        } catch { }

                        //var builder = new UnitedGroupsBulder(parser.GeometryCache);
                        var builder = new GroupGeoBuilder(parser.GeometryCache);
                        
                        var meshes = builder.Build();
                        var box = AxisAlignedBox.CreateFrom(parser.GeometryCache.PositionsCache.AsReadOnly());
                        var center = box.Center;
                        var move = MovingComponent.CreateTranslation(Vector3.Zero - center);

                        var loaded = LoadedVisualObject.Create(scene.Context, meshes, material, f.Name);
                        loaded.UpdateComponent(scene.Context.GetEntityManager(), move);

                        loads.Add(loaded);

                        break;
                }
            }

            return loads;
        }
    }
}
