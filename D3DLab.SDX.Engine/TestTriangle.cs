using D3DLab.Std.Engine.Core;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine {
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct VertexPositionColor {
        public readonly Vector3 Position;
        public readonly Color4 Color;

        public VertexPositionColor(Vector3 position, Color4 color) {
            Position = position;
            Color = color;
        }
    }

    public class TestTriangle1 : IDisposable {
        readonly IAppWindow window;
        private int Width => (int)window.Width;
        private int Height => (int)window.Height;

        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;

        /// <summary>
        /// Create and initialize a new game.
        /// </summary>
        public TestTriangle1(IAppWindow window) {
            this.window = window;
            InitializeDeviceResources();
          
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Run() {
            InitializeShaders();
            // Start the render loop
            RenderCallback();
        }

        private void RenderCallback() {
            Draw();
        }

        private void InitializeDeviceResources() {
            var backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            // Descriptor for the swap chain
            var swapChainDesc = new SwapChainDescription() {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = window.Handle,
                IsWindowed = true
            };

            var factory = new Factory1();
            var adapter = AdapterFactory.GetBestAdapter(factory);

            // Create device and swap chain
            D3D11.Device.CreateWithSwapChain(adapter, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);

            swapChain = swapChain.QueryInterface<SwapChain4>();
            d3dDevice = d3dDevice.QueryInterface<D3D11.Device5>();

            d3dDeviceContext = d3dDevice.ImmediateContext;

            var viewport = new Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);

            // Create render target view for back buffer
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0)) {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);          
        }

        private void InitializeShaders() {

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

            D3D11.InputElement[] inputElements = new D3D11.InputElement[]
        {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
            new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, D3D11.InputClassification.PerVertexData, 0)
        };

        // Triangle vertices
        VertexPositionColor[] vertices = new VertexPositionColor[] {
            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), SharpDX.Color.Red),
            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), SharpDX.Color.Green),
            new VertexPositionColor(new Vector3(0.0f, -0.5f, 0.0f), SharpDX.Color.Blue) };

        ShaderSignature inputSignature;
            D3D11.VertexShader vertexShader;
            D3D11.PixelShader pixelShader;

            // Compile the vertex shader code
            using (var vertexShaderByteCode = ShaderBytecode.Compile(vertexShaderText, "main", "vs_4_0", ShaderFlags.Debug)) {
                // Read input signature from shader code
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }

            // Compile the pixel shader code
            using (var pixelShaderByteCode = ShaderBytecode.Compile(pixelShaderText, "main", "ps_4_0", ShaderFlags.Debug)) {
                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }

            // Set as current vertex and pixel shaders
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Set input layout to use
            d3dDeviceContext.InputAssembler.InputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);

            var triangleVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));
        }

        /// <summary>
        /// Draw the game.
        /// </summary>
        private void Draw() {
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0));//new SharpDX.Color(32, 103, 178)
           
            d3dDeviceContext.Draw(3, 0);

            swapChain.Present(1, PresentFlags.None);
        }

        public void Dispose() {
            //inputLayout.Dispose();
            //inputSignature.Dispose();
           // triangleVertexBuffer.Dispose();
            //vertexShader.Dispose();
            //pixelShader.Dispose();
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
        }
    }
}
