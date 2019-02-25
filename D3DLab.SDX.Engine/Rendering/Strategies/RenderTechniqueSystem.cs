using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Filter;
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
    public interface IRenderTechniqueSystem {
        IRenderTechniquePass GetPass();
        /// <summary>
        /// remove all entities from Technique
        /// </summary>
        void Cleanup();
        void Render(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        void RegisterEntity(GraphicEntity entity);
        bool IsAplicable(GraphicEntity entity);
    }
    

    public abstract class RenderTechniqueSystem {
        protected readonly LinkedList<GraphicEntity> entities;
        protected RasterizerStateDescription rasterizerStateDescription;
        protected BlendStateDescription blendStateDescription;
        protected DepthStencilStateDescription depthStencilStateDescription;

        readonly EntityHasSet entityHasSet;

        protected RenderTechniqueSystem(EntityHasSet entityHasSet) {
           // pass = new HashSet<IRenderTechniquePass>();
            entities = new LinkedList<GraphicEntity>();
            this.entityHasSet = entityHasSet;
        }

        public void Render(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            Rendering(graphics, gameDataBuffer, lightDataBuffer);
        }
        
        public bool IsAplicable(GraphicEntity entity) {
            return entityHasSet.HasComponents(entity);
        }


        protected abstract void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        
        public void RegisterEntity(GraphicEntity entity) {
            entities.AddLast(entity);
        }

        public void Cleanup() {
            entities.Clear();
        }

        protected void UpdateShaders(GraphicsDevice graphics, D3DRenderComponent render,
             D3DShaderTechniquePass pass, VertexLayoutConstructor layconst) {
            var device = graphics.D3DDevice;

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            render.layout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            render.vertexShader?.Dispose();
            render.pixelShader?.Dispose();
            render.geometryShader?.Dispose();

            render.vertexShader = new VertexShader(device, vertexShaderByteCode);

            if (pass.GeometryShader != null) {
                render.geometryShader = new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes());
            }
            if (pass.PixelShader != null) {
                render.pixelShader = new PixelShader(device, pass.PixelShader.ReadCompiledBytes());
            }

            render.RasterizerState = new D3DRasterizerState(rasterizerStateDescription);
            render.SetStates(
                new BlendState(graphics.D3DDevice, blendStateDescription),
                new DepthStencilState(graphics.D3DDevice, depthStencilStateDescription));
        }

        protected static void SetShaders(DeviceContext context, D3DRenderComponent render) {
            context.VertexShader.Set(render.vertexShader);
            context.GeometryShader.Set(render.geometryShader);
            context.PixelShader.Set(render.pixelShader);
        }
        protected static void Render(GraphicsDevice graphics, DeviceContext context, D3DRenderComponent render, int vertexSize) {
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(), vertexSize, 0));
            context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

            context.InputAssembler.InputLayout = render.layout;
            context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;
            graphics.UpdateRasterizerState(render.RasterizerState.GetDescription());

            context.OutputMerger.SetDepthStencilState(render.DepthStencilState, 0);
            var blendFactor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);
            context.OutputMerger.SetBlendState(render.BlendingState, blendFactor, -1);
        }

        protected ShaderResourceView[] ConvertToResources(TexturedMaterialComponent material, TexturedLoader loader) {
            var resources = new ShaderResourceView[material.Images.Length];
            for (var i = 0; i < material.Images.Length; i++) {
                var file = material.Images[i];
                resources[i] = loader.LoadShaderResource(file);
            }
            return resources;
        }
    }
}
