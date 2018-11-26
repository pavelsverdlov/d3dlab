using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Numerics;

namespace D3DLab.SDX.Engine.Components {
    public class D3DRenderComponent {
        public bool CanRender { get; set; }
        public ElementTag Tag { get; set; }
        public ElementTag EntityTag { get; set; }

        public D3DRasterizerState RasterizerState { get; protected set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        public bool IsModified { get; set; }

        [IgnoreDebuging]
        public SharpDX.Direct3D11.Buffer VertexBuffer { get; internal set; }
        [IgnoreDebuging]
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; internal set; }

        public D3DRenderComponent() {
            CanRender = true;
        }

        public virtual void Dispose() {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

    }

    public class D3DTriangleColoredVertexesRenderComponent : D3DRenderComponent, ID3DRenderableComponent {

        public static D3DTriangleColoredVertexesRenderComponent AsStrip() {
            return new D3DTriangleColoredVertexesRenderComponent {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
        }

        public D3DTriangleColoredVertexesRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.Front,
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
        }

        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }

    }

    public class D3DLineVertexRenderComponent : D3DRenderComponent, ID3DRenderableComponent {

        public static D3DLineVertexRenderComponent AsLineStrip() {
            return new D3DLineVertexRenderComponent {
                PrimitiveTopology = PrimitiveTopology.LineStrip
            };
        }
        public static D3DLineVertexRenderComponent AsLineList() {
            return new D3DLineVertexRenderComponent {
                PrimitiveTopology = PrimitiveTopology.LineList
            };
        }

        public D3DLineVertexRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = true
            });
            PrimitiveTopology = PrimitiveTopology.LineStrip;
        }

        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class D3DSphereRenderComponent : D3DRenderComponent, ID3DRenderableComponent {
        public D3DSphereRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            });
            PrimitiveTopology = PrimitiveTopology.PointList;
        }
        
        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class D3DTerrainRenderComponent : D3DRenderComponent, ID3DRenderableComponent, ITerrainComponent {
        public int Width { get; set; }
        public int Heigth { get; set; }
     //   public Vector3[] HeightMap { get; set; }

        public D3DTerrainRenderComponent() {
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            });
            PrimitiveTopology = PrimitiveTopology.LineList;
        }

        void ID3DRenderableComponent.Accept(RenderFrameStrategiesVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class D3DShadersRenderComponent { //IShadersContainer

    }
}
