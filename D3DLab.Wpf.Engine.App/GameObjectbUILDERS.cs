using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public class SingleGameObject : GameObject {
        public ElementTag Tag { get; }

        public SingleGameObject(ElementTag tag) {
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
    }
    public class CoordinateSystemLinesGameObject : GameObject {
        public ElementTag Lines { get; private set; }
        public ElementTag[] Arrows { get; private set; }

        public static CoordinateSystemLinesGameObject Build(IEntityManager manager) {
            var llength = 100;
            var obj = new CoordinateSystemLinesGameObject();
            var points = new[] {
                Vector3.Zero - Vector3.UnitX * llength, Vector3.Zero + Vector3.UnitX * llength,
                Vector3.Zero- Vector3.UnitY  * llength, Vector3.Zero + Vector3.UnitY * llength,
                Vector3.Zero- Vector3.UnitZ  * llength, Vector3.Zero + Vector3.UnitZ * llength,
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
                center = Vector3.Zero + Vector3.UnitZ * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue,
                tag = new ElementTag("Arrow_Z")
            });
            obj.Arrows[1] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = Vector3.Zero + Vector3.UnitX * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
                tag = new ElementTag("Arrow_X")
            });
            obj.Arrows[2] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitY,
                orthogonal = Vector3.UnitZ,
                center = Vector3.Zero + Vector3.UnitY * (llength - lenght + 5),
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

        public CoordinateSystemLinesGameObject() {

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

    public class SphereGameObject {
        public struct Data {
            public Vector3 Center;
            public float Radius;
        }

        private readonly ElementTag tag;

        public SphereGameObject(ElementTag tag) {
            this.tag = tag;
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

    public class TerrainGameObject : GameObject {
        public ElementTag Tag;

        public TerrainGameObject(ElementTag tag) {
            this.Tag = tag;
        }

        public static TerrainGameObject Create(IEntityManager manager) {
            var tag = new ElementTag("Terrain");
            var heigtmap = @"C:\Storage\projects\sv\3d\d3dlab\D3DLab\bin\Debug\Resources\heightmap01.bmp";
            var width = 0;
            var height = 0;
            var HeightMap = new List<Vector3>();// bitmap.Width * bitmap.Height);

            using (var bitmap = new System.Drawing.Bitmap(heigtmap)) {
                width = bitmap.Width;
                height = bitmap.Height;
                float normalizeHeightMap = 15;
                // Read the image data into the height map
                for (var j = 0; j < bitmap.Height; j++) {
                    for (var i = 0; i < bitmap.Width; i++) {
                        HeightMap.Add(new Vector3(i, bitmap.GetPixel(i, j).R / normalizeHeightMap, j));
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
                   //     HeightMap = HeightMap.ToArray(),
                    },
                    geo
                });

            return new TerrainGameObject(tag);
        }
        static void TriangulateMap(List<Vector3> HeightMap, int height, int width, GeometryComponent geometry) {
            var count = (width - 1) * (height - 1) * 6;
            var indices = new int[count];
            var vertices = new Vector3[count];

            int index = 0;

            for (int j = 0; j < (height - 1); j++) {
                for (int i = 0; i < (width - 1); i++) {
                    int indexBottomLeft1 = (height * j) + i;          // Bottom left.
                    int indexBottomRight2 = (height * j) + (i + 1);      // Bottom right.
                    int indexUpperLeft3 = (height * (j + 1)) + i;      // Upper left.
                    int indexUpperRight4 = (height * (j + 1)) + (i + 1);  // Upper right.

                    #region First Triangle
                    // Upper left.
                    vertices[index] = HeightMap[indexUpperLeft3];
                    indices[index] = index++;
                    // Upper right.
                    vertices[index] = HeightMap[indexUpperRight4];
                    indices[index] = index++;
                    // Bottom left.
                    vertices[index] = HeightMap[indexBottomLeft1];
                    indices[index] = index++;
                    #endregion

                    #region Second Triangle
                    // Bottom left.
                    vertices[index] = HeightMap[indexBottomLeft1];
                    indices[index] = index++;
                    // Upper right.
                    vertices[index] = HeightMap[indexUpperRight4];
                    indices[index] = index++;
                    // Bottom right.
                    vertices[index] = HeightMap[indexBottomRight2];
                    indices[index] = index++;
                    #endregion
                }
            }

            var normals = vertices.CalculateNormals(indices);
            geometry.Indices = indices.ToImmutableArray();
            geometry.Positions = vertices.ToImmutableArray();
            geometry.Normals = normals.ToImmutableArray();
        }

        static bool CalculateNormals(List<Vector3> HeightMap, Vector3[] normalsMap, int height, int width) {
            // Create a temporary array to hold the un-normalized normal vectors.
            int index;
            //float length;
            Vector3 vertex1, vertex2, vertex3, vector1, vector2, sum;
            var normals = new Vector3[(height - 1) * (width - 1)];

            // Go through all the faces in the mesh and calculate their normals.
            for (int j = 0; j < (height - 1); j++) {
                for (int i = 0; i < (width - 1); i++) {
                    int index1 = (j * height) + i;
                    int index2 = (j * height) + (i + 1);
                    int index3 = ((j + 1) * height) + i;

                    // Get three vertices from the face.
                    vertex1 = HeightMap[index1];
                    vertex2 = HeightMap[index2];
                    vertex3 = HeightMap[index3];

                    // Calculate the two vectors for this face.
                    vector1 = vertex1 - vertex3;
                    vector2 = vertex3 - vertex2;

                    index = (j * (height - 1)) + i;

                    // Calculate the cross product of those two vectors to get the un-normalized value for this face normal.
                    Vector3 vecTestCrossProduct = Vector3.Cross(vector1, vector2);
                    normals[index].X = vecTestCrossProduct.X;
                    normals[index].Y = vecTestCrossProduct.Y;
                    normals[index].Z = vecTestCrossProduct.Z;
                }
            }

            // Now go through all the vertices and take an average of each face normal 	
            // that the vertex touches to get the averaged normal for that vertex.
            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    // Initialize the sum.
                    sum = Vector3.Zero;

                    // Initialize the count.
                    int count = 9;

                    // Bottom left face.
                    if (((i - 1) >= 0) && ((j - 1) >= 0)) {
                        index = ((j - 1) * (height - 1)) + (i - 1);

                        sum.X += normals[index].X;
                        sum.Y += normals[index].Y;
                        sum.Z += normals[index].Z;
                        count++;
                    }
                    // Bottom right face.
                    if ((i < (width - 1)) && ((j - 1) >= 0)) {
                        index = ((j - 1) * (height - 1)) + i;

                        sum.X += normals[index].X;
                        sum.Y += normals[index].Y;
                        sum.Z += normals[index].Z;
                        count++;
                    }
                    // Upper left face.
                    if (((i - 1) >= 0) && (j < (height - 1))) {
                        index = (j * (height - 1)) + (i - 1);

                        sum.X += normals[index].X;
                        sum.Y += normals[index].Y;
                        sum.Z += normals[index].Z;
                        count++;
                    }
                    // Upper right face.
                    if ((i < (width - 1)) && (j < (height - 1))) {
                        index = (j * (height - 1)) + i;

                        sum.X += normals[index].X;
                        sum.Y += normals[index].Y;
                        sum.Z += normals[index].Z;
                        count++;
                    }

                    // Take the average of the faces touching this vertex.
                    sum.X = (sum.X / (float)count);
                    sum.Y = (sum.Y / (float)count);
                    sum.Z = (sum.Z / (float)count);

                    // Calculate the length of this normal.
                    //length = (float)Math.Sqrt((sum.X * sum.X) + (sum.Y * sum.Y) + (sum.Z * sum.Z));

                    // Get an index to the vertex location in the height map array.
                    index = (j * height) + i;

                    // Normalize the final shared normal for this vertex and store it in the height map array.
                    normalsMap[index] = sum.Normalize();// new Vector3((sum.X / length), (sum.Y / length), (sum.Z / length));
                }
            }

            // Release the temporary normals.
            normals = null;

            return true;
        }

        public override void Hide(IEntityManager manager) {
            throw new NotImplementedException();
        }

        public override void Show(IEntityManager manager) {
            throw new NotImplementedException();
        }
    }
}
