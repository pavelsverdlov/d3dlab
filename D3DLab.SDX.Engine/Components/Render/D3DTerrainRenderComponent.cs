using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.Std.Engine.Core.Components;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTerrainRenderComponent : D3DRenderComponent, ITerrainComponent {
        public int Width { get; set; }
        public int Heigth { get; set; }
        public D3DTerrainRenderComponent() : base() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            });
            PrimitiveTopology = PrimitiveTopology.TriangleList;

            Pass = StategyStaticShaders.Terrain.GetPasses();
            LayoutConstructor = StategyStaticShaders.Terrain.GetLayoutConstructor();
        }

        internal override SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            var pos = geo.Positions;
            var normals = geo.Normals;
            var vertices = new StategyStaticShaders.Terrain.TerrainVertex[pos.Length];

            for (var i = 0; i < pos.Length; i++) {
                vertices[i] = new StategyStaticShaders.Terrain.TerrainVertex {
                    position = pos[i],
                    normal = normals[i],
                    color = geo.Colors[i],
                    texcoor = geo.TextureCoordinates[i]
                };
            }
            VertexSize = StategyStaticShaders.Terrain.TerrainVertex.Size;
            return graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
        }

    }

}
