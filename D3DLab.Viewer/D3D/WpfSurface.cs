using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;


namespace D3DLab.Viewer.D3D {
    using D3DLab.ECS.Input;
    using D3DLab.Render;
    using D3DLab.SDX.Engine;
    using ECS;
    using ECS.Input;
    using SharpDX.Direct3D11;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;

    public class WFSurface : IAppWindow, ISDXSurface {
        readonly System.Windows.Forms.Control surface;
        readonly FrameworkElement overlay;

        public WFSurface(System.Windows.Forms.Control control, FrameworkElement overlay, CurrentInputObserver input) {
            InputManager = new InputManager(input);
            this.surface = control;
            this.overlay = overlay;
            this.surface.SizeChanged += OnControlResized;
            width = (float)surface.Width;
            height = (float)surface.Width;
        }

        private void OnControlResized(object sender, EventArgs e) {
            //width = (float)overlay.ActualWidth;
            //height = (float)overlay.ActualHeight;

            width = (float)surface.Width;
            height = (float)surface.Height;

            Resized();
            surface.Refresh();
        }

        float width;
        float height;

        public event Action Resized = () => { };

        public float Width {
            get {
                return width;
            }
        }
        public float Height {
            get {
                return height;
            }
        }

        public bool IsActive => true;
        public IntPtr Handle => surface.Handle;
        //((HwndSource)HwndSource.FromVisual(control)).Handle;
        //  new WindowInteropHelper(Application.Current.MainWindow).Handle ;// new HwndSource(0, 0, 0, 0, 0, (int)Width, (int)height, "FakeHandle", IntPtr.Zero).Handle;
        public IInputManager InputManager { get; }

        public void Dispose() {
            this.surface.SizeChanged -= OnControlResized;
        }

        public System.Threading.WaitHandle BeginInvoke(Action action) {
            //surface.Dispatcher.InvokeAsync(action);
            return null;
        }

        public void SetTitleText(string txt) {

        }

        public void Present(IntPtr backBuffer) {
            //surface.Dispatcher.InvokeAsync(() => {
            //    surface.Lock();
            //    surface.SetBackBuffer(D3DResourceType.IDirect3DSurface9, backBuffer);
            //    surface.AddDirtyRect(new Int32Rect(0, 0, (int)Width, (int)Height));
            //    surface.Unlock();
            //});
        }

        public void SetRenderTarget(Texture2D tex) {
            //surface.Dispatcher.InvokeAsync(() => {
            //    surface.SetRenderTargetDX11(tex);
            //});
        }

        public void EndFrame(IGraphicsDevice device) {
            //DO NOTHING IN FW
        }

        public void StartFrame(IGraphicsDevice device) {

        }
    }
    public class WpfSurface : IAppWindow, ISDXSurface {
        readonly HwndHost surface;
        readonly FrameworkElement overlay;

        public WpfSurface(HwndHost control, FrameworkElement overlay, CurrentInputObserver input) {
            InputManager = new InputManager(input);
            this.surface = control;
            this.overlay = overlay;
            this.surface.SizeChanged += OnControlResized;

            width = (float)surface.ActualWidth;
            height = (float)surface.ActualHeight;

            //width = (float)surface.Width;
            //height = (float)surface.Width;

            //width = 150;
            //height = 150;

            //var d = (System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(control);
            //Handle = d.Handle;
            //var im = new System.Windows.Interop.D3DImage();
            //im.SetBackBuffer(System.Windows.Interop.D3DResourceType.IDirect3DSurface9,)

        }

        private void OnControlResized(object sender, EventArgs e) {
            width = (float)surface.ActualWidth;
            height = (float)surface.ActualHeight;

            //width = (float)surface.Width;
            //height = (float)surface.Height;

            Resized();
        }

        float width;
        float height;

        public event Action Resized = () => { };

        public float Width {
            get {
                return width;
            }
        }
        public float Height {
            get {
                return height;
            }
        }

        public bool IsActive => true;
        public IntPtr Handle => surface.Handle;
        //((HwndSource)HwndSource.FromVisual(control)).Handle;
        //  new WindowInteropHelper(Application.Current.MainWindow).Handle ;// new HwndSource(0, 0, 0, 0, 0, (int)Width, (int)height, "FakeHandle", IntPtr.Zero).Handle;
        public IInputManager InputManager { get; }

        public void Dispose() {
            this.overlay.SizeChanged -= OnControlResized;
        }

        public System.Threading.WaitHandle BeginInvoke(Action action) {
            surface.Dispatcher.InvokeAsync(action);
            return null;
        }

        public void SetTitleText(string txt) {

        }


        public void SetRenderTarget(Texture2D tex) {
            //surface.Dispatcher.InvokeAsync(() => {
            //    surface.SetRenderTargetDX11(tex);
            //});
        }

        public void EndFrame(IGraphicsDevice device) {

        }

        public void StartFrame(IGraphicsDevice device) {

        }
    }

    public class WpfD3DImageSurface : IAppWindow, ISDXSurface {
        readonly System.Windows.Controls.Image surface;
        readonly FrameworkElement overlay;

