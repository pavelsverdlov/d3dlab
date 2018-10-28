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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Components {

    public class VertexLayoutConstructor {
        const string SemanticPositionName = "POSITION";
        const string SemanticNormalName = "NORMAL";
        const string SemanticColorName = "COLOR";
        const InputClassification perverxdata = InputClassification.PerVertexData;
        const Format Vector3 = Format.R32G32B32_Float;
        const Format Vector4 = Format.R32G32B32A32_Float;

        readonly List<InputElement> elements;
        public VertexLayoutConstructor() {
            elements = new List<InputElement>();
        }

        int GetOffset() {
            return elements.Count == 0 ? 0 : InputElement.AppendAligned;
        }

        public InputElement[] ConstuctElements() {
            return elements.ToArray();
        }

        internal VertexLayoutConstructor AddPositionElementAsVector3() {
            elements.Add(new InputElement(SemanticPositionName, 0, Vector3, GetOffset(), 0, perverxdata, 0));
            return this;
        }

        internal VertexLayoutConstructor AddNormalElementAsVector3() {
            elements.Add(new InputElement(SemanticNormalName, 0, Vector3, GetOffset(), 0, perverxdata, 0));
            return this;
        }

        internal VertexLayoutConstructor AddColorElementAsVector4() {
            elements.Add(new InputElement(SemanticColorName, 0, Vector4, GetOffset(), 0, perverxdata, 0));
            return this;
        }
    }

    public class D3DRenderComponent : IShaderEditingComponent {
        

        public ElementTag Tag { get; set; }
        public ElementTag EntityTag { get; set; }

        public IRenderTechniquePass Pass { get; protected set; }
        public RasterizerState RasterizerState { get; protected set; }

        protected D3DShaderCompilator compilator;
        protected bool initialized;

        
        public IShaderCompilator GetCompilator() {
            return compilator;
        }

        public void ReLoad() {
            initialized = false;
        }

    }



    /*
    cbuffer Game : register(b0) {
        float4x4 World;
        float4x4 View;
        float4x4 Projection;
        Light Lights[MAX_LIGHTS];
    };
    */
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameResourceBuffer {
        public const int RegisterResourceSlot = 0;

        public readonly Matrix4x4 World;
        public readonly Matrix4x4 View;
        public readonly Matrix4x4 Projection;
        //  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        // public Vector3[] vec;
        // public readonly LightBuffer Lights;
        public GameResourceBuffer(Matrix4x4 world, Matrix4x4 view, Matrix4x4 proj) {
            World = world;
            View = view;
            Projection = proj;
            //  Lights = new LightBuffer[1];
            //   Lights = new LightBuffer(1);
            //  vec = new[] { Vector3.Zero };
        }
    }

    public class RasterizerState {
        public FillMode FillMode {
            get => state.FillMode;
            set {
                state.FillMode = value;
            }
        }
        public CullMode CullMode {
            get => state.CullMode;
            set {
                state.CullMode = value;
            }
        }
        RasterizerStateDescription state;
        public RasterizerState(RasterizerStateDescription state) {
            this.state = state;
        }
        public RasterizerStateDescription GetDescription() {
            return state;
        }
    }

    public class D3DColoredVertexesRenderComponent : D3DRenderComponent, ID3DRenderableComponent {

        #region shaders

        const string vertexShaderText =
@"
#define MAX_LIGHTS 1

struct Light {
    uint Type;
    float Intensity;
    float3 Position;   
    float3 Direction;
    float4 Color;
};
struct VSOut
{
    float4 position : SV_POSITION;
    //float4 normal : NORMAL;
    float4 color : COLOR;
};

cbuffer Game : register(b0) {
	float4x4 World;
    float4x4 View;
    float4x4 Projection;
};
cbuffer Lights : register(b1) {
    Light lights[3];  
}

float computeLight(float3 position, float3 normal){
    float intensity = 0.0; 
    for(int i = 0; i < 3; ++i) {
        Light l = lights[i];
        if(l.Type == 1){ //ambient
            intensity += l.Intensity;
        } else {
            float3 lightDir;
            if(l.Type == 2){ //point
                lightDir = l.Position - position;
            } else if(l.Type == 3){ //directional
                lightDir = l.Direction;
            }
            float dotval = dot(normal, lightDir);
            if(dotval > 0){
                intensity += l.Intensity * dotval / (length(normal) * length(lightDir));
            }
        }
    }
    return intensity;
}
float computeLight(float3 position, float3 normal,Light l){
    float intensity = 0.0; 
        if(l.Type == 1){ //ambient
            intensity += l.Intensity;
        } else {
            float3 lightDir;
            if(l.Type == 2){ //point
                lightDir = l.Position - position;
            } else if(l.Type == 3){ //directional
                lightDir = l.Direction;
            }
            float dotval = dot(normal, lightDir);
            if(dotval > 0){
                intensity += l.Intensity * dotval / (length(normal) * length(lightDir));
            }
        }
    return intensity;
}

VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR) { 
    VSOut output = (VSOut)0;
    
    output.position = mul(View, position);
    output.position = mul(Projection, output.position);

    output.color = color * computeLight(position.xyz, normal);;

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

        

        public bool IsBuffersCreated { get; set; }

        VertexPositionColor[] vertices;
        int[] indexes;
        
        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;

        public D3DColoredVertexesRenderComponent() {
            RasterizerState = new RasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = true
            });
            compilator = new D3DShaderCompilator();
            Pass = new ShaderTechniquePass(new IShaderInfo[] {
                new ShaderInMemoryInfo("CV_VertexShader", vertexShaderText, null, ShaderStages.Vertex.ToString(), "main"),
                new ShaderInMemoryInfo("CV_FragmentShader", pixelShaderText, null, ShaderStages.Fragment.ToString(), "main"),
            });

            compilator.Compile(Pass.VertexShader);
            compilator.Compile(Pass.PixelShader);
        }

        public void Dispose() {
            
        }

        void ID3DRenderableComponent.Render(RenderState state) {
            var camera = state.Camera;
            var lights = state.Lights;
            var context = state.Graphics.ImmediateContext;

            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, camera.ViewMatrix, camera.ProjectionMatrix);
            var newlightData = lights;

            state.Graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameResourceBuffer.RegisterResourceSlot);
            state.Graphics.UpdateDynamicBuffer(newlightData, lightDataBuffer, LightStructLayout.RegisterResourceSlot);

            state.Graphics.UpdateRasterizerState(RasterizerState.GetDescription());

            state.Graphics.ImmediateContext.Draw(indexes.Length, 0); 
            // state.Graphics.ImmediateContext.DrawIndexed(indexes.Length, 0, 0);
        }


        void ID3DRenderableComponent.Update(RenderState state) {
            if (initialized) { return; }

            var device = state.Graphics.Device;
            var context = state.Graphics.ImmediateContext;

            var geometry = state.ContextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(EntityTag);

            indexes = geometry.Indices.ToArray();
            vertices = new VertexPositionColor[indexes.Length];
            for (var i = 0; i < indexes.Length; i++) {
                var index = indexes[i];
                vertices[i] = new VertexPositionColor(geometry.Positions[index], geometry.Normals[index], geometry.Colors[index]);
            }

            var layconst = new VertexLayoutConstructor()
                .AddPositionElementAsVector3()
                .AddNormalElementAsVector3()
                .AddColorElementAsVector4();

            var vertexShaderByteCode = Pass.VertexShader.ReadCompiledBytes();
            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            var dinamicLightbuff = state.Lights;

            var triangleVertexBuffer = state.Graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
            var indexBuffer = state.Graphics.CreateBuffer(BindFlags.IndexBuffer, indexes);

            gameDataBuffer = state.Graphics.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);
            lightDataBuffer = state.Graphics.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructLayout>() * dinamicLightbuff.Length);

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShader = new PixelShader(device, Pass.PixelShader.ReadCompiledBytes());

            context.VertexShader.Set(vertexShader);
            context.VertexShader.SetConstantBuffer(GameResourceBuffer.RegisterResourceSlot, gameDataBuffer);
            context.VertexShader.SetConstantBuffer(LightStructLayout.RegisterResourceSlot, lightDataBuffer);

            context.PixelShader.Set(pixelShader);

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_SInt, 0);

            initialized = true;
        }

       
    }
}
