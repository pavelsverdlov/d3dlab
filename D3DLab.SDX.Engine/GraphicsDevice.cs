using D3DLab.Std.Engine.Core;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;

namespace D3DLab.SDX.Engine {
    public sealed class AdapterFactory {
        public static event Func<Adapter[], int, Adapter> SelectAdapter;

        public static Adapter GetBestAdapter(global::SharpDX.DXGI.Factory f) {
            Adapter bestAdapter = null;
            var bestLevel = global::SharpDX.Direct3D.FeatureLevel.Level_11_1;

            var selectedId = -1;
            for (int i = 0; i < f.Adapters.Length; i++) {
                Adapter adapter = f.Adapters[i];
                var level = global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(adapter);
                if (bestAdapter == null || level > bestLevel) {
                    selectedId = adapter.Description.DeviceId;
                    bestAdapter = adapter;
                    bestLevel = level;
                    break;
                }
            }

            if (SelectAdapter != null) {
                bestAdapter = SelectAdapter(f.Adapters, selectedId) ?? bestAdapter;
            }

            return bestAdapter;
        }
    }

    internal class GraphicsDevice {
        internal readonly SharpDX.Direct3D11.Device Device;
        internal readonly DeviceContext ImmediateContext;
        readonly RenderTargetView renderTargetView;
        readonly DepthStencilView depthStencilView;

        readonly SwapChain swapChain;        
        readonly IAppWindow window;
        int Width => (int)window.Width;
        int Height => (int)window.Height;

        public GraphicsDevice(IAppWindow window) {
            this.window = window;
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
            SharpDX.Direct3D11.Device.CreateWithSwapChain(adapter, DeviceCreationFlags.Debug, swapChainDesc, out var d3dDevice, out var sch);

            swapChain = sch.QueryInterface<SwapChain4>();
            Device = d3dDevice.QueryInterface<Device5>();

            ImmediateContext = d3dDevice.ImmediateContext;

            var viewport = new Viewport(0, 0, Width, Height);
            ImmediateContext.Rasterizer.SetViewport(viewport);

            // Create render target view for back buffer
            using (Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0)) {
                renderTargetView = new RenderTargetView(d3dDevice, backBuffer);
            }

            var zBufferTextureDescription = new Texture2DDescription {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            
            using (var zBufferTexture = new Texture2D(Device, zBufferTextureDescription)) { 
                depthStencilView = new DepthStencilView(Device, zBufferTexture);
            }
            var depthDisabledStencilDesc = new DepthStencilStateDescription() {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                // Stencil operation if pixel front-facing.
                FrontFace = new DepthStencilOperationDescription() {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
                // Stencil operation if pixel is back-facing.
                BackFace = new DepthStencilOperationDescription() {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                }
            };
            var depthDisabledStencilState = new DepthStencilState(Device, depthDisabledStencilDesc);


            //no zbuffer and DepthStencil
            //ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView);

            //with zbuffer / DepthStencil
            ImmediateContext.OutputMerger.SetDepthStencilState(depthDisabledStencilState, 0);
            ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
        }

        internal void UpdateRasterizerState(RasterizerStateDescription descr) {
            ImmediateContext.Rasterizer.State = new RasterizerState(Device, descr);
        }

        public void Refresh() {
            try {
                ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
                ImmediateContext.ClearRenderTargetView(renderTargetView, new RawColor4(0, 0, 0, 0));
            } catch (Exception ex) {
                ex.ToString();
            }           
        }

        public static bool IsDirectX11Supported() {
            return global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
        }

        public void Present() {
            swapChain.Present(1, PresentFlags.None);
            //swapChain.Present(1, PresentFlags.None, new PresentParameters());
        }

        void Resize(uint width, uint height) {
            float _pixelScale = 1;
            uint actualWidth = (uint)(width * _pixelScale);
            uint actualHeight = (uint)(height * _pixelScale);
            swapChain.ResizeBuffers(2, (int)actualWidth, (int)actualHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            //using (Texture2D backBufferTexture = swapChain.GetBackBuffer<Texture2D>(0)) {
            //    if (_depthFormat != null) {
            //        TextureDescription depthDesc = new TextureDescription(
            //            actualWidth, actualHeight, 1, 1, 1,
            //            _depthFormat.Value,
            //            TextureUsage.DepthStencil,
            //            TextureType.Texture2D);
            //        _depthTexture = new D3D11Texture(_device, ref depthDesc);
            //    }

            //    D3D11Texture backBufferVdTexture = new D3D11Texture(backBufferTexture);
            //    FramebufferDescription desc = new FramebufferDescription(_depthTexture, backBufferVdTexture);
            //    _framebuffer = new D3D11Framebuffer(_device, ref desc);
            //    _framebuffer.Swapchain = this;
            //}
        }

        public void Dispose() {
            renderTargetView.Dispose();
            Device.Dispose();
            swapChain.Dispose();
        }

        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, ref T range)
            where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(Device, flags, ref range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, T[] range)
           where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(Device, flags, range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(T[] range,  BufferDescription desc)
          where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(Device, range, desc);
        }
        /// <summary>
        /// For referrence types and any arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="structureByteStride"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        public SharpDX.Direct3D11.Buffer CreateDynamicBuffer<T>(T[] range, int sizeInBytes)
         where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(Device, range, new BufferDescription {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Dynamic,
              //  StructureByteStride = structureByteStride,
                SizeInBytes = sizeInBytes
            });
        }
        //SharpDX.DataBox src = context.MapSubresource(lightDataBuffer, 1, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out SharpDX.DataStream stream);
        //stream.Write(light.GetStructLayoutResource());
        //context.UnmapSubresource(lightDataBuffer, LightStructLayout.RegisterResourceSlot);

        public void UpdateDynamicBuffer<T>(T[] newdata, SharpDX.Direct3D11.Buffer buffer, int slot) where T : struct{
            SharpDX.DataStream stream = null;
            try {
                SharpDX.DataBox src = ImmediateContext.MapSubresource(buffer, slot, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
                stream.WriteRange(newdata);
                ImmediateContext.UnmapSubresource(buffer, slot);
            } finally {

            }
        }

        public void UpdateSubresource<T>(ref T data, SharpDX.Direct3D11.Buffer buff, int subresource) where T : struct {
            ImmediateContext.UpdateSubresource(ref data, buff, subresource);
        }

    }
}
