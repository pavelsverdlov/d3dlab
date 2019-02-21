using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.Std.Engine.Core.Components;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Components {
    public class D3DLineVertexRenderComponent : D3DRenderComponent {

        public static D3DLineVertexRenderComponent AsLineStrip() {
            return new D3DLineVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.LineStrip
            };
        }
        public static D3DLineVertexRenderComponent AsLineList() {
            return new D3DLineVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.LineList
            };
        }

        public D3DLineVertexRenderComponent() : base() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = true
            });
            PrimitiveTopology = PrimitiveTopology.LineStrip;
            Pass = StategyStaticShaders.LineVertex.GetPasses();
            LayoutConstructor = StategyStaticShaders.LineVertex.GetLayoutConstructor();
        }


        internal override SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            var pos = geo.Positions;

            var vertices = new StategyStaticShaders.LineVertex.LineVertexColor[pos.Length];
            for (var i = 0; i < pos.Length; i++) {
                vertices[i] = new StategyStaticShaders.LineVertex.LineVertexColor(pos[i], geo.Colors[i]);
            }
            VertexSize = Unsafe.SizeOf<StategyStaticShaders.LineVertex.LineVertexColor>();

            return graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
        }

        internal override void Draw(GraphicsDevice graphics, IGeometryComponent geo) {
            UpdateGeometry(graphics, geo);
            graphics.ImmediateContext.Draw(geo.Positions.Length, 0);
        }

    }

}
