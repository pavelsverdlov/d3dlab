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

        public D3DRasterizerState RasterizerState { get; protected set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        [IgnoreDebuging]
        internal SharpDX.Direct3D11.Buffer TransformWorldBuffer { get; set; }

        #region geo
        [IgnoreDebuging]
        internal SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        [IgnoreDebuging]
        internal SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }

        public int VertexSize { get; set; }

        #endregion

        public IRenderTechniquePass Pass { get; set; }
        public VertexLayoutConstructor LayoutConstructor { get; set; }

        InputLayout layout;
        VertexShader vertexShader;
        PixelShader pixelShader;
        GeometryShader geometryShader;

        public D3DRenderComponent() {
            CanRender = true;
            IsModified = true;
        }

        public override void Dispose() {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            TransformWorldBuffer?.Dispose();
            layout?.Dispose();
            vertexShader?.Dispose();
            pixelShader?.Dispose();
            geometryShader?.Dispose();

            base.Dispose();
            CanRender = false;
        }

        void ID3DRenderable.Update(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;

            if (!Pass.IsCompiled) {
                Pass.Compile(graphics.Compilator);
            }

            InitializeShaders(graphics);

            IsModified = false;
        }

        void ID3DRenderable.Render(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;

            context.VertexShader.Set(vertexShader);
            context.GeometryShader.Set(geometryShader);
            context.PixelShader.Set(pixelShader);

            graphics.ImmediateContext.InputAssembler.InputLayout = layout;
            graphics.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology;
            graphics.UpdateRasterizerState(RasterizerState.GetDescription());
        }

        internal virtual void Draw(GraphicsDevice graphics, IGeometryComponent geo) {
            UpdateGeometry(graphics, geo);

            graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
        }

        internal void UpdateGeometry(GraphicsDevice graphics, IGeometryComponent geo) {
            var context = graphics.ImmediateContext;
            if (geo.IsModified) {
                VertexBuffer?.Dispose();
                IndexBuffer?.Dispose();

                VertexBuffer = GetVertexBuffer(graphics, geo);
                IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray());

                geo.MarkAsRendered();
            }
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
        }

        //TODO: should be abstract
        internal virtual SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            return null;
        }

        void InitializeShaders(GraphicsDevice graphics) {
            var device = graphics.D3DDevice;

            var vertexShaderByteCode = Pass.VertexShader.ReadCompiledBytes();

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            layout = new InputLayout(device, inputSignature, LayoutConstructor.ConstuctElements());

            vertexShader?.Dispose();
            pixelShader?.Dispose();
            geometryShader?.Dispose();

            vertexShader = new VertexShader(device, vertexShaderByteCode);

            if (Pass.GeometryShader != null) {
                geometryShader = new GeometryShader(device, Pass.GeometryShader.ReadCompiledBytes());
            }
            if (Pass.PixelShader != null) {
                pixelShader = new PixelShader(device, Pass.PixelShader.ReadCompiledBytes());
            }
        }
    }

}
