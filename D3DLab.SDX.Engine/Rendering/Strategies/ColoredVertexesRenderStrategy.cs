using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal interface IRenderStrategy {
        IRenderTechniquePass GetPass();
        void Render(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        void Cleanup();
    }

    internal abstract class RenderStrategy {

        #region default shaders

        protected const string pixelShaderOnlyColor =
@"
float4 main(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET
{
    return color;
}
";

        #endregion

        protected readonly D3DShaderCompilator compilator;

        protected VertexShader vertexShader;
        protected PixelShader pixelShader;
        protected GeometryShader geometryShader;

        protected RenderStrategy(D3DShaderCompilator compilator) {
            this.compilator = compilator;
        }

        public void Render(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.Device;
            var context = graphics.ImmediateContext;
            var pass = GetPass();

            //if (!pass.IsCompiled) {
                CompileShaders(graphics, pass, GetLayoutConstructor());
//            }

            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
            context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

            if (vertexShader != null) {
                context.VertexShader.Set(vertexShader);
            }
            if (geometryShader != null) {
                context.GeometryShader.Set(geometryShader);
            }
            if (pixelShader != null) {
                context.PixelShader.Set(pixelShader);
            }

            Rendering(graphics);
        }

        public abstract IRenderTechniquePass GetPass();
        protected abstract VertexLayoutConstructor GetLayoutConstructor();
        protected abstract void Rendering(GraphicsDevice graphics);

        protected void CompileShaders(GraphicsDevice graphics, IRenderTechniquePass pass, VertexLayoutConstructor layconst) {
            var device = graphics.Device;
            if (!pass.IsCompiled) {
                pass.Compile(compilator);
            }

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var inputLayout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            vertexShader?.Dispose();
            pixelShader?.Dispose();

            vertexShader = new VertexShader(device, vertexShaderByteCode);

            if (pass.GeometryShader != null) {
                geometryShader = new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes());
            }
            if (pass.PixelShader != null) {
                pixelShader = new PixelShader(device, pass.PixelShader.ReadCompiledBytes());
            }

            graphics.ImmediateContext.InputAssembler.InputLayout = inputLayout;
        }

    }

    internal class ColoredVertexesRenderStrategy : RenderStrategy, IRenderStrategy {

        #region shaders

        const string vertexShaderText =
@"
#include ""Game""
#include ""Light""

struct VSOut
{
    float4 position : SV_POSITION;
    //float4 normal : NORMAL;
    float4 color : COLOR;
};
VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR) { 
    VSOut output = (VSOut)0;
    
    output.position = mul(World, position);
    output.position = mul(View, output.position);
    output.position = mul(Projection, output.position);

    output.color = color * computeLight(position.xyz, normal);

    return output;
}
";
          #endregion

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static ColoredVertexesRenderStrategy() {
            pass = new D3DShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("CV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
                new ShaderInMemoryInfo("CV_FragmentShader", pixelShaderOnlyColor, null, ShaderStages.Fragment.ToString(), "main"),
            });
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();
        }

        readonly List<Tuple<D3DTriangleColoredVertexesRenderComponent, IGeometryComponent, D3DTransformComponent>> entities;

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexPositionColor {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector4 Color;

            public VertexPositionColor(Vector3 position, Vector3 normal, Vector4 color) {
                Position = position;
                Normal = normal;
                Color = color;
            }
        }

        public ColoredVertexesRenderStrategy(D3DShaderCompilator compilator) : base(compilator) {
            entities = new List<Tuple<D3DTriangleColoredVertexesRenderComponent, IGeometryComponent, D3DTransformComponent>>();            
        }

        public override IRenderTechniquePass GetPass() => pass;
        protected override VertexLayoutConstructor GetLayoutConstructor() => layconst;

        public void RegisterEntity(Components.D3DTriangleColoredVertexesRenderComponent rcom, IGeometryComponent geocom, D3DTransformComponent tr) {
            entities.Add(Tuple.Create(rcom, geocom, tr));
        }

        protected override void Rendering(GraphicsDevice graphics) {
            var device = graphics.Device;
            var context = graphics.ImmediateContext;

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geometryCom = en.Item2;
                var trcom = en.Item3;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;

                if (geometryCom.IsModified) {
                    try {
                        var indexes = geometryCom.Indices.ToArray();
                        VertexPositionColor[] vertices = new VertexPositionColor[geometryCom.Positions.Count];
                        for (var index = 0; index < vertices.Length; index++) {
                            vertices[index] = new VertexPositionColor(geometryCom.Positions[index], geometryCom.Normals[index], geometryCom.Colors[index]);
                        }
                        geometryCom.MarkAsRendered();

                        renderCom.VertexBuffer?.Dispose();
                        renderCom.IndexBuffer?.Dispose();

                        renderCom.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
                        renderCom.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, indexes);
                    }catch(Exception ex) {
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
                    //    graphics.UpdateSubresource(ref tr, trcom.TransformBuffer, TransforStructBuffer.RegisterResourceSlot);
                    //}
                }

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, trcom.TransformBuffer);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer,
                    Unsafe.SizeOf<VertexPositionColor>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());
                graphics.ImmediateContext.DrawIndexed(geometryCom.Indices.Count, 0, 0);
            }
        }

        public void Cleanup() {
            entities.Clear();
        }
    }
}
