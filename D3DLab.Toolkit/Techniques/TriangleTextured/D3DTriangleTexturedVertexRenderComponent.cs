using D3DLab.ECS.Common;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Toolkit.Techniques.TriangleColored;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.Toolkit.Techniques.TriangleTextured {
    class D3DTriangleTexturedVertexRenderComponent : D3DTriangleColoredVertexRenderComponent {

        [IgnoreDebuging]
        internal EnumerableDisposableSetter<ShaderResourceView[]> TextureResources { get; set; }
        [IgnoreDebuging]
        internal DisposableSetter<SamplerState> SampleState { get; set; }

        public D3DTriangleTexturedVertexRenderComponent() : this(CullMode.Front) {
        }
        public D3DTriangleTexturedVertexRenderComponent(CullMode cull) : base() {
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
            SampleState = new DisposableSetter<SamplerState>(disposer);
            TextureResources = new EnumerableDisposableSetter<ShaderResourceView[]>(disposer);
        }

    }
}
