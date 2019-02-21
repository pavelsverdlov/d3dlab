using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.Std.Engine.Core.Components;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTriangleColoredVertexesRenderComponent : D3DRenderComponent {

        public static D3DTriangleColoredVertexesRenderComponent AsStrip() {
            return new D3DTriangleColoredVertexesRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
        }

        public static D3DTriangleColoredVertexesRenderComponent AsTriangleListCullNone() {
            return new D3DTriangleColoredVertexesRenderComponent(CullMode.None) {
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };
        }
        public D3DTriangleColoredVertexesRenderComponent() : this(CullMode.Front) {

        }
        public D3DTriangleColoredVertexesRenderComponent(CullMode cull) : base() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = cull,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,

                IsFrontCounterClockwise = false,
                IsScissorEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                SlopeScaledDepthBias = .0f
            });

            PrimitiveTopology = PrimitiveTopology.TriangleList;

            Pass = StategyStaticShaders.ColoredVertexes.GetPasses();
            LayoutConstructor = StategyStaticShaders.ColoredVertexes.GetLayoutConstructor();
        }



        internal override SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            var vertices = new StategyStaticShaders.ColoredVertexes.VertexPositionColor[geo.Positions.Length];
            for (var index = 0; index < vertices.Length; index++) {
                vertices[index] = new StategyStaticShaders.ColoredVertexes.VertexPositionColor(
                    geo.Positions[index], geo.Normals[index], geo.Colors[index]);
            }
            VertexSize = Unsafe.SizeOf<StategyStaticShaders.ColoredVertexes.VertexPositionColor>();
            return graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
        }

    }

}
