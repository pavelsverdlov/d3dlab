using D3DLab.ECS.Components;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.Toolkit.Math3D;
using D3DLab.Viewer.D3D;

using System.Collections.Generic;
using System.IO;

namespace D3DLab.Viewer.Presentation {
    class VisualObjectExporter {
        public void Export(LoadedVisualObject loaded, FileInfo path, WFScene scene) {
            var context = scene.Context;
            var manager = context.GetComponentManager();
            switch (path.Extension.ToLower()) {
                case ".obj":
                    var meshes = new List<IFileGeometry3D>();
                    foreach (var tag in loaded.Tags) {
                        var matrix = manager.GetComponent<TransformComponent>(tag).MatrixWorld;
                        var geo = loaded.GetMesh(context, tag).OriginGeometry;
                        geo.ApplyMatrix(ref matrix);
                        meshes.Add(geo);
                    }

                    G3Writers.WriteObj(path, meshes);
                    break;
            }
        }
    }
}
