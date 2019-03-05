using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using D3DLab.Std.Engine.Core.Utilities;


namespace D3DLab.Std.Engine.Core.Systems {
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using D3DLab.Std.Engine.Core.Common;
    using D3DLab.Std.Engine.Core.Components;
    using D3DLab.Std.Engine.Core.Ext;
    using LibNoise;
    using LibNoise.Builder;
    using LibNoise.Filter;
    using LibNoise.Modifier;
    using LibNoise.Primitive;
    using LibNoise.Transformer;

    public interface ITerrainComponent : IGraphicComponent { }

    public class TerrainGeneratorSystem : BaseEntitySystem, IGraphicSystem {
        public void Execute(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            foreach (var entity in emanager.GetEntities()) {
                var coms = entity.GetComponents<ITerrainComponent>();
                if (!coms.Any()) {
                    continue;
                }
                var conf = coms.OfType<TerrainConfigurationComponent>().Single();
                if (conf.IsModified) {
                    entity.RemoveComponents<TerrainGeneratorComponent>();
                    conf.IsModified = false;
                    var generator = new TerrainGeneratorComponent(conf.Width, conf.Height,
                        new TerrainGeneratorComponent.TerrainParams {
                            ElevationPower = conf.ElevationPower,

                        });
                    generator.StartGeneratingAsync();
                    entity.AddComponent(generator);
                    continue;
                }
                var generating = coms.OfType<TerrainGeneratorComponent>().Single();
                if (generating.IsGenerated) {
                    entity.RemoveComponent(generating);

                    var geo = entity.GetComponents<HittableGeometryComponent>();
                    if (geo.Any()) {
                        entity.RemoveComponent(geo.First());
                    }

                    var newgeo = new HittableGeometryComponent();

                    BuildGeometry(generating.HeightMap, conf, newgeo);

                    entity.AddComponent(newgeo);

                    entity.GetComponent<IRenderableComponent>().CanRender = true;

                    break;
                }
            }
        }

