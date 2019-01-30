using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal interface IRenderStrategy {
        IRenderTechniquePass GetPass();
        void Render(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        void Cleanup();
    }

    internal abstract class RenderStrategy {

        protected readonly D3DShaderCompilator compilator;
        readonly IRenderTechniquePass pass;
        readonly VertexLayoutConstructor layoutConstructor;
        protected VertexShader vertexShader;
        protected PixelShader pixelShader;
        protected GeometryShader geometryShader;
        protected InputLayout layout;

        protected RenderStrategy(D3DShaderCompilator compilator, IRenderTechniquePass pass, VertexLayoutConstructor layoutConstructor) {
            this.compilator = compilator;
            this.pass = pass;
            this.layoutConstructor = layoutConstructor;
        }

        public void Render(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;
            var pass = GetPass();

            if (!pass.IsCompiled) {
                CompileShaders(graphics, pass, layoutConstructor);
            }

            graphics.ImmediateContext.InputAssembler.InputLayout = layout;

            //context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
            //context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

            context.VertexShader.Set(vertexShader);
            context.GeometryShader.Set(geometryShader);
            context.PixelShader.Set(pixelShader);

            Rendering(graphics, gameDataBuffer, lightDataBuffer);
        }

        public IRenderTechniquePass GetPass() => pass;

        protected abstract void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);

        protected void CompileShaders(GraphicsDevice graphics, IRenderTechniquePass pass, VertexLayoutConstructor layconst) {
            var device = graphics.D3DDevice;
            //if (!pass.IsCompiled) {
            pass.Compile(compilator);
            //}

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            layout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            vertexShader?.Dispose();
            pixelShader?.Dispose();

            vertexShader = new VertexShader(device, vertexShaderByteCode);

            if (pass.GeometryShader != null) {
                geometryShader = new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes());
            }
            if (pass.PixelShader != null) {
                pixelShader = new PixelShader(device, pass.PixelShader.ReadCompiledBytes());
            }


        }

    } 

    internal class ColoredVertexesRenderStrategy : RenderStrategy, IRenderStrategy {
        class RenderEntity {
            public D3DTriangleColoredVertexesRenderComponent Render;
            public IGeometryComponent Geometry;
            public D3DTransformComponent Transform;
            public ColorComponent Material;
        }

        
        readonly LinkedList<RenderEntity> entities;

        public ColoredVertexesRenderStrategy(D3DShaderCompilator compilator, IRenderTechniquePass pass, VertexLayoutConstructor layoutConstructor) :
            base(compilator, pass, layoutConstructor) {
            entities = new LinkedList<RenderEntity>();
        }

        public void RegisterEntity(Components.D3DTriangleColoredVertexesRenderComponent rcom, IGeometryComponent geocom, D3DTransformComponent tr, ColorComponent color) {
            entities.AddFirst(new RenderEntity { Render = rcom, Geometry = geocom, Transform = tr, Material = color });
        }

        protected override void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
            context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

            foreach (var en in entities) {
                var renderCom = en.Render;
                var geometryCom = en.Geometry;
                var trcom = en.Transform;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;
                
                if (geometryCom.IsModified || en.Material.IsModified) {
                    try {
                        var indexes = geometryCom.Indices.ToArray();
                        var vertices = new StategyStaticShaders.ColoredVertexes.VertexPositionColor[geometryCom.Positions.Length];
                        for (var index = 0; index < vertices.Length; index++) {
                            vertices[index] = new StategyStaticShaders.ColoredVertexes.VertexPositionColor(
                                geometryCom.Positions[index], geometryCom.Normals[index], en.Material.Color);
                        }
                        geometryCom.MarkAsRendered();

                        renderCom.VertexBuffer?.Dispose();
                        renderCom.IndexBuffer?.Dispose();

                        renderCom.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
                        renderCom.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, indexes);
                    } catch (Exception ex) {
                        ex.ToString();
                        throw ex;
                    }
                }

                if (trcom != null) {
                    var tr = new TransforStructBuffer(trcom.MatrixWorld);

                    //if (trcom.TransformBuffer == null) {
                    trcom.TransformBuffer?.Dispose();
                    trcom.TransformBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref tr);

                    //} else {
                     //  graphics.UpdateSubresource(ref tr, trcom.TransformBuffer, TransforStructBuffer.RegisterResourceSlot);
                    //}
                }

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, trcom.TransformBuffer);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer,
                    Unsafe.SizeOf<StategyStaticShaders.ColoredVertexes.VertexPositionColor>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());
                graphics.ImmediateContext.DrawIndexed(geometryCom.Indices.Length, 0, 0);
            }
        }

        public void Cleanup() {
            entities.Clear();
        }
    }
}
