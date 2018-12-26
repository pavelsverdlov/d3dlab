using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace D3DLab.Wpf.Engine.App {
    public class SingleGameObject : GameObject {
        public ElementTag Tag { get; }

        public SingleGameObject(ElementTag tag,string desc) :base(desc){
            Tag = tag;
        }

        public override void Hide(IEntityManager manager) {
            manager.GetEntity(Tag)
                  .GetComponent<IRenderableComponent>()
                  .CanRender = false;
        }

        public override void Show(IEntityManager manager) {
            manager.GetEntity(Tag)
                  .GetComponent<IRenderableComponent>()
                  .CanRender = true;
        }

        public override void LookAtSelf(IEntityManager manager) {
            var entity = manager.GetEntity(Tag);
            var geos = entity.GetComponents<IGeometryComponent>();
            if (geos.Any()) {
                var geo = geos.First();
                var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition = geo.Box.GetCenter() };
                entity.AddComponent(com);
            }
        }

        public override void Dispose(IEntityManager manager) {
            base.Dispose(manager);
            manager.RemoveEntity(Tag);
        }
    }

    public class CoordinateSystemLinesGameObject : GameObject {
        public ElementTag Lines { get; private set; }
        public ElementTag[] Arrows { get; private set; }

        public static CoordinateSystemLinesGameObject Build(IEntityManager manager, Vector3 center = new Vector3()) {
            var llength = 100;
            var obj = new CoordinateSystemLinesGameObject();
            var points = new[] {
                center - Vector3.UnitX * llength, center + Vector3.UnitX * llength,
                center- Vector3.UnitY  * llength, center + Vector3.UnitY * llength,
                center- Vector3.UnitZ  * llength, center + Vector3.UnitZ * llength,
            };
            var color = new[] {
                V4Colors.Green, V4Colors.Green,
                V4Colors.Red, V4Colors.Red,
                V4Colors.Blue, V4Colors.Blue,
            };
            obj.Lines = PolylineGameObject.Create(manager, new ElementTag("Coordinate System"),
                points, color).Tag;
            var lenght = 20.0f;
            var radius = 5.0f;

            obj.Arrows = new ElementTag[3];
            obj.Arrows[0] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitZ,
                orthogonal = Vector3.UnitX,
                center = center + Vector3.UnitZ * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue,
                tag = new ElementTag("Arrow_Z")
            });
            obj.Arrows[1] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = center + Vector3.UnitX * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
                tag = new ElementTag("Arrow_X")
            });
            obj.Arrows[2] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitY,
                orthogonal = Vector3.UnitZ,
                center = center + Vector3.UnitY * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Red,
                tag = new ElementTag("Arrow_Y")
            });

            return obj;
        }

        public override void Show(IEntityManager manager) {
            foreach (var tag in Arrows) {
                manager.GetEntity(tag)
                    .GetComponent<IRenderableComponent>()
                    .CanRender = true;
            }
            manager.GetEntity(Lines)
                .GetComponent<IRenderableComponent>()
                .CanRender = true;
        }

        public override void Hide(IEntityManager manager) {
            foreach (var tag in Arrows) {
                manager.GetEntity(tag)
                    .GetComponent<IRenderableComponent>()
                    .CanRender = false;
            }
            manager.GetEntity(Lines)
                .GetComponent<IRenderableComponent>()
                .CanRender = false;
        }

        public CoordinateSystemLinesGameObject():base(typeof(CoordinateSystemLinesGameObject).Name) {

        }
    }

    public class ArrowGameObject {
        public ElementTag Tag { get; }

        public ArrowGameObject(ElementTag tag) {
            Tag = tag;
        }

        public static ArrowGameObject Build(IEntityManager manager, ArrowData data) {
            var en = manager
              .CreateEntity(data.tag)
              .AddComponent(GeometryBuilder.BuildArrow(data))
              .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
              .AddComponent(new SDX.Engine.Components.D3DTransformComponent());

            return new ArrowGameObject(en.Tag);
        }

        [Obsolete("Remove")]
        public static ElementTag BuildArrow(IEntityManager manager, ArrowData data) {
            return manager
               .CreateEntity(data.tag)
               .AddComponent(GeometryBuilder.BuildArrow(data))
               .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;
        }
    }

    public class OrbitsRotationGameObject {
        public static OrbitsRotationGameObject Create(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("OrbitsRotationGameObject"))
               .AddComponent(GeometryBuilder.BuildRotationOrbits(100, Vector3.Zero))
               .AddComponent(D3DLineVertexRenderComponent.AsLineStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;

            return new OrbitsRotationGameObject();
        }
    }

    public class SphereGameObject : SingleGameObject {
        public struct Data {
            public Vector3 Center;
            public Vector4 Color;
            public float Radius;
        }

        public SphereGameObject(ElementTag tag) :base(tag,"SphereByPoint") {
        }

        public static SphereGameObject Create(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("Sphere_" + DateTime.Now.Ticks))
               .AddComponent(new GeometryComponent() {
                   Indices = new[] { 0 }.ToImmutableArray(),
                   Positions = new Vector3[] { Vector3.Zero }.ToImmutableArray(),
                   Color = V4Colors.Red,
               })
               .AddComponent(new D3DSphereRenderComponent())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;


            return new SphereGameObject(tag);
        }
        public static SphereGameObject Create(IEntityManager manager, Data data) {
            var tag = manager
               .CreateEntity(new ElementTag("Sphere_" + DateTime.Now.Ticks))
               .AddComponent(new GeometryComponent() {
                   Indices = new[] { 0 }.ToImmutableArray(),
                   Positions = new Vector3[] { data.Center }.ToImmutableArray(),
                   Color = data.Color,
               })
               .AddComponent(new D3DSphereRenderComponent())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;


            return new SphereGameObject(tag);
        }
    }

    public class PolylineGameObject {
        public ElementTag Tag { get; }

        public PolylineGameObject(ElementTag tag1) {
            this.Tag = tag1;
        }

        public static PolylineGameObject Create(IEntityManager manager, ElementTag tag,
            IEnumerable<Vector3> points, IEnumerable<Vector4> color) {
            var indeces = new List<int>();
            for (var i = 0; i < points.Count(); i++) {
                indeces.AddRange(new[] { i, i });
            }

            var tag1 = manager
               .CreateEntity(tag)
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                   Positions = points.ToImmutableArray(),
                   Indices = indeces.ToImmutableArray(),
                   Colors = color.ToImmutableArray(),
               })
               .AddComponent(SDX.Engine.Components.D3DLineVertexRenderComponent.AsLineList())
               .Tag;

            return new PolylineGameObject(tag1);
        }
    }

    public class TerrainGameObject : SingleGameObject {
        /*
         * http://libnoise.sourceforge.net/ A portable, open-source, coherent noise-generating library for C++ 
         * https://github.com/JacekPrzemieniecki/VoxelTerrain
         * 
         */

        const int textureRepeat = 8;


        public TerrainGameObject(ElementTag tag) : base(tag,typeof(TerrainGameObject).Name) {
        }

        public static TerrainGameObject Create(IEntityManager manager) {
            var tag = new ElementTag("Terrain");
            var heigtmap = @"C:\Storage\projects\sv\3d\d3dlab\D3DLab\bin\x64\Debug\Resources\heightmap01.bmp";
            var texture_bnm = @"C:\Storage\projects\sv\3d\d3dlab\D3DLab\bin\x64\Debug\Resources\dirt03.bmp";

            var width = 0;
            var height = 0;
            var HeightMap = new List<Vector3>();

            using (var bitmap = new System.Drawing.Bitmap(heigtmap)) {
                width = bitmap.Width;
                height = bitmap.Height;
                float normalizeHeightMap = 15;
                // Read the image data into the height map
                for (var j = 0; j < bitmap.Height; j++) {
                    for (var i = 0; i < bitmap.Width; i++) {
                        HeightMap.Add(new Vector3(i, j, bitmap.GetPixel(i, j).R / normalizeHeightMap));
                    }
                }
            }
            //
            var geo = new GeometryComponent();
            TriangulateMap(HeightMap, height, width, geo);

            manager.CreateEntity(tag)
                .AddComponents(new IGraphicComponent[] {
                    new D3DTerrainRenderComponent() {
                        Width = width,
                        Heigth = height,
                    },
                    geo,
                    new D3DTexturedMaterialComponent(new System.IO.FileInfo(texture_bnm))
                });

            return new TerrainGameObject(tag);
        }

        static void TriangulateMap(List<Vector3> HeightMap, int height, int width, GeometryComponent geometry) {
            var count = (width - 1) * (height - 1) * 6;
            var indices = new int[count];
            var vertices = HeightMap.ToArray();
            var index = 0;
            for (int j = 0; j < (height - 1); j++) {
                var row = height * j;
                var row2 = height * (j + 1);
                for (int i = 0; i < (width - 1); i++) {
                    var indx1 = row + i;
                    var indx2 = row + i + 1;
                    var indx3 = row2 + i;
                    var indx4 = row2 + i + 1;

                    indices[index++] = indx1;
                    indices[index++] = indx2;
                    indices[index++] = indx3;

                    indices[index++] = indx2;
                    indices[index++] = indx4;
                    indices[index++] = indx3;
                }
            }
            var normals = vertices.CalculateNormals(indices);
            geometry.Indices = indices.ToImmutableArray();
            geometry.Positions = vertices.ToImmutableArray();
            geometry.Normals = normals.ToImmutableArray();
            geometry.TextureCoordinates = CalculateTextureCoordinates(HeightMap, width, height).ToImmutableArray();
            geometry.Color = V4Colors.White;

            //Light.SetAmbientColor(0.05f, 0.05f, 0.05f, 1.0f);
            //Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        }
        static Vector2[] CalculateTextureCoordinates(List<Vector3> HeightMap, int width, int height) {
            // Calculate how much to increment the texture coordinates by.
            float incrementValue = (float)textureRepeat / (float)width;

            // Calculate how many times to repeat the texture.
            int incrementCount = width / textureRepeat;

            // Initialize the tu and tv coordinate values.
            float tuCoordinate = 0.0f;
            float tvCoordinate = 1.0f;

            // Initialize the tu and tv coordinate indexes.
            int tuCount = 0;
            int tvCount = 0;
            var texture = new Vector2[HeightMap.Count];
            // Loop through the entire height map and calculate the tu and tv texture coordinates for each vertex.
            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    // Store the texture coordinate in the height map.
                    texture[(height * j) + i] = new Vector2(tuCoordinate, tvCoordinate);

                    // Increment the tu texture coordinate by the increment value and increment the index by one.
                    tuCoordinate += incrementValue;
                    tuCount++;

                    // Check if at the far right end of the texture and if so then start at the beginning again.
                    if (tuCount == incrementCount) {
                        tuCoordinate = 0.0f;
                        tuCount = 0;
                    }
                }

                // Increment the tv texture coordinate by the increment value and increment the index by one.
                tvCoordinate -= incrementValue;
                tvCount++;

                // Check if at the top of the texture and if so then start at the bottom again.
                if (tvCount == incrementCount) {
                    tvCoordinate = 1.0f;
                    tvCount = 0;
                }
            }
            return texture;
        }
        
    }
    public class CameraGameObject : SingleGameObject {
        static int cameras = 0;

        public CameraGameObject(ElementTag tag, string descr) : base(tag, descr) { }

        public static CameraGameObject Create(IContextState context) {
            IEntityManager manager = context.GetEntityManager();
            var cameraTag = new ElementTag("CameraEntity_" + Interlocked.Increment(ref cameras));

            var obj = new CameraGameObject(cameraTag, "PerspectiveCamera");

            manager.CreateEntity(cameraTag)
                   //.AddComponent(new OrthographicCameraComponent(Window.Width, Window.Height));
                   .AddComponent(new PerspectiveCameraComponent());

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<SDX.Engine.Rendering.RenderSystem>(cameraTag, 0)
                       .RegisterOrder<Std.Engine.Core.Systems.InputSystem>(cameraTag, 0);
            }

            return obj;
        }

    }

    public class LightGameObject : SingleGameObject {
        static float lightpower = 1;
        static int lights = 0;

        GameObject debugVisualObject;
        CoordinateSystemLinesGameObject coordinateSystemObject;

        public LightGameObject(ElementTag tag, string desc) : base(tag, desc) { }

        public static LightGameObject CreateDirectionLight(IEntityManager manager, Vector3 direction) {// ,
            var tag = new ElementTag("DirectionLight_" + Interlocked.Increment(ref lights));
            manager.CreateEntity(tag)
                   .AddComponents(
                       new LightComponent {
                           Index = 2,
                           Intensity = 0.2f,
                           Direction = direction,
                           Type = LightTypes.Directional
                       },
                       new ColorComponent { Color = new Vector4(1, 1, 1, 1) }
                   );

            return new LightGameObject(tag, "DirectionLight");
        }

        public static LightGameObject CreatePointLight(IEntityManager manager, Vector3 position) {// 
            var tag = new ElementTag("PointLight_" + Interlocked.Increment(ref lights));

            manager.CreateEntity(tag)
                 .AddComponents(
                     new LightComponent {
                         Index = 1,
                         Intensity = 0.4f,
                         Position = position,
                         Type = LightTypes.Point
                     },
                     new ColorComponent { Color = new Vector4(1, 1, 1, 1) }
                 );

            return new LightGameObject(tag, "PointLight");
        }

        public static LightGameObject CreateAmbientLight(IEntityManager manager) {
            var tag = new ElementTag("AmbientLight_" + Interlocked.Increment(ref lights));

            manager.CreateEntity(tag)
                   .AddComponents(
                           new LightComponent {
                               Index = 0,
                               Intensity = 0.4f,
                               //Position = Vector3.Zero + Vector3.UnitZ * 1000,
                               Type = LightTypes.Ambient
                           },
                           new ColorComponent { Color = V4Colors.White }
                       );

            return new LightGameObject(tag, "AmbientLight");
        }

        public override void ShowDebugVisualization(IEntityManager manager) {
            if (debugVisualObject.IsNotNull()) {
                debugVisualObject.Show(manager);
                coordinateSystemObject.Show(manager);
                return;
            }

            var entity = manager.GetEntity(Tag);
            var l = entity.GetComponent<LightComponent>();
            var c = entity.GetComponent<ColorComponent>();

            var center = l.Position;
            switch (l.Type) {
                case LightTypes.Point:
                    debugVisualObject = SphereGameObject.Create(manager, new SphereGameObject.Data {
                        Center = center,
                        Color = V4Colors.Red,// c.Color * l.Intensity
                    });
                    break;
            }

            coordinateSystemObject = CoordinateSystemLinesGameObject.Build(manager, center);

            base.ShowDebugVisualization(manager);
        }

        public override void HideDebugVisualization(IEntityManager manager) {
            debugVisualObject.Hide(manager);
            coordinateSystemObject.Hide(manager);

            base.HideDebugVisualization(manager);
        }

        public override void LookAtSelf(IEntityManager manager) {
            var entity = manager.GetEntity(Tag);
            var l = entity.GetComponent<LightComponent>();

            var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition =l.Position };

            manager.GetEntity(Tag).AddComponent(com);
        }
    }
}
