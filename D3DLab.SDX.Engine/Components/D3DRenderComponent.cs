using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;

namespace D3DLab.SDX.Engine.Components {
    public class D3DRenderComponent {
        public ElementTag Tag { get; set; }
        public ElementTag EntityTag { get; set; }

        public D3DRasterizerState RasterizerState { get; protected set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        public bool IsModified { get; set; }

        public SharpDX.Direct3D11.Buffer VertexBuffer { get; internal set; }
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; internal set; }

        public virtual void Dispose() {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

    }

    public class D3DColoredVertexesRenderComponent : D3DRenderComponent, ID3DRenderableComponent {

        public D3DColoredVertexesRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = true
            });

            PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }

    }

    public class D3DLineVertexRenderComponent : D3DRenderComponent, ID3DRenderableComponent {
        public D3DLineVertexRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = true,
                IsAntialiasedLineEnabled = true
            });
            PrimitiveTopology = PrimitiveTopology.LineList;
        }

        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
