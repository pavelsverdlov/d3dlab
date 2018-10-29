using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal interface IRenderStrategy {
        void Update(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer, CameraState camera);
        void Cleanup();
    }

    internal abstract class RenderStrategy {
        protected readonly D3DShaderCompilator compilator;

        protected VertexShader vertexShader;
        protected PixelShader pixelShader;

        protected RenderStrategy(D3DShaderCompilator compilator) {
            this.compilator = compilator;
        }

        protected void CompileShaders(GraphicsDevice graphics, ShaderTechniquePass pass, VertexLayoutConstructor layconst) {
            var device = graphics.Device;
            pass.Compile(compilator);

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var inputLayout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            vertexShader?.Dispose();
            pixelShader?.Dispose();

            vertexShader = new VertexShader(device, vertexShaderByteCode);

            if (pass.GeometryShader != null) {
                pixelShader = new PixelShader(device, pass.GeometryShader.ReadCompiledBytes());
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
        const string pixelShaderText =
@"
float4 main(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET
{
    return color;
}
";
        #endregion

        static readonly ShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static ColoredVertexesRenderStrategy() {
            pass = new ShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("CV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
                new ShaderInMemoryInfo("CV_FragmentShader", pixelShaderText, null, ShaderStages.Fragment.ToString(), "main"),
            });
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();
        }

        Random ran;

        readonly List<Tuple<D3DColoredVertexesRenderComponent, IGeometryComponent>> entities;

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
            entities = new List<Tuple<D3DColoredVertexesRenderComponent, IGeometryComponent>>();
            ran = new Random(100);
        }

        public void RegisterEntity(Components.D3DColoredVertexesRenderComponent rcom, IGeometryComponent geocom) {
            entities.Add(Tuple.Create(rcom, geocom));
        }

        public void Update(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer, CameraState camera) {
            var device = graphics.Device;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                CompileShaders(graphics, pass, layconst);
            }

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geometryCom = en.Item2;

                if (geometryCom.IsModified) {
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
                }

                {
                    var v = ran.NextVector3(
                        new Vector3(-100, -100, -100),
                        new Vector3(100, 100, 100));
                    var matrix = //Matrix4x4.Identity;
                    Matrix4x4.CreateTranslation(v);

                    var gamebuff = new GameResourceBuffer(matrix, camera.ViewMatrix, camera.ProjectionMatrix);

                    graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameResourceBuffer.RegisterResourceSlot);
                }

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                context.VertexShader.SetConstantBuffer(GameResourceBuffer.RegisterResourceSlot, gameDataBuffer);
                context.VertexShader.SetConstantBuffer(LightStructLayout.RegisterResourceSlot, lightDataBuffer);

                context.VertexShader.Set(vertexShader);
                context.PixelShader.Set(pixelShader);

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());

                graphics.ImmediateContext.DrawIndexed(geometryCom.Indices.Count, 0, 0);
            }
        }


        public void Cleanup() {
            entities.Clear();
        }
    }

    internal class LineVertexRenderStrategy : RenderStrategy, IRenderStrategy {

        #region shaders

        const string vertexShaderText =
@"
struct InputFS {
	float4 position : SV_Position;
	float4 color : COLOR;
};
cbuffer Game : register(b0) {
	float4x4 World;
    float4x4 View;
    float4x4 Projection;
};
InputFS main(float4 position : POSITION, float4 color : COLOR){
    InputFS output;

    output.position = mul(World, position);
    output.position = mul(View, output.position);
    output.position = mul(Projection, output.position);
    output.color = color;

    return output;
}

";

        const string geometryShaderText =
@"
struct InputFS {
	float4 position : SV_Position;
	float4 color : COLOR;
};
[maxvertexcount(1)]
void main(point InputFS points[1], inout TriangleStream<InputFS> output) {
    
    InputFS fs = (InputFS)0;
    fs.position = points[0].position;
    fs.color = points[0].color;
    output.Append(fs);

    //fs.position = points[1].position;
    //fs.color = points[1].color;
    //output.Append(fs);

	//output.RestartStrip();
}
";

        const string pixelShaderText =
@"
float4 main(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET
{
    return color;
}
";


        [StructLayout(LayoutKind.Sequential)]
        public struct LineVertexColor {
            public readonly Vector3 Position;
            public readonly Vector4 Color;

            public LineVertexColor(Vector3 position, Vector4 color) {
                Position = position;
                Color = color;
            }
        }

        #endregion

        static readonly ShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static LineVertexRenderStrategy() {
            pass = new ShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("LV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
              //  new ShaderInMemoryInfo("LV_GeometryShader", geometryShaderText, null, ShaderStages.Geometry.ToString(), "main"),
                new ShaderInMemoryInfo("LV_FragmentShader", pixelShaderText, null, ShaderStages.Fragment.ToString(), "main"),
            });
            layconst = new VertexLayoutConstructor()
                .AddPositionElementAsVector3()
                .AddColorElementAsVector4();
        }

        readonly List<Tuple<D3DLineVertexRenderComponent, IGeometryComponent>> entities;

        public LineVertexRenderStrategy(D3DShaderCompilator compilator) : base(compilator) {
            entities = new List<Tuple<D3DLineVertexRenderComponent, IGeometryComponent>>();
        }

        public void RegisterEntity(Components.D3DLineVertexRenderComponent rcom, IGeometryComponent geocom) {
            entities.Add(Tuple.Create(rcom, geocom));
        }

        public void Cleanup() {
            entities.Clear();
        }

        public void Update(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer, CameraState camera) {
            var device = graphics.Device;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                CompileShaders(graphics, pass, layconst);
            }

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geometryCom = en.Item2;

                var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, camera.ViewMatrix, camera.ProjectionMatrix);
                graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameResourceBuffer.RegisterResourceSlot);

                int indexCount = 0;
                if (geometryCom.IsModified) {
                    var pos = geometryCom.Positions.ToArray();
                    var index = new List<int>();
                    var vertices = new LineVertexColor[pos.Length];
                    for (var i = 0; i < pos.Length; i++) {
                        vertices[i] = new LineVertexColor(pos[i], geometryCom.Colors[i]);
                        index.Add(i);
                    }

                    geometryCom.MarkAsRendered();

                    renderCom.VertexBuffer?.Dispose();
                    renderCom.IndexBuffer?.Dispose();

                    renderCom.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
                    renderCom.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, pos);
                    indexCount = vertices.Length;
                }

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

                context.VertexShader.SetConstantBuffer(GameResourceBuffer.RegisterResourceSlot, gameDataBuffer);
                context.VertexShader.SetConstantBuffer(LightStructLayout.RegisterResourceSlot, lightDataBuffer);

                context.VertexShader.Set(vertexShader);
                context.PixelShader.Set(pixelShader);

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());

                //graphics.ImmediateContext.DrawIndexed(indexCount, 0, 0);
                graphics.ImmediateContext.Draw(geometryCom.Positions.Count, 0);
            }
        }

    }
}