        public WpfD3DImageSurface(System.Windows.Controls.Image control, FrameworkElement overlay, CurrentInputObserver input) {
            InputManager = new InputManager(input);
            this.surface = control;
            this.overlay = overlay;
            this.overlay.SizeChanged += OnControlResized;

            Width = (float)overlay.ActualWidth;
            Height = (float)overlay.ActualHeight;
        }

        private void OnControlResized(object sender, SizeChangedEventArgs e) {
            Width = (float)overlay.ActualWidth;
            Height = (float)overlay.ActualHeight;

            Resized();
        }

        public float Width { get; private set; }
        public float Height { get; private set; }
        public bool IsActive => true;
        public IntPtr Handle => IntPtr.Zero;
        public IInputManager InputManager { get; }

        public event Action Resized = () => { };

        public WaitHandle BeginInvoke(Action action) {
            return null;
        }
        static uint[] colors = new[] {
            (uint)Color.Red.ToArgb(),
            (uint)Color.Green.ToArgb(),
            (uint)Color.Blue.ToArgb(),
            (uint)Color.Yellow.ToArgb(),
        };

        int index = 0;
        public void EndFrame(IGraphicsDevice device) {

            var sw = new Stopwatch();
            sw.Start();

            var texture = device.GetBackBuffer();

            SharpDX.DataStream dataStream;
            SharpDX.DataRectangle db;
            int w = texture.Description.Width;
            int h = texture.Description.Height;
            {
                var desc = new Texture2DDescription {
                    Width = (int)texture.Description.Width,
                    Height = (int)texture.Description.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = texture.Description.Format,
                    Usage = ResourceUsage.Staging,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.None
                };
                var textureCopy = new Texture2D(device.D3DDevice, desc);
                device.D3DDevice.ImmediateContext.CopyResource(texture, textureCopy);

                using (var surface = textureCopy.QueryInterface<SharpDX.DXGI.Surface>()) {
                    db = surface.Map(SharpDX.DXGI.MapFlags.Read, out dataStream);
                }
                textureCopy.Dispose();
            }
            var writeableBitmap = new WriteableBitmap(w, h,
                     96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            writeableBitmap.Lock();

            unsafe {
                uint* wbb = (uint*)writeableBitmap.BackBuffer;

                dataStream.Position = 0;
                for (int y = 0; y < h; y++) {
                    dataStream.Position = y * db.Pitch;
                    for (int x = 0; x < w; x++) {
                        var c = dataStream.Read<uint>();
                        wbb[y * w + x] = c;
                    }
                }
            }
            dataStream.Dispose();

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, texture.Description.Width, texture.Description.Height));
            writeableBitmap.Unlock();

            writeableBitmap.Freeze();
            sw.Stop();

            Debug.WriteLine(sw.ElapsedMilliseconds);

            overlay.Dispatcher.Invoke(() => {
                //System.Windows.Controls.Image surface
                surface.Source = writeableBitmap;
            });
        }

