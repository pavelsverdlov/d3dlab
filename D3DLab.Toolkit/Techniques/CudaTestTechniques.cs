using D3DLab.CLI.Toolkit;
using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;

using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques {
    public class CudaTestTechniques<TProperties> :
        NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {

        const string shaderText =
@"
@vertex@
cbuffer cbuf 
{ 
  float4 g_vQuadRect; 
  int g_UseCase; 
} 

struct Fragment{ 
    float4 Pos : SV_POSITION;
    float3 Tex : TEXCOORD0; 
};

Fragment main( uint vertexId : SV_VertexID )
{
    Fragment f;
    f.Tex = float3( 0.f, 0.f, 0.f); 
    if (vertexId == 1) f.Tex.x = 1.f; 
    else if (vertexId == 2) f.Tex.y = 1.f; 
    else if (vertexId == 3) f.Tex.xy = float2(1.f, 1.f); 
    
    f.Pos = float4( g_vQuadRect.xy + f.Tex * g_vQuadRect.zw, 0, 1);
    
    if (g_UseCase == 1) { 
        if (vertexId == 1) f.Tex.z = 0.5f; 
        else if (vertexId == 2) f.Tex.z = 0.5f; 
        else if (vertexId == 3) f.Tex.z = 1.f; 
    } 
    else if (g_UseCase >= 2) { 
        f.Tex.xy = f.Tex.xy * 2.f - 1.f; 
    } 
    return f;
}

@fragment@

Texture2D g_Texture2D : register(t0); 
SamplerState samLinear;

struct Fragment{ 
    float4 Pos : SV_POSITION;
    float3 Tex : TEXCOORD0; 
};

float4 main( Fragment f ) : SV_Target
{
    //return float4(0,1,0,1);
    return g_Texture2D.Sample( samLinear, f.Tex.xy ); 
    //return float4(f.Tex, 1);
}";

        struct ConstBuffer {
            public Vector4 QuadRect;
            public float UseCase;

            public Vector3 temp;
        }

        public IContextState ContextState { get; set; }

        readonly D3DShaderTechniquePass pass;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<DepthStencilState> depthStencilState;
        readonly DisposableSetter<SharpDX.Direct3D11.Buffer> constBuff;
        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;
        readonly CudaTexture2DTechnique cudaTexture2DTechnique;
        readonly DisposableSetter<SamplerState> sampleState;

        public CudaTestTechniques() {
            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(CudaTestTechniques<>)));
            pass = new D3DShaderTechniquePass(d.LoadText(shaderText, "TV_"));
            depthStencilStateDesc = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            constBuff = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            depthStencilState = new DisposableSetter<DepthStencilState>(disposer);
            cudaTexture2DTechnique = new CudaTexture2DTechnique();
            sampleState = new DisposableSetter<SamplerState>(disposer);
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

        public override bool IsAplicable(GraphicEntity entity) => true;

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            if (!depthStencilState.HasValue) {
                depthStencilState.Set(new DepthStencilState(graphics.D3DDevice, D3DDepthStencilStateDescriptions.DepthDisabled));
            }
            if (!sampleState.HasValue) {
                sampleState.Set(graphics.CreateSampler(SamplerStateDescriptions.Default));
            }

            var buff = new ConstBuffer {
                QuadRect = new Vector4(-0.9f, -0.9f, 0.7f, 0.7f),
                UseCase = 0,
            };
            if (!constBuff.HasValue) {
                constBuff.Set(graphics.CreateBuffer(BindFlags.ConstantBuffer, ref buff));
            }

            graphics.ClearAllShader();
            graphics.SetVertexShader(vertexShader);
            graphics.SetPixelShader(pixelShader);

            graphics.DisableIndexVertexBuffers();

            cudaTexture2DTechnique.Render(graphics.NativeGraphicsAdapter);

            context.VertexShader.SetConstantBuffer(0, constBuff.Get());
            context.PixelShader.SetSampler(0, sampleState.Get());

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
            context.OutputMerger.SetDepthStencilState(depthStencilState.Get());

    

            using (var rasterizerState = graphics.CreateRasterizerState(D3DRasterizerStateDescriptions.Default(CullMode.None))) {
                context.Rasterizer.State = rasterizerState;
                context.Draw(4, 0);
            }
        }
    }
}
