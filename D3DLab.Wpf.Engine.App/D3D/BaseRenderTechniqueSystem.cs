using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Wpf.Engine.App.D3D {
    [Obsolete("Old version")]
    public abstract class NestedRenderTechniqueSystem : D3DAbstractRenderTechnique<CustomRenderProperties> {
        protected NestedRenderTechniqueSystem(EntityHasSet entityHasSet) : base(entityHasSet) {
        }

        protected static void Render(GraphicsDevice graphics, DeviceContext context, D3DRenderComponent render, int vertexSize) {

            if (render.TransformWorldBuffer.HasValue) {
                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());
            }

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(), vertexSize, 0));
            context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

            context.InputAssembler.InputLayout = render.Layout.Get();
            context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;
            graphics.UpdateRasterizerState(render.RasterizerStateDescription.GetDescription());

            context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
            var blendFactor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);
            context.OutputMerger.SetBlendState(render.BlendingState.Get(), blendFactor, -1);
        }

        protected void UpdateTransformWorld(GraphicsDevice graphics, ID3DTransformWorldRenderComponent render, TransformComponent transform) {
            if (transform.IsModified) {
                var tr = TransforStructBuffer.ToTranspose(transform.MatrixWorld);

                if (render.TransformWorldBuffer.HasValue) {
                    var buff = render.TransformWorldBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref tr, buff);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
                    render.TransformWorldBuffer.Set(buff);
                }

                transform.IsModified = false;
            }
        }

        protected void UpdateShaders(GraphicsDevice graphics, D3DRenderComponent render,
            D3DShaderTechniquePass pass, VertexLayoutConstructor layconst) {
            var device = graphics.D3DDevice;

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

            //var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            //render.Layout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));

            //render.VertexShader.Set(new VertexShader(device, vertexShaderByteCode));

            //if (pass.GeometryShader != null) {
            //    render.GeometryShader.Set(new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes()));
            //}
            //if (pass.PixelShader != null) {
            //    render.PixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            //}
            throw new NotImplementedException();
        }

        protected static void SetShaders(DeviceContext context, D3DRenderComponent render) {
            //context.VertexShader.Set(render.VertexShader.Get());
            //context.GeometryShader.Set(render.GeometryShader.Get());
            //context.PixelShader.Set(render.PixelShader.Get());
            throw new NotImplementedException();
        }
    }
}