        static void BuildGeometry(Vector3[] HeightMap, TerrainConfigurationComponent config, GeometryComponent geometry) {
            var width = config.Width;
            var height = config.Height;
            var count = (width - 1) * (height - 1) * 6;
            var indices = new int[count];
            var vertices = HeightMap;//.ToArray();
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
            geometry.TextureCoordinates = CalculateTextureCoordinates(HeightMap, width, height, config.TextureRepeat)
                .ToImmutableArray();
            geometry.Color = V4Colors.White;

            //Light.SetAmbientColor(0.05f, 0.05f, 0.05f, 1.0f);
            //Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        }
        static Vector2[] CalculateTextureCoordinates(Vector3[] HeightMap, int width, int height, int textureRepeat) {
            // Calculate how much to increment the texture coordinates by.
            float incrementValue = (float)textureRepeat / (float)width;

            // Calculate how many times to repeat the texture.
            int incrementCount = width / textureRepeat;

            // Initialize the tu and tv coordinate values.
            float tuCoordinate = 0.0f;
            float tvCoordinate = 0.0f;

            // Initialize the tu and tv coordinate indexes.
            int tuCount = 0;
            int tvCount = 0;
            var texture = new Vector2[HeightMap.Length];

            var tvIncrementValue = incrementValue;
            var tuIncrementValue = incrementValue;


            //for (int j = 0; j < height; j++) {
            //    for (int i = 0; i < width; i++) {
            //        texture[i + width * j] = new Vector2(tuCoordinate, tvCoordinate);
            //        tuCoordinate += tuIncrementValue;
            //    }
            //    tvCoordinate += tvIncrementValue;
            //    tuCoordinate = 0;
            //}
            //return texture;

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
    }

    public class TerrainConfigurationComponent : GraphicComponent, ITerrainComponent {
        public int Width { get; set; }
        public int Height { get; set; }

        public int TextureRepeat { get; set; }

        public double ElevationPower { get; set; }

        public TerrainConfigurationComponent() {
            Width = Height = 256;
            TextureRepeat = 8;// * 4
            IsModified = true;
        }

    }
    public class TerrainGeneratorComponent : GraphicComponent, ITerrainComponent {
        public struct TerrainParams {
            public double ElevationPower;
        }

        public bool IsGenerated { get; private set; }
        public Vector3[] HeightMap { get; private set; }

        readonly int width;
        readonly int height;
        readonly TerrainParams terrainParams;

        public TerrainGeneratorComponent(int width, int height, TerrainParams terrainParams) {
            this.width = width;
            this.height = height;
            this.terrainParams = terrainParams;
        }

        void Redistribution(ref float e) {
            //Redistribution (make mountain peaks steeper)
            //The noise function gives us values between 0 and 1 (or -1 and +1 depending on which library you’re using). 
            //To make flat valleys, we can raise the elevation to a power.
            //0.1 - 10
            e = (float)Math.Pow(e, terrainParams.ElevationPower);
        }
        static float ReScale(float e) {
            // Rescale from -1.0:+1.0 to 0.0:1.0
            return e / 2.0f + 0.5f;
        }
        static void ReScale(ref float e) {
            // Rescale from -1.0:+1.0 to 0.0:1.0
            e = e / 2.0f + 0.5f;
        }

        /// -1.0:+1.0 | 
        /// -0.2500f| 0.375
        /// 0       | 0.5
        /// 0.0625f | 0.53125
        /// 0.1250f | 0.5625
        /// 0.3750f | 0.6875
        /// 0.7500f | 0.875
        /// 

        void Biome(ref float e) {
            if (e < ReScale(-0.2500f)) {// deep water

            } else if (e < ReScale(0.0000f)) { //shallow water                           
            } else if (e < ReScale(0.0625f)) { //shore                           
                e = (float)Math.Round(e * 19) / 19f;
            } else if (e < ReScale(0.1250f)) {//sand    
                e = (float)Math.Round(e * 19) / 19f;
            } else if (e < ReScale(0.3750f)) { // grass                           
                //e = (float)Math.Round(e * 19) / 19f;
            } else if (e < ReScale(0.7500f)) { //dirt                           
            } else if (e < ReScale(1.0000f)) { //rock

            } else { //snow

            }

            Redistribution(ref e);

            //if (e < 0.1) return WATER;
            //else if (e < 0.2) return BEACH;
            //else if (e < 0.3) return FOREST;
            //else if (e < 0.5) return JUNGLE;
            //else if (e < 0.7) return SAVANNAH;
            //else if (e < 0.9) return DESERT;
            //else return SNOW;
        }

        /*
         * m - height map of moisture texture
         function biome(e, m) {      
  if (e < 0.1) return OCEAN;
  if (e < 0.12) return BEACH;
  
  if (e > 0.8) {
    if (m < 0.1) return SCORCHED;
    if (m < 0.2) return BARE;
    if (m < 0.5) return TUNDRA;
    return SNOW;
  }

  if (e > 0.6) {
    if (m < 0.33) return TEMPERATE_DESERT;
    if (m < 0.66) return SHRUBLAND;
    return TAIGA;
  }

  if (e > 0.3) {
    if (m < 0.16) return TEMPERATE_DESERT;
    if (m < 0.50) return GRASSLAND;
    if (m < 0.83) return TEMPERATE_DECIDUOUS_FOREST;
    return TEMPERATE_RAIN_FOREST;
  }

  if (m < 0.16) return SUBTROPICAL_DESERT;
  if (m < 0.33) return GRASSLAND;
  if (m < 0.66) return TROPICAL_SEASONAL_FOREST;
  return TROPICAL_RAIN_FOREST;
}
             */

        public Vector3[] GetHeightMap1() {
            var _params = NoiseParams.Terrain(0, width, height, NoiseGradients.Grayscale);
            var pModule = new SimplexPerlin();

            pModule.Quality = NoiseQuality.Best;
            pModule.Seed = _params.Seed;

            var fModule = new SumFractal();

            fModule.Frequency = _params.frequency;
            fModule.Lacunarity = _params.lacunarity;
            fModule.OctaveCount = _params.octaveCount;
            fModule.Offset = _params.offset;
            fModule.Gain = _params.gain;
            fModule.Primitive3D = pModule;



            var map = new Vector3[height * width];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    float nx = x;// / width - 0.5f;
                    var ny = y;// / height - 0.5f;
                    var h = pModule.GetValue(nx, ny);// fModule.GetValue(nx, ny);

                    map[height * y + x] = new Vector3(x, h * 10, -y);
                }
            }
            return map;



            //NoiseMapBuilder projection = null;
            //switch (_params.Map) {
            //    case NoiseMaps.Planar:
            //        float bound = 2f;
            //        projection = new NoiseMapBuilderPlane(bound, bound * 2, bound, bound * 2, _params.seamless);
            //        break;
            //}
            //var noiseMap = new NoiseMap();


            //projection.SetSize(_params.Size.Width, _params.Size.Height);
            //projection.SourceModule = finalModule;
            //projection.NoiseMap = noiseMap;

            //projection.Build();

        }

