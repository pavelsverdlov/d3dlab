using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace D3DLab.SDX.Engine.Components {
    public class D3DLineVertexRenderComponent : D3DRenderComponent, ID3DRenderableComponent {

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

        LineVertexColor[] vertices;
        SharpDX.Direct3D11.Buffer gameDataBuffer;

        public D3DLineVertexRenderComponent() {
            RasterizerState = new RasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = true
            });
            compilator = new D3DShaderCompilator();
            Pass = new ShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("LV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
                new ShaderInMemoryInfo("LV_GeometryShader", geometryShaderText, null, ShaderStages.Geometry.ToString(), "main"),
                new ShaderInMemoryInfo("LV_FragmentShader", pixelShaderText, null, ShaderStages.Fragment.ToString(), "main"),
            });

            compilator.Compile(Pass.VertexShader);
            compilator.Compile(Pass.GeometryShader);
            compilator.Compile(Pass.PixelShader);
        }

        public void Dispose() {
            
        }

        void ID3DRenderableComponent.Render(RenderState state) {
            var camera = state.Camera;

            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, camera.ViewMatrix, camera.ProjectionMatrix);
            state.Graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameResourceBuffer.RegisterResourceSlot);

            state.Graphics.UpdateRasterizerState(RasterizerState.GetDescription());
            state.Graphics.ImmediateContext.Draw(vertices.Length, 0);
        }

        void ID3DRenderableComponent.Update(RenderState state) {
            if (initialized) { return; }

            var camera = state.Camera;
            var device = state.Graphics.Device;
            var context = state.Graphics.ImmediateContext;

            var geometry = state.ContextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(EntityTag);

            var pos = geometry.Positions.ToArray();

            vertices = new LineVertexColor[pos.Length];
            for (var i = 0; i < pos.Length; i++) {
                vertices[i] = new LineVertexColor(pos[i], geometry.Colors[i]);
            }

            var layconst = new VertexLayoutConstructor()
                .AddPositionElementAsVector3()
                .AddColorElementAsVector4();

            var vertexShaderByteCode = Pass.VertexShader.ReadCompiledBytes();

            var triangleVertexBuffer = state.Graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
            var indexBuffer = state.Graphics.CreateBuffer(BindFlags.IndexBuffer, pos);

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var vertexShader = new VertexShader(device, vertexShaderByteCode);
            var pixelShader = new PixelShader(device, Pass.PixelShader.ReadCompiledBytes());
            var geometryShader = new GeometryShader(device, Pass.GeometryShader.ReadCompiledBytes());

            context.VertexShader.Set(vertexShader);
            context.GeometryShader.Set(geometryShader);
            context.PixelShader.Set(pixelShader);
            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, camera.ViewMatrix, camera.ProjectionMatrix);

            gameDataBuffer = state.Graphics.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);
            context.VertexShader.SetConstantBuffer(GameResourceBuffer.RegisterResourceSlot, gameDataBuffer);
            //context.VertexShader.SetConstantBuffer(LightStructLayout.RegisterResourceSlot, lightDataBuffer);

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            context.InputAssembler.InputLayout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_SInt, 0);

            initialized = true;
        }
    }
}
