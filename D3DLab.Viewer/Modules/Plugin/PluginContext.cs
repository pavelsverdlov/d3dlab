using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;

using D3DLab.ECS;
using D3DLab.Plugin;
using D3DLab.Toolkit;
using D3DLab.Toolkit.D3Objects;
using D3DLab.Toolkit.Math3D;

namespace D3DLab.Viewer.Modules.Plugin {
    class PluginContext : IPluginContext {
        public IEnumerable<IPluginLoadedObjectDetails> Objects { get; }
        public IPluginScene Scene { get; }
        public DirectoryInfo PluginDirectory { get; }
        //public Window Window { get; }

        public PluginContext(IPluginScene scene, Window window, DirectoryInfo directory, IEnumerable<IPluginLoadedObjectDetails> objects) {
            Scene = scene;
            PluginDirectory = directory;
            Objects = objects;
          //  Window = window;
        }
    }

    class PluginLoadedObjectDetails : IPluginLoadedObjectDetails {
        public FileInfo FilePath { get; }
        public IEnumerable<ElementTag> VisualObjectTags { get; }

        public PluginLoadedObjectDetails(FileInfo filePath, IEnumerable<ElementTag> visualObjectTags) {
            FilePath = filePath;
            VisualObjectTags = visualObjectTags;
        }
    }

    class PluginScene : IPluginScene {
        public IContextState Context { get; }

        public PluginScene(IContextState context) {
            Context = context;
        }
        public GameObject DrawPoint(string key, Vector3 center, Vector4 color) {
            return VisualSphereObject.Create(Context, ElementTag.New($"{key}_Point"), new VisualSphereObject.Data {
                Center = center,
                Color = color,
                Radius = .1f
            });
        }
        public GameObject DrawArrow(string key, ArrowDetails arrowData) {
            var llength = 10;

            var tt = new ArrowData {
                axis = arrowData.Axis,
                orthogonal = arrowData.Orthogonal,
                center = arrowData.Center + arrowData.Axis * (llength - 2),
                lenght = 2.1f,
                radius = .8f,
                color = arrowData.Color
            };

            var points = new[] {
                arrowData.Center, arrowData.Center + arrowData.Axis * llength,
            };

            var arrow = ArrowGameObject.Create(Context, ElementTag.New($"{key}_arrowhead"), tt);

            var line = VisualPolylineObject.Create(Context, ElementTag.New($"{key}_arrowline"),
               points, arrowData.Color, true);

            return new MultiVisualObject(new[] { arrow.Tag, line.Tag }, key);
        }

        public GameObject DrawPolyline(string key, Vector3[] margin, Vector4 green) {
            var line = VisualPolylineObject.Create(Context, ElementTag.New($"{key}_polyline"),
             margin, green, true);

            return line;
        }

        public GameObject DrawCylinder(string key, CylinderDetails cyl) {
            var geo = GeometryBuilder.BuildCylinder(cyl.Start, cyl.Axis, cyl.Radius, cyl.Length);

            var en = EntityBuilders.BuildColored(Context, ElementTag.New(key), geo, cyl.Color, SharpDX.Direct3D11.CullMode.None);

            return new SingleVisualObject(en.Tag, key);
        }
    }
}
