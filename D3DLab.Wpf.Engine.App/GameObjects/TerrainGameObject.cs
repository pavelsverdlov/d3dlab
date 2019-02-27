using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using D3DLab.Wpf.Engine.App.D3D.Components;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {

    public class SkyGameObject : SingleGameObject {
        /*
        * Sky
        * http://vterrain.org/Atmosphere/index.html
        * 
        * https://gamedev.ru/code/forum/?id=19689
        * http://www.philohome.com/skycollec/skycollec.htm
        * http://steps3d.narod.ru/tutorials/sky-tutorial.html
        */

        public static SkyGameObject Create(IEntityManager manager) {
            var tag = new ElementTag("SkyDome");
            var plane = new ElementTag("SkyPlane");

            //var geo = GenerateSphere(2, Vector3.Zero, 15f.ToRad());
            var file = new FileInfo(@"C:\Storage\projects\sv\3d\d3dlab\D3DLab.Wpf.Engine.App\Resources\sky\sky.obj");
            var points = new List<Vector3>();
            var indx = new List<int>();

            var index = 0;
            foreach (var line in File.ReadAllLines(file.FullName)) {
                var parts = line.Split(' ');
                points.Add(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));

                indx.Add(index);
                index++;
            }


            var geo = new HittableGeometryComponent();
            geo.Positions = points.ToImmutableArray();
            geo.Indices = indx.ToImmutableArray();
            geo.Color = V4Colors.White;
            //geo.TextureCoordinates = geo.Positions.Select(x => new Vector2()).ToImmutableArray();

            manager.CreateEntity(tag)
                .AddComponents(new IGraphicComponent[] {
                    new D3DSkyRenderComponent() { },
                    geo,
                    new D3DWorldTransformComponent(),
                    new GradientMaterialComponent(),
                    new FollowUpCameraPositionComponent()
                });

            var planemesh = GeometryBuilder.BuildSkyPlane(SkyPlaneData.Default);
            manager.CreateEntity(plane)
                .AddComponents(new IGraphicComponent[] {
                    new D3DSkyPlaneRenderComponent(),
                    new D3DTexturedMaterialSamplerComponent(
                        new System.IO.FileInfo(@"D:\Storage_D\trash\3d\SharpDX-Rastertek-Tutorials-master\SharpDXWinForm\Externals\Data\cloud001.bmp"),
                        new System.IO.FileInfo(@"D:\Storage_D\trash\3d\SharpDX-Rastertek-Tutorials-master\SharpDXWinForm\Externals\Data\cloud002.bmp")
                    ),
                    new SimpleGeometryComponent {
                        Positions = planemesh.Positions.ToImmutableArray(),
                        Indices = planemesh.Indices.ToImmutableArray(),
                        TextureCoordinates = planemesh.TextureCoordinates.ToImmutableArray(),
                    },
                    new D3DWorldTransformComponent(),
                    new FollowUpCameraPositionComponent(),
                    new SkyPlaneParallaxAnimationComponent()
                });

            return new SkyGameObject(tag);
        }

        static SimpleGeometryComponent GenerateSphere(float radius, Vector3 center, float stepDegree) {
            //{Minimum:X:-2 Y:-2 Z:-2 Maximum:X:2 Y:2 Z:2}
            var com = new SimpleGeometryComponent();

            for (var angle = stepDegree; angle < 360f; angle += stepDegree) {

            }

            return com;
        }

        public SkyGameObject(ElementTag tag) : base(tag, typeof(SkyGameObject).Name) {
        }
    }

   
    public class TerrainGameObject : SingleGameObject {
        /*
         * http://libnoise.sourceforge.net/ A portable, open-source, coherent noise-generating library for C++ 
         * https://github.com/JacekPrzemieniecki/VoxelTerrain
         * 
         * 
         * http://blog.wolfire.com/2010/12/Overgrowth-graphics-overview
         * http://archive.gamedev.net/archive/reference/programming/features/randomriver/page2.html
         * 
         * https://developer.nvidia.com/sites/default/files/akamai/gamedev/files/sdk/11/TerrainTessellation_WhitePaper.pdf
         * https://www.reddit.com/r/gamedev/comments/6i03ix/realistic_procedural_height_maps/
         * 
         * https://github.com/srajangarg/mesh-simplify
         * 
         * http://vterrain.org/LOD/Implementations/
         * 
         * https://apps.dtic.mil/dtic/tr/fulltext/u2/a439499.pdf
         */

        const int textureRepeat = 8;




        public TerrainGameObject(ElementTag tag) : base(tag, typeof(TerrainGameObject).Name) {
        }

        public static TerrainGameObject Create(IEntityManager manager) {
            var tag = new ElementTag("Terrain");

            var resources = Path.Combine("../../../../D3DLab.Wpf.Engine.App/Resources/terrain/");
            var grass = Path.Combine(resources, "images.jpg");// @"C:\Storage\projects\sv\3d\d3dlab\D3DLab.Wpf.Engine.App\Resources\terrain\images.jpg";// folder + "grass.bmp";
            var slope = Path.Combine(resources, "slope.bmp");
            var rock = Path.Combine(resources,"rock.bmp");
            var heigtmap = Path.Combine(resources,"1024x1024_1.bmp");

            var width = 0;
            var height = 0;
            var HeightMap = new List<Vector3>();

            using (var bitmap = new System.Drawing.Bitmap(heigtmap)) {
                width = bitmap.Width;
                height = bitmap.Height;
                float normalizeHeightMap = 10;
                // Read the image data into the height map
                for (var j = 0; j < bitmap.Height; j++) {
                    for (var i = 0; i < bitmap.Width; i++) {
                        HeightMap.Add(new Vector3(i, bitmap.GetPixel(i, j).R / normalizeHeightMap, -j));
                    }
                }
            }
            //
            var geo = new HittableGeometryComponent();

            TriangulateMap(HeightMap, height, width, geo);

            // geo.Colors = LoadColourMap(colorMapping, width, height).ToImmutableArray();

            manager.CreateEntity(tag)
                .AddComponents(new IGraphicComponent[] {
                    new D3DTerrainRenderComponent() {
                        Width = width,
                        Heigth = height
                    },
                    geo,
                    new D3DTexturedMaterialSamplerComponent(
                        new System.IO.FileInfo(grass),
                        new System.IO.FileInfo(slope),
                        new System.IO.FileInfo(rock)
                        ),
                    new D3DWorldTransformComponent(),
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

            var tvIncrementValue = incrementValue;
            var tuIncrementValue = incrementValue;

            // Loop through the entire height map and calculate the tu and tv texture coordinates for each vertex.
            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    // Store the texture coordinate in the height map.
                    texture[(height * j) + i] = new Vector2(tuCoordinate, tvCoordinate);

                    // Increment the tu texture coordinate by the increment value and increment the index by one.
                    tuCoordinate += tuIncrementValue;
                    tuCount++;

                    // Check if at the far right end of the texture and if so then start at the beginning again.
                    if (tuCount == incrementCount) {
                        tuIncrementValue *= -1;
                        //tuCoordinate = 0.0f;
                        tuCount = 0;
                    }
                }
                // Increment the tv texture coordinate by the increment value and increment the index by one.
                tvCoordinate -= tvIncrementValue;
                tvCount++;

                // Check if at the top of the texture and if so then start at the bottom again.
                if (tvCount == incrementCount) {
                    tvIncrementValue *= -1;
                    //tvCoordinate = 1.0f;
                    tvCount = 0;
                }
            }
            return texture;
        }
        static Vector4[] LoadColourMap(string fullPath, int width, int height) {
            Bitmap colourBitMap = null;
            try {
                colourBitMap = new Bitmap(fullPath);
                // This check is optional.
                // Make sure the color map dimensions are the same as the terrain dimensions for easy 1 to 1 mapping.
                var btmW = colourBitMap.Width;
                var btmH = colourBitMap.Height;

                var colors = new Vector4[btmH * btmW];

                if ((btmW != width) || (btmH != height)) {
                    throw new Exception("Textures are not compatible.");
                }

                // Read the image data into the color map portion of the height map structure.
                int index;
                for (int j = 0; j < btmH; j++)
                    for (int i = 0; i < btmW; i++) {
                        index = (btmH * j) + i;
                        colors[index].X = colourBitMap.GetPixel(i, j).R / 255.0f; // 117.75f; //// 0.678431392
                        colors[index].Y = colourBitMap.GetPixel(i, j).G / 255.0f;  //117.75f; // 0.619607866
                        colors[index].Z = colourBitMap.GetPixel(i, j).B / 255.0f;  // 117.75f; // 0.549019635
                    }

                return colors;
            } finally {
                colourBitMap?.Dispose();
            }
        }
    }
}
