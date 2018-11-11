using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal class LineVertexRenderStrategy : RenderStrategy, IRenderStrategy {

        #region shaders

        const string vertexShaderText =
@"
#include ""Game""

struct InputFS {
	float4 position : SV_Position;
	float4 color : COLOR;
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
float THICKNESS = 10;
float2 WIN_SCALE = float2(300, 600);

struct InputFS {
	float4 position : SV_Position;
	float4 color : COLOR;
};


float2 screen_space(float4 vertex) {
    return float2( vertex.xy / vertex.w ) * WIN_SCALE;
}

[maxvertexcount(2)]
void main(line InputFS points[2], inout LineStream<InputFS> output) {
    
    InputFS fs = (InputFS)0;
    fs.position = points[0].position;
    fs.color = points[0].color;
    output.Append(fs);

    fs = (InputFS)0;
    fs.position = points[1].position;
    fs.color = points[1].color;
    output.Append(fs);

	output.RestartStrip();
}
";
        protected const string pixelShaderText =
@"
float4 main(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET {
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

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static LineVertexRenderStrategy() {
            pass = new D3DShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("LV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
               // new ShaderInMemoryInfo("LV_GeometryShader", geometryShaderText, null, ShaderStages.Geometry.ToString(), "main"),
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
        public override IRenderTechniquePass GetPass() => pass;
        protected override VertexLayoutConstructor GetLayoutConstructor() => layconst;

        public void RegisterEntity(Components.D3DLineVertexRenderComponent rcom, IGeometryComponent geocom) {
            entities.Add(Tuple.Create(rcom, geocom));
        }

        public void Cleanup() {
            entities.Clear();
        }

        protected override void Rendering(GraphicsDevice graphics) {
            var device = graphics.Device;
            var context = graphics.ImmediateContext;

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geometryCom = en.Item2;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;

                int indexCount = 0;
                if (geometryCom.IsModified) {
                    var pos = geometryCom.Positions.ToArray();
                    var index = new List<int>();
                    var vertices = new LineVertexColor[pos.Length];
                    for (var i = 0; i < pos.Length; i++) {
                        vertices[i] = new LineVertexColor(pos[i], geometryCom.Colors[i]);
                        index.Add(i);
                        index.Add(i);
                    }
                    
                    geometryCom.Indices.Clear();
                    geometryCom.Indices.AddRange(index);

                    geometryCom.MarkAsRendered();

                    renderCom.VertexBuffer?.Dispose();
                    renderCom.IndexBuffer?.Dispose();

                    renderCom.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
                    renderCom.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, index.ToArray());
                    indexCount = vertices.Length;
                }

                var tr = new TransforStructBuffer(Matrix4x4.Identity);
                var TransformBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref tr);

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, TransformBuffer);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt


                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());

                //graphics.ImmediateContext.DrawIndexed(indexCount, 0, 0);
                graphics.ImmediateContext.Draw(geometryCom.Positions.Count, 0);
            }
        }

    }
}
