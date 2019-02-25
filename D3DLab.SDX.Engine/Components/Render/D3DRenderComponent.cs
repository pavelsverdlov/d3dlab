using D3DLab.SDX.Engine.Rendering;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.SDX.Engine.Components {

    public abstract class D3DRenderComponent : GraphicComponent, ID3DRenderable, IRenderableComponent {
        public bool CanRender { get; set; }

        public D3DRasterizerState RasterizerState { get; set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        [IgnoreDebuging]
        internal SharpDX.Direct3D11.Buffer TransformWorldBuffer { get; set; }
        [IgnoreDebuging]
        internal DisposableSetter<SharpDX.Direct3D11.Buffer> VertexBuffer { get;  }
        [IgnoreDebuging]
        internal DisposableSetter<SharpDX.Direct3D11.Buffer> IndexBuffer { get; }

        public IRenderTechniquePass Pass { get; set; }
        public VertexLayoutConstructor LayoutConstructor { get; set; }
        public DepthStencilState DepthStencilState { get; private set; }
        public BlendState BlendingState { get; private set; }


        public InputLayout layout;
        public VertexShader vertexShader;
        public PixelShader pixelShader;
        public GeometryShader geometryShader;

        public int VertexSize { get; set; }

        public D3DRenderComponent() {
            CanRender = true;
            IsModified = true;
            VertexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>();
            IndexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>();
        }

        public override void Dispose() {
            Disposer.DisposeAll(
                VertexBuffer,
                IndexBuffer,
                TransformWorldBuffer,
                layout,
                vertexShader,
                pixelShader,
                geometryShader,
                DepthStencilState);

            base.Dispose();
            CanRender = false;
        }


        public void SetStates(BlendState blend, DepthStencilState depth) {
            DepthStencilState?.Dispose();
            BlendingState?.Dispose();

            DepthStencilState = depth;
            BlendingState = blend;
        }

        void ID3DRenderable.Update(GraphicsDevice graphics) {
        }
        void ID3DRenderable.Render(GraphicsDevice graphics) {
        }

        internal virtual void Draw(GraphicsDevice graphics, IGeometryComponent geo) {
        }

        internal void UpdateGeometry(GraphicsDevice graphics, IGeometryComponent geo) {
            //var context = graphics.ImmediateContext;
            //if (geo.IsModified) {
            //    VertexBuffer = GetVertexBuffer(graphics, geo);
            //    IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray());

            //    geo.MarkAsRendered();
            //}
            //context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            //context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
        }

        //TODO: should be abstract
        internal virtual SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            return null;
        }

    }

}