        public void EndFrame1(IGraphicsDevice device) {
            //surface.Lock();
            //surface.AddDirtyRect(new Int32Rect(0, 0, surface.PixelWidth, surface.PixelHeight));
            //surface.Unlock();
            //surface.Dispatcher.Invoke(() => {
            //    //surface.End();
            //});  

            overlay.Dispatcher.Invoke(() => {
                var texture = device.GetBackBuffer();
                // var btm = SharpDX.WIC.Bitmap.FromPointer<SharpDX.WIC.Bitmap>(tex.NativePointer);
                // var frame = new SharpDX.WIC.BitmapFrameDecode(tex.NativePointer);
                //  var wic = new WicBitmapSource(btm);

                //    surface.Source = wic;

                //var bm = System.Windows.Media.Imaging.BitmapSource.Create(
                //    tex.Description.Width, tex.Description.Height, 96, 96,
                //    System.Windows.Media.PixelFormats.Pbgra32, null,
                //   tex.NativePointer, btm.Size.Width, btm.Size.Width);
                Texture2D textureCopy;
                SharpDX.DataStream dataStream;
                SharpDX.WIC.Bitmap bitmap;
                {
                    var desc = new Texture2DDescription {
                        Width = (int)texture.Description.Width,
                        Height = (int)texture.Description.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = texture.Description.Format,
                        Usage = ResourceUsage.Staging,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = BindFlags.None,
                        CpuAccessFlags = CpuAccessFlags.Read,
                        OptionFlags = ResourceOptionFlags.None
                    };
                    textureCopy = new Texture2D(device.D3DDevice, desc);
                    device.D3DDevice.ImmediateContext.CopyResource(texture, textureCopy);
                    var dataBox = device.D3DDevice.ImmediateContext.MapSubresource(
                        textureCopy,
                        0,
                        0,
                        MapMode.Read,
                        global::SharpDX.Direct3D11.MapFlags.None,
                        out dataStream);

                    var dataRectangle = new SharpDX.DataRectangle {
                        DataPointer = dataStream.DataPointer,
                        Pitch = dataBox.RowPitch
                    };
                    using (var factory = new SharpDX.WIC.ImagingFactory()) {
                        bitmap = new SharpDX.WIC.Bitmap(factory, textureCopy.Description.Width, textureCopy.Description.Height,
                            SharpDX.WIC.PixelFormat.Format32bppRGBA, dataRectangle, 0);
                    }
                }
                var f = texture.Description.Format;

                var writeableBitmap = new WriteableBitmap(texture.Description.Width, texture.Description.Height,
                   96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

                //texture.Description.Width = 304
                // texture.Description.Height = 585

                writeableBitmap.Lock();
                var count = texture.Description.Width * texture.Description.Height
                    * System.Windows.Media.PixelFormats.Bgra32.BitsPerPixel / 8;

                int bytesPerPixel = (writeableBitmap.Format.BitsPerPixel + 7) / 8;
                int stride = texture.Description.Width * bytesPerPixel;
                count = writeableBitmap.PixelHeight * writeableBitmap.PixelWidth;

                //count = 4 * writeableBitmap.PixelHeight * writeableBitmap.PixelWidth;
                stride = writeableBitmap.BackBufferStride;
                // Get a pointer to the back buffer.
                unsafe {

                    uint* pBackbuffer = (uint*)writeableBitmap.BackBuffer;
                    //   uint darkcolorPixel = 0xffaaaaaa;
                    var color = colors[index];

                    //using (var fac = new SharpDX.WIC.ImagingFactory()) {
                    //    var palette = new SharpDX.WIC.Palette(fac);
                    //    frame.CopyPalette(palette);
                    //    var data = palette.GetColors<uint>();
                    //    for (int i = 0; i < count; i++)
                    //        pBackbuffer[i] = data[i];

                    //}

                    
                    uint* data = (uint*)dataStream.DataPointer;
                    //for (int i = 0; i < count; i++)
                    //    pBackbuffer[i] = data[i];
                    var i = 0;
                    for (var y = 0; y < writeableBitmap.PixelHeight; y++) {
                        color = colors[index];
                        for (var x = 0; x < writeableBitmap.PixelWidth; x++) {
                            pBackbuffer[x + (y * writeableBitmap.PixelWidth)] = data[i]; i++;
                        }
                        index++;
                        if (index == colors.Length) {
                            index = 0;
                        }
                    }

                    //for (var x = 0; x < writeableBitmap.PixelWidth; x++){
                    //    color = colors[index];
                    //    for (var y = 0; y < writeableBitmap.PixelHeight; y++) {
                    //        pBackbuffer[y + (x * writeableBitmap.PixelHeight)] = data[i];
                    //        i++;
                    //    }
                    //    index++;
                    //    if (index == colors.Length) {
                    //        index = 0;
                    //    }
                    //}

                    //var data = device.CopyBackBufferMemoryStream().ToArray();
                    //for (int i = 0; i < data.Length; i++)
                    //    pBackbuffer[i] = data[i];

                    //System.IO.File.WriteAllBytes(@"C:\Storage\test.png",data);


                    //System.Buffer.MemoryCopy(tex.NativePointer.ToPointer(), writeableBitmap.BackBuffer.ToPointer(),
                    //    count, count);


                }

                //writeableBitmap.WritePixels(new Int32Rect(0,0, tex.Description.Width, tex.Description.Height),
                //    tex.NativePointer, count, stride);


                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, texture.Description.Width, texture.Description.Height));

                writeableBitmap.Unlock();
                //var wic = new WicBitmapSource1(@"C:\Storage\5lzrcejm7cert584fgerxwtj54o.jpeg");
                //surface.Source = wic;

                surface.Source = writeableBitmap;
                index++;
                if (index == colors.Length) {
                    index = 0;
                }

                textureCopy.Dispose();
                dataStream.Dispose();
                bitmap.Dispose();
            });
        }

        public Texture2D Bitmap2Texture(Device device, BitmapSource source) {
            var bitmap = new WriteableBitmap(source);
            bitmap.Lock();
            var texture = new Texture2D(device, new Texture2DDescription() {
                Width = source.PixelWidth,
                Height = source.PixelHeight,
                ArraySize = 1,
                MipLevels = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            }, new SharpDX.DataRectangle(bitmap.BackBuffer, bitmap.BackBufferStride));
            bitmap.Unlock();
            return texture;
        }

        public void SetTitleText(string txt) {


        }

        public void StartFrame(IGraphicsDevice device) {
            overlay.Dispatcher.Invoke(() => {
                var tex = device.GetBackBuffer();



                // surface.Begin(tex);



                return;


                //int rawStride = (tex.Description.Width * System.Windows.Media.PixelFormats.Pbgra32.BitsPerPixel + 7) / 8;

                //var bm = System.Windows.Media.Imaging.BitmapSource.Create(
                //    tex.Description.Width, tex.Description.Height, 96, 96,
                //    System.Windows.Media.PixelFormats.Pbgra32, null,
                //    tex.NativePointer, tex.Description.Width * tex.Description.Height * System.Windows.Media.PixelFormats.Pbgra32.BitsPerPixel, rawStride);

            });
        }

    }
}
