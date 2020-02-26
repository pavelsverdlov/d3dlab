using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Shader;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine {
    public class GraphicsDeviceException : Exception {
        GraphicsDeviceException(string mess) : base(mess) { }
        public static Exception ResourseSlotAlreadyUsed(int slot) { return new GraphicsDeviceException($"Resourse Slot '{slot}' is already used."); }
        public static Exception ShaderAddedTwice() { return new GraphicsDeviceException($"Shader war added twise to DeviceContext."); }

        public static Exception NotDynamicBuffer =>
            new GraphicsDeviceException($"Can't be updated. Buffer must be CpuAccessFlags.Write & ResourceUsage.Dynamic.");
    }

    sealed class AdapterFactory {
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




    public interface IGraphicsDevice {
        Texture2D GetBackBuffer();
        MemoryStream CopyBackBufferMemoryStream();
        SharpDX.Direct3D11.Device5 D3DDevice { get; }
    }

    public class GraphicsDevice : IGraphicsDevice {
        const Format BackBufferTextureFormat = Format.R8G8B8A8_UNorm;

        abstract class DirectX11 {
            public SharpDX.Direct3D11.Device5 D3DDevice;
            public DeviceContext ImmediateContext;//readonly
            public RenderTargetView RenderTarget;

            public virtual void Dispose() {
                RenderTarget.Dispose();

                if (!ImmediateContext.IsDisposed) {
                    ImmediateContext.ClearState();
                    ImmediateContext.Flush();
                    ImmediateContext.Dispose();
                }

                D3DDevice.Dispose();
            }
            public abstract Texture2D GetBackBuffer();
            public abstract void Present();
            public abstract void Resize(int width, int height);
        }
        class RenderToTexture : DirectX11 {
            Texture2D targetTexture;
            public RenderToTexture(Adapter adapter, int width, int height) {
                var d = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport);
                D3DDevice = d.QueryInterface<Device5>();
                ImmediateContext = D3DDevice.ImmediateContext;

                //Resize(width, height);
            }
            public override Texture2D GetBackBuffer() => targetTexture;
            public override void Dispose() {
                base.Dispose();
                targetTexture.Dispose();
            }

            public override void Present() {

            }

            public override void Resize(int width, int height) {
                targetTexture = new Texture2D(D3DDevice, new Texture2DDescription() {
                    Format = Format.B8G8R8A8_UNorm,//Format.B8G8R8A8_UNorm,//
                    Width = width,//Bgra32
                    Height = height,
                    ArraySize = 1,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.Shared, //ResourceOptionFlags.None
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                });

                RenderTarget = new RenderTargetView(D3DDevice, targetTexture);
            }
        }
        class RenderWithSwapChain : DirectX11 {
            readonly SwapChain4 swapChain;
            public RenderWithSwapChain(Adapter adapter, IntPtr handle, int width, int height) {

                var backBufferDesc = new ModeDescription(width, height, new Rational(60, 1), BackBufferTextureFormat);

                // Descriptor for the swap chain
                var swapChainDesc = new SwapChainDescription() {
                    ModeDescription = backBufferDesc,
                    SampleDescription = new SampleDescription(1, 0),
                    BufferCount = 2,
                    IsWindowed = true,
                    OutputHandle = handle,
                    Usage = Usage.RenderTargetOutput,
                };
                // Create device and swap chain
                var flags = DeviceCreationFlags.None;
#if DEBUG
                flags |= DeviceCreationFlags.Debug;
#endif
                if (SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_1)) { //update win->dxdiag
                    //flags |= DeviceCreationFlags.Debuggable;
                }
                if (SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(adapter, FeatureLevel.Level_12_0)) {

                }

                //var result = SharpDX.Direct3D11.Device.CreateDevice(adapter, DriverType.Unknown, IntPtr.Zero, DeviceCreationFlags.None,
                //                          new[] { FeatureLevel.Level_11_1 }, 1, D3D11.SdkVersion, device, out outputLevel,
                //                          out var context);


                SharpDX.Direct3D11.Device.CreateWithSwapChain(adapter, flags, swapChainDesc, out var d3dDevice, out var sch);

                swapChain = sch.QueryInterface<SwapChain4>();
                D3DDevice = d3dDevice.QueryInterface<Device5>();

                ImmediateContext = d3dDevice.ImmediateContext;

                // Enable object tracking
                //SharpDX.Configuration.EnableObjectTracking = true;
            }

            public override Texture2D GetBackBuffer() => swapChain.GetBackBuffer<Texture2D>(0);

            public override void Dispose() {
                base.Dispose();
                swapChain.Dispose();
                ImmediateContext.Dispose();
            }

            public override void Present() {
                //swapChain.Present(1, PresentFlags.None);//TODO: use second one
                swapChain.Present(1, PresentFlags.None, new PresentParameters());

                try {//only for Window 10
                     //    WaitForSingleObjectEx(swapChain.FrameLatencyWaitableObject.ToInt32(), 1000, true);
                } catch {

                }

                // Output the current active Direct3D objects
                //System.Diagnostics.Debug.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
            }

            public override void Resize(int width, int height) {
                swapChain.ResizeBuffers(2, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

                using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0)) {
                    RenderTarget = new RenderTargetView(D3DDevice, backBuffer);
                }
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

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern int WaitForSingleObjectEx(int hHandle, int dwMilliseconds, bool bAlertable);
        }


        class ResourseRegistrHash {
            readonly Dictionary<int, HashSet<int>> shaders;

            public ResourseRegistrHash() {
                shaders = new Dictionary<int, HashSet<int>>();
            }

            public void RegisterShader(int ptr) {
                if (shaders.ContainsKey(ptr)) {
                    throw GraphicsDeviceException.ShaderAddedTwice();
                }
                shaders.Add(ptr, new HashSet<int>());
            }
            public void RegisterResourseSlot(int ptr, int slot) {
                if (shaders[ptr].Contains(slot)) {
                    throw GraphicsDeviceException.ResourseSlotAlreadyUsed(slot);
                }
                shaders[ptr].Add(slot);
            }
            public void Clear() {
                shaders.Clear();
            }
        }

        public readonly D3DShaderCompilator Compilator;

        public SharpDX.Direct3D11.Device5 D3DDevice => directX.D3DDevice;
        public TextureLoader TexturedLoader { get; }
        public DeviceContext ImmediateContext => directX.ImmediateContext;
        public string VideoCardDescription { get; }
        public System.Drawing.Size Size { get; private set; }

        DepthStencilView depthStencilView;

        readonly ResourseRegistrHash resourseHash;
        readonly DirectX11 directX;

        public GraphicsDevice(IntPtr handle, float w, float h) {
            resourseHash = new ResourseRegistrHash();
            Compilator = new D3DShaderCompilator();

            var width = (int)w;
            var height = (int)h;

            var factory = new Factory1();
            var adapter = AdapterFactory.GetBestAdapter(factory);

            VideoCardDescription = adapter.Description.Description.Trim('\0');
            /*
             * 
             *  DeviceCreationFlags.Debug - not supported by default, need to install the optional feature Graphics Tools
             * 
             */



            directX = handle == IntPtr.Zero ?
                (DirectX11)new RenderToTexture(adapter, width, height) :
                new RenderWithSwapChain(adapter, handle, width, height);

            directX.Resize(width, height);
            CreateBuffers(width, height);
            //TODO: Динамический оверлей. Direct3D 11.2 https://habr.com/company/microsoft/blog/199380/
            //swapChain.SetSourceSize
            //DContext = new DeviceContext(D3DDevice);

            TexturedLoader = new TextureLoader(directX.D3DDevice);
        }

        public void Dispose() {
            directX.Dispose();
        }

        public void Resize(float w, float h) {
            var width = (int)Math.Ceiling(w);
            var height = (int)Math.Ceiling(h);

            directX.RenderTarget.Dispose();
            depthStencilView.Dispose();

            directX.Resize(width, height);
            CreateBuffers(width, height);
        }

        void CreateBuffers(int width, int height) {
            Size = new System.Drawing.Size(width, height);
            var zBufferTextureDescription = new Texture2DDescription {
                Format = Format.D32_Float_S8X24_UInt,//D24_UNorm_S8_UInt
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            using (var zBufferTexture = new Texture2D(directX.D3DDevice, zBufferTextureDescription)) {
                depthStencilView = new DepthStencilView(directX.D3DDevice, zBufferTexture);
            }

            var depthEnabledStencilState = new DepthStencilState(directX.D3DDevice, D3DDepthStencilStateDescriptions.DepthEnabled);

            var viewport = new SharpDX.Viewport(0, 0, width, height, 0f, 1.0f);
            ImmediateContext.Rasterizer.SetViewport(viewport);
            ImmediateContext.OutputMerger.SetTargets(depthStencilView, directX.RenderTarget);

            //no zbuffer and DepthStencil
            //ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView);

            //with zbuffer / DepthStencil
            ImmediateContext.OutputMerger.SetDepthStencilState(depthEnabledStencilState, 0);

            //var blendStateDesc = new BlendStateDescription();
            //blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            //blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            //blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            //blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            //blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            //blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            //blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            //blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            //var blend = new BlendState(Device, blendStateDesc);

            //var blendFactor = new Color4(0, 0, 0, 0);
            //Device.ImmediateContext.OutputMerger.SetBlendState(blend, blendFactor, -1);
        }

        [Obsolete("Store RasterizerState in render component")]
        public void UpdateRasterizerState(RasterizerStateDescription2 descr) {
            ImmediateContext.Rasterizer.State = new RasterizerState2(directX.D3DDevice, descr);
        }

        public void Refresh() {
            ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1f, 0);
            ImmediateContext.ClearRenderTargetView(directX.RenderTarget, new RawColor4(0, 0, 0, 0));
        }

        public static bool IsDirectX11Supported() {
            return global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
        }

        public void Present() {
            directX.Present();
            resourseHash.Clear();
            // CopyBackBufferTexture().Save(@"D:\Zirkonzahn\MB_Database\back.png");
        }

        public Texture2D GetBackBuffer() => directX.GetBackBuffer();

        public System.Drawing.Bitmap CopyBackBufferTexture() {
            using (var stream = new MemoryStream()) {
                //using (var tex = directX.GetBackBuffer()) { //for swapchain
                var tex = directX.GetBackBuffer();
                Copy(tex, stream, directX.D3DDevice);
                stream.Position = 0;
                var bmp = new System.Drawing.Bitmap(stream);
                return bmp;
            }
        }
        public MemoryStream CopyBackBufferMemoryStream() {
            using (var stream = new MemoryStream()) {
                //using (var tex = directX.GetBackBuffer()) { //for swapchain
                var tex = directX.GetBackBuffer();
                Copy(tex, stream, directX.D3DDevice);
                stream.Position = 0;
                return stream;
            }
        }

        //TODO: check this way to getting back texture
        //public BitmapSource ToBitmap() {
        //    if (_d3D11Image == null)
        //        return null;

        //    // Copy back buffer to WriteableBitmap.
        //    int width = _d3D11Image.PixelWidth;
        //    int height = _d3D11Image.PixelHeight;
        //    var format = EnableAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
        //    var writeableBitmap = new WriteableBitmap(width, height, 96, 96, format, null);
        //    writeableBitmap.Lock();
        //    try {
        //        uint[] data = new uint[width * height];
        //        _d3D11Image.TryGetData(data);

        //        // Get a pointer to the back buffer.
        //        unsafe {
        //            uint* pBackbuffer = (uint*)writeableBitmap.BackBuffer;
        //            for (int i = 0; i < data.Length; i++)
        //                pBackbuffer[i] = data[i];
        //        }

        //        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        //    } finally {
        //        writeableBitmap.Unlock();
        //    }

        //    return writeableBitmap;
        //}


        static void Copy(Texture2D texture, Stream stream, SharpDX.Direct3D11.Device device) {
            var desc = new Texture2DDescription {
                Width = (int)texture.Description.Width,
                Height = (int)texture.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = texture.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };



            using (var factory = new SharpDX.WIC.ImagingFactory()) {
                using (var textureCopy = new Texture2D(device, desc)) {
                    device.ImmediateContext.CopyResource(texture, textureCopy);

                    var dataBox = device.ImmediateContext.MapSubresource(
                        textureCopy,
                        0,
                        0,
                        MapMode.Read,
                        global::SharpDX.Direct3D11.MapFlags.None,
                        out SharpDX.DataStream dataStream);
                    using (dataStream) {
                        var t = dataStream.ReadByte(); //ReadFloat();

                        var dataRectangle = new SharpDX.DataRectangle {
                            DataPointer = dataStream.DataPointer,
                            Pitch = dataBox.RowPitch
                        };
                        //https://github.com/sharpdx/Toolkit/blob/master/Source/Toolkit/SharpDX.Toolkit.Graphics/WICHelper.cs
                        using (var bitmap = new SharpDX.WIC.Bitmap(factory, textureCopy.Description.Width, textureCopy.Description.Height,
                            SharpDX.WIC.PixelFormat.Format32bppRGBA, dataRectangle, 0)) {

                            stream.Position = 0;
                            using (var bitmapEncoder = new SharpDX.WIC.PngBitmapEncoder(factory, stream)) {
                                using (var bitmapFrameEncode = new SharpDX.WIC.BitmapFrameEncode(bitmapEncoder)) {
                                    bitmapFrameEncode.Initialize();
                                    bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                                    var pixelFormat = SharpDX.WIC.PixelFormat.FormatDontCare;
                                    bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                                    bitmapFrameEncode.WriteSource(bitmap);
                                    bitmapFrameEncode.Commit();
                                    bitmapEncoder.Commit();
                                }
                            }

                        }
                        device.ImmediateContext.UnmapSubresource(textureCopy, 0);
                    }
                }
            }
        }


        public SharpDX.Direct3D11.Buffer CreateUnorderedRWStructuredBuffer<T>(ref T[] range) where T : struct {
            BufferStaticVerifications.CheckSizeInBytes<T>();

            var desc = new BufferDescription() {
                SizeInBytes = Unsafe.SizeOf<T>(),
                CpuAccessFlags = GetCpuAccessFlagsFromUsage(ResourceUsage.Default),
                Usage = ResourceUsage.Default,
            };

            desc.OptionFlags |= ResourceOptionFlags.BufferStructured;
            desc.BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource;

            // We keep the element size in the structure byte stride, even if it is not a structured buffer
            desc.StructureByteStride = desc.SizeInBytes / range.Length;


            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, desc);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, ref T range)
            where T : struct {
            BufferStaticVerifications.CheckSizeInBytes<T>();
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, flags, ref range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, T[] range)
           where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, flags, range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(T[] range, BufferDescription desc)
          where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, desc);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(ref T data, BufferDescription desc)
         where T : struct {
            BufferStaticVerifications.CheckSizeInBytes(desc.SizeInBytes);
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, ref data, desc);
        }

        public SharpDX.Direct3D11.Buffer CreateDynamicBuffer<T>(ref T range, int sizeInBytes)
        where T : struct {
            BufferStaticVerifications.CheckSizeInBytes(sizeInBytes);

            var des = new BufferDescription() {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                //    StructureByteStride = 0, 
            };
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, ref range, des);
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
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, new BufferDescription {
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

        public void UpdateDynamicBuffer<T>(T[] newdata, SharpDX.Direct3D11.Buffer buffer, int slot) where T : struct {
            var desc = buffer.Description;
            if (desc.CpuAccessFlags != CpuAccessFlags.Write && desc.Usage != ResourceUsage.Dynamic) {
                throw GraphicsDeviceException.NotDynamicBuffer;
            }
            try {
                ImmediateContext.MapSubresource(buffer, slot, MapMode.WriteDiscard,
                    SharpDX.Direct3D11.MapFlags.None, out var stream);
                // src.DataPointer
                stream.WriteRange(newdata);
            } finally {
                ImmediateContext.UnmapSubresource(buffer, slot);
            }
        }

        public void UpdateDynamicBuffer<T>(ref T newdata, SharpDX.Direct3D11.Buffer buffer, int slot) where T : struct {
            var desc = buffer.Description;
            if (desc.CpuAccessFlags != CpuAccessFlags.Write && desc.Usage != ResourceUsage.Dynamic) {
                throw GraphicsDeviceException.NotDynamicBuffer;
            }
            try {
                //  Disable GPU access to the buffer data.
                ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None,
                    out var mappedResource);
                mappedResource.Write(newdata);
            } finally {
                //  Reenable GPU access to the buffer data.
                ImmediateContext.UnmapSubresource(buffer, slot);
            }


            //try {
            //    var box = ImmediateContext.MapSubresource(buffer, slot, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //    SharpDX.Utilities.WriteAndPosition(box.DataPointer, ref newdata);
            //} catch (Exception ex) {
            //    ex.ToString();
            //} finally {
            //    ImmediateContext.UnmapSubresource(buffer, slot);
            //}
        }

        public void UpdateArraySubresource<T>(T[] data, SharpDX.Direct3D11.Buffer buff) where T : struct {
            ImmediateContext.UpdateSubresource(data, buff);
        }
        public void UpdateSubresource<T>(ref T data, SharpDX.Direct3D11.Buffer buff, int subresource) where T : struct {
            ImmediateContext.UpdateSubresource(ref data, buff, subresource);
        }
        public void UpdateSubresource<T>(ref T data, SharpDX.Direct3D11.Buffer buff) where T : struct {
            ImmediateContext.UpdateSubresource(ref data, buff);
        }

        public void RegisterConstantBuffer(CommonShaderStage stage, int slot, SharpDX.Direct3D11.Buffer buff) {
            stage.SetConstantBuffer(slot, buff);
            resourseHash.RegisterResourseSlot(stage.GetHashCode(), slot);
        }
        public void RegisterConstantBuffer(CommonShaderStage stage, int slot, DisposableSetter<SharpDX.Direct3D11.Buffer> buff) {
            RegisterConstantBuffer(stage, slot, buff.Get());
        }

        public void SetVertexShader(DisposableSetter<VertexShader> shader) {
            ImmediateContext.VertexShader.Set(shader.Get());
            resourseHash.RegisterShader(ImmediateContext.VertexShader.GetHashCode());
        }
        public void SetPixelShader(DisposableSetter<PixelShader> shader) {
            ImmediateContext.PixelShader.Set(shader.Get());
            resourseHash.RegisterShader(ImmediateContext.PixelShader.GetHashCode());
        }

        public SamplerState CreateSampler(SamplerStateDescription desc) {
            return new SamplerState(D3DDevice, desc);
        }

        #region RenderTargets

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTargetView"></param>
        /// <param name="startSlot"></param>
        /// <param name="unorderedAccessViews"></param>
        /// <unmanaged>void ID3D11DeviceContext::OMSetRenderTargetsAndUnorderedAccessViews([In] unsigned int NumRTVs,[In, Buffer, Optional] const ID3D11RenderTargetView** ppRenderTargetViews,[In, Optional] ID3D11DepthStencilView* pDepthStencilView,[In] unsigned int UAVStartSlot,[In] unsigned int NumUAVs,[In, Buffer, Optional] const ID3D11UnorderedAccessView** ppUnorderedAccessViews,[In, Buffer, Optional] const unsigned int* pUAVInitialCounts)</unmanaged>	
        /// <unmanaged-short>ID3D11DeviceContext::OMSetRenderTargetsAndUnorderedAccessViews</unmanaged-short>	
        //public void SetRenderTargetsAndUnorderedAccessViews(RenderTargetView renderTargetView, DepthStencilView depthStencil, int startSlot, int count, UnorderedAccessView[] unorderedAccessViews) {
        //   // ImmediateContext.OutputMerger.SetTargets(renderTargetView, startSlot, unorderedAccessViews);
        //    ImmediateContext.OutputMerger.SetTargets(depthStencil, renderTargetView, startSlot, unorderedAccessViews, count);
        //}

        #endregion


        static CpuAccessFlags GetCpuAccessFlagsFromUsage(ResourceUsage usage) {
            switch (usage) {
                case ResourceUsage.Dynamic:
                    return CpuAccessFlags.Write;
                case ResourceUsage.Staging:
                    return CpuAccessFlags.Read | CpuAccessFlags.Write;
            }
            return CpuAccessFlags.None;
        }

    }
    /*
    
    D3D11_MAP_WRITE_NO_OVERWRITE if you plan on writing to the buffer more than once per frame
    MapMode.WriteNoOverwrite


     */
}