        void Generating() {
            var _params = NoiseParams.Terrain(0, width, height, NoiseGradients.Terrain);
            /*
            IModule finalTerrain;

            var simplexPerlin = new SimplexPerlin();
            simplexPerlin.Quality = NoiseQuality.Best;
            simplexPerlin.Seed = 0;

            var sumFractal = new SumFractal();
            sumFractal.Frequency = 1;
            sumFractal.Lacunarity = 3;
            sumFractal.OctaveCount = 6;
            sumFractal.Offset = 1;
            sumFractal.Gain = 2;
            sumFractal.SpectralExponent = 0.9f;// 0.1f;
            sumFractal.Primitive3D = simplexPerlin;

            
            var mountainTerrain = new RidgedMultiFractal();
            mountainTerrain.Frequency = 1;
            mountainTerrain.Lacunarity = 2;
            mountainTerrain.OctaveCount = 6;
            mountainTerrain.Offset = 1;
            mountainTerrain.Gain = 2f;

            var baseFlatTerrain = new Billow();
            baseFlatTerrain.Bias = -0.2f;
            baseFlatTerrain.Scale = 2f;

           
            
            //baseFlatTerrain.Frequency = _params.frequency;
            //baseFlatTerrain.Lacunarity = _params.lacunarity;
            //baseFlatTerrain.OctaveCount = _params.octaveCount;
            //baseFlatTerrain.Offset = _params.offset;
            //baseFlatTerrain.Gain = _params.gain;
            //baseFlatTerrain.SpectralExponent = 0.9f;

            // baseFlatTerrain.Primitive3D = simplPerlin;
            //mountainTerrain.Primitive3D = simplPerlin;
            sumFractal.Primitive3D = simplPerlin;
            

            finalTerrain = sumFractal;

            float bound = 2f;
            var projection = new NoiseMapBuilderPlane(bound, bound * 2, bound, bound * 2, _params.seamless);
            var noiseMap = new NoiseMap();
            projection.SetSize(_params.Size.Width, _params.Size.Height);
            projection.SourceModule = finalTerrain;// fModule;
            projection.NoiseMap = noiseMap;

            projection.Build();
            */

            var flat = BuildHeightMap(
                new SimplexPerlin(), 
                new Billow() {
                    Bias = -0.2f, Scale = 2f,
                    Frequency = 2,
                    Lacunarity = 1,
                    OctaveCount = 3,//6
                    Offset = 1,
                    Gain = 1,
                    SpectralExponent = 0.9f
                }, 0.025f);

            var mountain = BuildHeightMap(
                new SimplexPerlin(),
                new RidgedMultiFractal() {
                    Frequency = 0.4f,
                    Lacunarity = 3,
                    Offset = 1,
                    Gain = 1

                    //moutance around 
                    //Frequency = 0.4f,
                    //Lacunarity = 2,
                    //Offset = 1,
                    //Gain = 1.2
                    //seamless = false

                    //Frequency = 1,
                    //Lacunarity = 5,
                    //OctaveCount = 6,
                    //Offset = 1,
                    //Gain = 4,
                    //SpectralExponent = 0.9f
                },
                0.3f,
                new ScaleBias { Scale = 1.5f, Bias = -1.25f });

            //var mountain = BuildHeightMap(
            //   new ImprovedPerlin(),
            //   new SumFractal() {
            //       Frequency = 2,
            //       Lacunarity = 1,
            //       OctaveCount = 6,
            //   }, 0.1f);

            var resources = Path.Combine("../../../../D3DLab.Wpf.Engine.App/Resources/terrain/");
           // using (var bitmap = new System.Drawing.Bitmap(Path.Combine(resources, "mask_circular_256x256.png"))) {
                HeightMap = new Vector3[height * width];
                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        //var e =  noiseMap.GetValue(x, y);
                        //ReScale(ref e);
                        //var r = (float)bitmap.GetPixel(x, y).R /255;

                        //MakeIsland(ref e, x, y, 200f);
                        var index = height * y + x;

                        var mo = mountain[index];
                    if(mo < 0) {
                        mo = 0;
                    }
                       // Redistribution(ref mo);

                        HeightMap[index] = new Vector3(x, mo, -y);
                    }
                }
            //}
            IsGenerated = true;
        }


       float[] BuildHeightMap(PrimitiveModule primitive, FilterModule filter, float correction, ScaleBias scale = null) {
            //var simplexPerlin = new SimplexPerlin();
            primitive.Quality = NoiseQuality.Best;
            primitive.Seed = 0;
            //
            filter.Primitive3D = (IModule3D)primitive;

            IModule module;
            if (scale != null) {
                scale.SourceModule = filter;
                module = scale;
            } else {
                module = filter;
            }
            
            float bound = 2f;
            var projection = new NoiseMapBuilderPlane(bound, bound * 2, bound, bound * 2, true);
            var noiseMap = new NoiseMap();
            projection.SetSize(height, width);
            projection.SourceModule = module;
            projection.NoiseMap = noiseMap;

            projection.Build();

            var renderer = new LibNoise.Renderer.ImageRenderer();
            renderer.NoiseMap = noiseMap;
            renderer.Gradient = LibNoise.Renderer.GradientColors.Grayscale;
            renderer.LightBrightness = 2;
            renderer.LightContrast = 8;
            //renderer.LightEnabled = true;

            var bmpAdaptater = new BitmapAdaptater(width, height);
            renderer.Image = bmpAdaptater;

            renderer.Render();
            var btm = bmpAdaptater.Bitmap;
            //btm.Save(@"C:\Storage\projects\sv\3d\d3dlab\D3DLab.Wpf.Engine.App\Resources\terrain\flat.bmp");
            var map = new float[height * width];
            using (btm) {
                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        var pixel = btm.GetPixel(x, y);
                        var e = pixel.R;

                        if(e > 0) {

                        }

                        map[height * y + x] = e * correction;
                    }
                }
            }
            return map;
        }

        void MakeIsland(ref float elevation, float x, float y, float island_size) {
            float distance_x = (float)Math.Abs(x - island_size * 0.5f);
            float distance_y = (float)Math.Abs(y - island_size * 0.5f);
            float distance =(float) Math.Sqrt(distance_x * distance_x + distance_y * distance_y); // circular mask

            float max_width = island_size * 0.5f - 10.0f;
            float delta = distance / max_width;
            float gradient = delta * delta;
            var val = 1.0f - delta;
            if (val < 0) {
            } else if (val > 1) {
                val = 1;
            }
            elevation *= val;// Math.Max(0.0f, 1.0f - gradient);
        }

        void Generating1() {

            var _params = NoiseParams.Terrain(0, width, height, NoiseGradients.Terrain);
            var simplPerlin = new SimplexPerlin();

            simplPerlin.Quality = NoiseQuality.Best;
            simplPerlin.Seed = _params.Seed;

            var improvedPerlin = new ImprovedPerlin();
            improvedPerlin.Quality = NoiseQuality.Best;
            improvedPerlin.Seed = _params.Seed;


            var sumFractal = new SumFractal();

            sumFractal.Frequency = _params.frequency;
            sumFractal.Lacunarity = _params.lacunarity;
            sumFractal.OctaveCount = _params.octaveCount;
            sumFractal.Offset = _params.offset;
            sumFractal.Gain = _params.gain;
            sumFractal.Primitive3D = simplPerlin;
            sumFractal.SpectralExponent = 0.9f;

            float bound = 2f;

            var mountainTerrain = new RidgedMultiFractal();
            //mountainTerrain.Frequency = _params.frequency;
            //mountainTerrain.Lacunarity = _params.lacunarity;
            //mountainTerrain.OctaveCount = _params.octaveCount;
            //mountainTerrain.Offset = _params.offset;
            //mountainTerrain.Gain = _params.gain;
            //mountainTerrain.SpectralExponent = 0.9f;
            mountainTerrain.Primitive3D = simplPerlin;

            var baseFlatTerrain = new Billow();
            baseFlatTerrain.Frequency = 2.0f;
            baseFlatTerrain.Lacunarity = _params.lacunarity;
            baseFlatTerrain.OctaveCount = _params.octaveCount;
            baseFlatTerrain.Offset = _params.offset;
            baseFlatTerrain.Gain = _params.gain;
            baseFlatTerrain.Primitive3D = simplPerlin;
            baseFlatTerrain.SpectralExponent = 0.9f;

            var flatTerrain = new ScaleBias(baseFlatTerrain, 0.125f, -0.75f);

            var terrainSelector = new Select();
            terrainSelector.LeftModule = mountainTerrain;
            terrainSelector.RightModule = flatTerrain;
            terrainSelector.ControlModule = sumFractal;
            terrainSelector.SetBounds(0.0f, 1000.0f);
            terrainSelector.EdgeFalloff = 0.125f;

            var finalTerrain = new Turbulence();
            finalTerrain.SourceModule = mountainTerrain;
            finalTerrain.Power = 0.125f;
            finalTerrain.XDistortModule = baseFlatTerrain;
            finalTerrain.YDistortModule = baseFlatTerrain;
            finalTerrain.ZDistortModule = baseFlatTerrain;



            var projection = new NoiseMapBuilderPlane(6.0f, 10.0f, 1.0f, 5.0f, _params.seamless);
            var noiseMap = new NoiseMap();
            projection.SetSize(_params.Size.Width, _params.Size.Height);
            projection.SourceModule = finalTerrain;// fModule;
            projection.NoiseMap = noiseMap;



            projection.Build();

            HeightMap = new Vector3[height * width];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    float nx = x;// / width - 0.5f;
                    var ny = y;// / height - 0.5f;
                    var e = noiseMap.GetValue(x, y);// fModule.GetValue(nx, ny);

                    e = ReScale(e);
                    //Biome(ref e);

                    //if(h < 0) {
                    //    h =0;
                    //}

                    //Terraces
                    // e = (float)Math.Round(e * 19) / 19f;

                    HeightMap[height * y + x] = new Vector3(x, e * 30, -y);
                }
            }
            IsGenerated = true;
        }
        public Task StartGeneratingAsync() {
            return Task.Run(Generating);
        }
    }
}
