using D3DLab.ECS.Common;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.Toolkit.Techniques.TriangleColored {
    class D3DTriangleColoredVertexRenderComponent : D3DRenderComponent {

        public static D3DTriangleColoredVertexRenderComponent AsStrip() {
            return new D3DTriangleColoredVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
        }

        public static D3DTriangleColoredVertexRenderComponent AsTriangleListCullNone() {
            return new D3DTriangleColoredVertexRenderComponent(CullMode.None) {
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };
        }

        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> MaterialBuffer { get; }

        public D3DTriangleColoredVertexRenderComponent() : this(CullMode.Front) {

        }
        public D3DTriangleColoredVertexRenderComponent(CullMode cull) : base() {
            RasterizerStateDescription = new D3DRasterizerState(new RasterizerStateDescription2() {
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
            MaterialBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
        }

    }
}
