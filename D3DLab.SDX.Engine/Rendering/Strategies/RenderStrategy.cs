using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal interface IRenderStrategy {
        IEnumerable<IRenderTechniquePass> GetPass();
        void Render(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        void Cleanup();
    }

    internal abstract class RenderStrategy {
        protected readonly LinkedList<GraphicEntity> entities;
        protected readonly HashSet<IRenderTechniquePass> pass;
        protected RenderStrategy() {
            pass = new HashSet<IRenderTechniquePass>();
            entities = new LinkedList<GraphicEntity>();
        }

        public void Render(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            Rendering(graphics, gameDataBuffer, lightDataBuffer);
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => pass;

        protected abstract void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);


        #region updates 


        protected static void UpdateGeometry<T>(GraphicsDevice graphics,
            D3DRenderComponent render, IGeometryComponent geo, T[] vertices) where T : struct {
            geo.MarkAsRendered();

            render.VertexBuffer?.Dispose();
            render.IndexBuffer?.Dispose();

            render.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
            render.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray());
        }
        protected static void UpdateTransformWorld(GraphicsDevice graphics,
           D3DRenderComponent render, TransformComponent trcom) {
            render.TransformWorldBuffer?.Dispose();
            var tr = new TransforStructBuffer(Matrix4x4.Identity);
            render.TransformWorldBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref tr);
        }

        protected static void UpdateShaders(GraphicsDevice graphics, DeviceContext context, D3DRenderComponent renderCom) {
            context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;
            context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, renderCom.TransformWorldBuffer);

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, renderCom.VertexSize, 0));
            context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);

            graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());
        }
        protected static void UpdaeTextureToPixelShaders(DeviceContext context, D3DTexturedMaterialComponent renderCom) {
            context.PixelShader.SetShaderResources(0, renderCom.TextureResources);
            context.PixelShader.SetSampler(0, renderCom.SampleState);
        }

        protected void UpdateComponent(ID3DRenderable updatable, GraphicsDevice graphics) {
            updatable.Update(graphics);
        }

        #endregion

        internal void RegisterEntity(GraphicEntity entity) {
            entities.AddFirst(entity);
        }

        public void Cleanup() {
            entities.Clear();
        }
    }
}
