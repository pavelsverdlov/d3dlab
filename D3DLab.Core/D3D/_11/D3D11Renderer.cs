using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using D3DLab.Core;
using D3DLab.Core.Common;
using D3DLab.Core.D3D._11;
using D3DLab.Core.Render;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = System.Buffer;
using Device = SharpDX.DXGI.Device;

namespace D3DLab.Core.D3D._11 {
    using SharpDX.Direct3D11;

    public struct ShaderResource {
        public readonly string FileName;
        public readonly string EntryPoint;
        public readonly ShaderFlags Flags;

        public ShaderResource(string fileName, string entryPoint) {
            FileName = fileName;
            EntryPoint = entryPoint;

            Flags = ShaderFlags.EnableStrictness;
#if DEBUG
            Flags |= ShaderFlags.Debug;
#endif
        }
    }

    public class ShaderResources {
        private readonly List<ShaderResource> pixels;
        private readonly List<ShaderResource> vertexes;

        public ShaderResources() {
            this.pixels = new List<ShaderResource>();
            this.vertexes = new List<ShaderResource>();
        }

        public void AddPixel(ShaderResource resource) {
            pixels.Add(resource);
        }
        public void AddVertex(ShaderResource resource) {
            vertexes.Add(resource);
        }

    }

    public class ConstantBuffer<T> : IDisposable, INotifyPropertyChanged
        where T : struct {
        private Device _device;
        private Buffer _buffer;
        private DataStream _dataStream;

        public Buffer Buffer { get { return _buffer; } }
        public Device Device { get { return _device; } }

        public ConstantBuffer(Device device)
            : this(device, new BufferDescription {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            }) {
        }

        public ConstantBuffer(Device device, BufferDescription desc) {
            desc.SizeInBytes = Marshal.SizeOf(typeof(T));

            if (device == null)
                throw new ArgumentNullException("device");

            this._device = device;
            //            _device.AddReference();

            _buffer = new Buffer(device, desc);
            _dataStream = new DataStream(desc.SizeInBytes, true, true);
        }

        ~ConstantBuffer() {
            Dispose(false);
        }
        public void Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
        void Dispose(bool disposing) {
            if (_device == null)
                return;

            if (disposing)
                _dataStream.Dispose();
            // NOTE:_device.Release() SharpDX 1.3 requires explicit Dispose() of all resource
            _device.Dispose();
            _buffer.Dispose();
            _device = null;
            _buffer = null;
        }

        public T Value {
            get { return bufvalue; }
            set {
                if (_device == null)
                    throw new ObjectDisposedException(GetType().Name);

                bufvalue = value;

                Marshal.StructureToPtr(value, _dataStream.DataPointer, false);
                //var dataBox = new DataBox(0, 0, _dataStream);
                var dataBox = new DataBox(_dataStream.DataPointer);
                _device.ImmediateContext.UpdateSubresource(dataBox, _buffer, 0);

                OnPropertyChanged("Value");
            }
        }
        T bufvalue;

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string name) {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Projections {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
    }

    public class D3D11Renderer : BaseRenderer, IRenderer {

        public sealed class D3D11ShaderCompiler : IDisposable {
            public SharpDX.Direct3D11.VertexShader vertexShader;
            public SharpDX.Direct3D11.PixelShader pixelShader;
            public ShaderSignature inputSignature;

            public D3D11ShaderCompiler() {
            }

            public void Compile(D3D11Device device) {

                // Compile the vertex shader code
                using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug)) {
                    // Read input signature from shader code
                    inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                    vertexShader = new VertexShader(device.Device, vertexShaderByteCode);
                    //var effect = new Effect(device.Device, vertexShaderByteCode);
                }


                // Compile the pixel shader code
                using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug)) {
                    pixelShader = new SharpDX.Direct3D11.PixelShader(device.Device, pixelShaderByteCode);
                }

                
            }
            public void Dispose() {
                inputSignature.Dispose();
                vertexShader.Dispose();
                pixelShader.Dispose();
            }
        }
        public sealed class D3D11Device : IDisposable {

            public SharpDX.Direct3D11.Device Device => device;
            public SharpDX.Direct3D11.DeviceContext ImmediateContext => Device.ImmediateContext;
            public SwapChain swapChain;
            private Viewport viewport;

            private SharpDX.Direct3D11.Device device;

            ~D3D11Device() { Dispose(); }

            public void Attach(IntPtr handle, int width, int height) {
                ModeDescription backBufferDesc = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

                // Descriptor for the swap chain
                SwapChainDescription swapChainDesc = new SwapChainDescription() {
                    ModeDescription = backBufferDesc,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput,
                    BufferCount = 1,
                    OutputHandle = handle,
                    IsWindowed = true
                };

                // Create device and swap chain
                SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None, swapChainDesc, out device, out swapChain);

                viewport = new Viewport(0, 0, width, height);
                ImmediateContext.Rasterizer.SetViewport(viewport);
            }

            public void Detach() {
                Dispose();
            }

            public void Dispose() {
                swapChain.Dispose();
                device.Dispose();
                ImmediateContext.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private readonly D3D11Device device;
        private readonly D3D11ShaderCompiler shader;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;
        private SharpDX.Direct3D11.InputLayout inputLayout;


        private static readonly SharpDX.Direct3D11.InputElement[] InputElements = new SharpDX.Direct3D11.InputElement[] {
            new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, SharpDX.Direct3D11.InputClassification.PerVertexData, 0),
            new SharpDX.Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, SharpDX.Direct3D11.InputClassification.PerVertexData, 0)
        };

        private ConstantBuffer<Projections> constantBuffer;
        private Stopwatch timer;


        public D3D11Renderer(IntPtr handleForm) : base(handleForm) {
            device = new D3D11Device();
            shader = new D3D11ShaderCompiler();
        }

        public void Attach() {
            device.Attach(handleForm, Width, Height);
            // Create render target view for back buffer
            using (SharpDX.Direct3D11.Texture2D backBuffer = device.swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0)) {
                renderTargetView = new SharpDX.Direct3D11.RenderTargetView(device.Device, backBuffer);
            }

            shader.Compile(device);

            device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Create the input layout from the input signature and the input elements
            inputLayout = new SharpDX.Direct3D11.InputLayout(device.Device, shader.inputSignature, InputElements);
            // Set input layout to use
            device.ImmediateContext.InputAssembler.InputLayout = inputLayout;

            // --- create the constant buffer
            constantBuffer = new D3DLab.Core.D3D._11.ConstantBuffer<Projections>(device.Device);
            device.ImmediateContext.VertexShader.SetConstantBuffer(0, constantBuffer.Buffer);
            timer = new Stopwatch();
        }



        public void Attach(IRenderable renderable) {

        }

        public void Draw(IRenderable renderable) {
            if (!timer.IsRunning) {
                timer.Start();
            }

            // Set render targets
            device.ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView);
            // Clear the screen
            device.ImmediateContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            var vertices = renderable.GetRenderData();
            var indexes = renderable.GetIndices();
            using (var dispose = new DisposeGroup()) {
                // Create a vertex buffer, and use our array with vertices as data
                var triangleVertexBuffer = SharpDX.Direct3D11.Buffer.Create(device.Device, SharpDX.Direct3D11.BindFlags.VertexBuffer, vertices);
                //                var indicesBuffer = CreateBuffer(device.Device, indexes);
                var indicesBuffer = SharpDX.Direct3D11.Buffer.Create(device.Device, SharpDX.Direct3D11.BindFlags.VertexBuffer, indexes);

                // Set vertex buffer
                var vertexbuf = new SharpDX.Direct3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0);

                device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexbuf);
                device.ImmediateContext.InputAssembler.SetIndexBuffer(indicesBuffer, Format.R16_UInt, 0);

                triangleVertexBuffer.Dispose();
                indicesBuffer.Dispose();
            }
            float t = (float) timer.Elapsed.Seconds;
            var g_World = Matrix.RotationY(t);
            //
            // Update variables
            //
            constantBuffer.Value = new Projections {
                World = Matrix.Transpose(g_World),
                View = Matrix.Transpose(Matrix.LookAtLH(new Vector3(0.0f, 0.0f, -5.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0, 1, 0))),
                Projection = Matrix.Transpose(Matrix.PerspectiveFovLH((float)Math.PI / 2, 1280 / (float)720, 0.01f, 100.0f)),
            };

            // Set as current vertex and pixel shaders
            device.ImmediateContext.VertexShader.Set(shader.vertexShader);
            device.ImmediateContext.PixelShader.Set(shader.pixelShader);
            // Draw the triangle
            device.ImmediateContext.DrawIndexed(indexes.Length, 0, 0);

            // Swap front and back buffer
            device.swapChain.Present(1, PresentFlags.None);
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            inputLayout.Dispose();
            renderTargetView.Dispose();
        }

        //        public static Buffer CreateBuffer<T>(Device device, T[] range)
        //            where T : struct {
        //            int sizeInBytes = Marshal.SizeOf(typeof(T));
        //            using (var stream = new DataStream(range.Length * sizeInBytes, true, true)) {
        //                stream.WriteRange(range);
        //                return new Buffer(device, stream, new BufferDescription {
        //                    BindFlags = BindFlags.VertexBuffer,
        //                    SizeInBytes = (int)stream.Length,
        //                    CpuAccessFlags = CpuAccessFlags.None,
        //                    OptionFlags = ResourceOptionFlags.None,
        //                    StructureByteStride = 0,
        //                    Usage = ResourceUsage.Default,
        //                });
        //            }
        //        }
    }
}