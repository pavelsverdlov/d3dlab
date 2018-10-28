using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Tests.Line {
    //https://github.com/RobyDX/SharpDX_Demo/blob/master/SharpDXTutorial/SharpHelper/SharpMesh.cs

    public static class LineEntityBuilder {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct LinesVertex {
            public Vector4 Position;
            public SharpDX.Color4 Color;
            public const int SizeInBytes = 4 * (4 + 4);
        }

        public static void Build(IEntityManager manager, Vector3[] points) {
            IShaderInfo[] lineShaders = {
                    new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Tests", "Line"),
                        $"line-{ShaderStages.Vertex}",
                        ShaderStages.Vertex.ToString(),
                        "VShaderLines"),
                        //new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line")}-{ShaderStages.Geometry}.hlsl",
                    //    Stage = ShaderStages.Geometry.ToString(), EntryPoint = "GShaderLines"},
                     new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Tests", "Line"),
                        $"line-{ShaderStages.Fragment}",
                        ShaderStages.Fragment.ToString(),
                        "PShaderLinesFade")
                };

            uint first = 0;
            var positions = new List<Vector3>();
            var lineListIndices = new List<uint>();
            positions.AddRange(points);
            var lineCount = positions.Count - first - 1;

            for (uint i = 0; i < lineCount; i++) {
                lineListIndices.Add(first + i);
                lineListIndices.Add(first + i + 1);
            }

            //lineListIndices.Add(positions.Count - 1);
            //lineListIndices.Add(first);

            var line = manager
                 .CreateEntity(new ElementTag("LineEntity"))
                 .AddComponent(new LineGeometryRenderComponent(lineShaders, points, lineListIndices.ToArray()));
        }

        public class LineGeometryRenderComponent : ID3DRenderableComponent {
            readonly IShaderInfo[] lineShaders;
            readonly Vector3[] vertices;
            readonly InputElement[] elements;
            bool init = false;
            readonly uint[] indices;
            readonly SharpDX.Direct3D11.Buffer triangleVertexBuffer;
            readonly ShaderTechniquePass pass;
            readonly uint[] indexes;

            public LineGeometryRenderComponent(IShaderInfo[] lineShaders, Vector3[] points, uint[] indexes) {
                this.lineShaders = lineShaders;
                pass = new ShaderTechniquePass(lineShaders);
                this.vertices = points;
                this.indexes = indexes;

                var list = new List<InputElement>();//InputClassification.PerVertexData 
                list.Add(new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0));
                list.Add(new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0));

                elements = list.ToArray();
            }

            public ElementTag Tag { get; set; }
            public ElementTag EntityTag { get; set; }

            public void Dispose() {

            }

            [StructLayout(LayoutKind.Sequential)]
            public struct VertexPositionColor {
                public readonly Vector3 Position;
                public readonly Vector4 Color;

                public VertexPositionColor(Vector3 position, Vector4 color) {
                    Position = position;
                    Color = color;
                }
            }


            void ID3DRenderableComponent.Update(RenderState state) {
                if (init) {
                    return;
                }


                #region shaders

                var vertexShaderText =
    @"
struct VSOut
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

VSOut main(float4 position : POSITION, float4 color : COLOR) { 

    VSOut output;
    output.position = position;
    output.color = color;

    return output;
}
";
                var pixelShaderText =
    @"
//SamplerState PointSampler
//{
//    Filter = MIN_MAG_MIP_POINT;
//    AddressU = Wrap;
//    AddressV = Wrap;
//};

SamplerState SurfaceSampler : register(s0);
Texture2D SurfaceTexture : register(t0);

float4 main(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET
{
    //texDiffuseMap.Sample(PointSampler, input.t);
    return color;
}
";
                #endregion

                var device = state.Graphics.Device;
                var context = state.Graphics.ImmediateContext;

                InputElement[] inputElements = new InputElement[]
            {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0)
            };

                // Triangle vertices
                VertexPositionColor[] vertices = new VertexPositionColor[] {
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), SharpDX.Color.Red.ToNumericV4()),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), SharpDX.Color.Green.ToNumericV4()),
            new VertexPositionColor(new Vector3(0.0f, -0.5f, 0.0f), SharpDX.Color.Blue.ToNumericV4()) };

                ShaderSignature inputSignature;
                VertexShader vertexShader;
                PixelShader pixelShader;

                // Compile the vertex shader code
                using (var vertexShaderByteCode = ShaderBytecode.Compile(vertexShaderText, "main", "vs_4_0", ShaderFlags.Debug)) {
                    // Read input signature from shader code
                    inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                    vertexShader = new VertexShader(device, vertexShaderByteCode);
                }

                // Compile the pixel shader code
                using (var pixelShaderByteCode = ShaderBytecode.Compile(pixelShaderText, "main", "ps_4_0", ShaderFlags.Debug)) {
                    pixelShader = new PixelShader(device, pixelShaderByteCode);
                }

                // Set as current vertex and pixel shaders
                context.VertexShader.Set(vertexShader);
                context.PixelShader.Set(pixelShader);

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                // Set input layout to use
                context.InputAssembler.InputLayout = new InputLayout(device, inputSignature, inputElements);

                var triangleVertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));

                init = true;
            }

            void ID3DRenderableComponent.Render(RenderState state) {
                state.Graphics.ImmediateContext.Draw(3, 0);
            }


        }

    }

}
