using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace D3DLab.Render {
    public class DX11ImageSource : D3DImage, IDisposable {
        Texture renderTarget;
        DeviceEx device;
        Direct3DEx context;
        readonly int adapterIndex;
        public DX11ImageSource() :base() {//int adapterIndex = 0
            this.adapterIndex = 0;
            this.StartD3D();

           // SharpDX.Direct3D

        }
        public void InvalidateD3DImage() {
            if (this.renderTarget != null) {
                base.Lock();
                base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                base.Unlock();
            }
        }

        public void SetRenderTargetDX11(Texture2D target) {
            if (this.renderTarget != null) {
                renderTarget.Dispose();
                renderTarget = null;
                base.Lock();
                base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                base.Unlock();
            }

            if (target == null)
                return;

            if (!IsShareable(target))
                throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");

            var format = TranslateFormat(target);
            if (format == Format.Unknown)
                throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

            var handle = GetSharedHandle(target);
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException("Handle");

            try {
                this.renderTarget = new Texture(device, target.Description.Width, target.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
                using (Surface surface = this.renderTarget.GetSurfaceLevel(0)) {
                    base.Lock();
#if NET40
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
#else
                    // "enableSoftwareFallback = true" makes Remote Desktop possible.
                    // See: http://msdn.microsoft.com/en-us/library/hh140978%28v=vs.110%29.aspx
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer, true);
#endif
                    this.AddDirtyRect(new Int32Rect(0, 0, this.PixelWidth, this.PixelHeight));
                    base.Unlock();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        void StartD3D() {
            context = new Direct3DEx();
            // Ref: https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/wpf-and-direct3d9-interoperation
            var presentparams = new PresentParameters {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                //DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = PresentInterval.Default,
                BackBufferHeight = 1,
                BackBufferWidth = 1,
                BackBufferFormat = Format.Unknown
            };

            device = new DeviceEx(context, this.adapterIndex, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
        }
        void EndD3D() {
            renderTarget.Dispose();
            device.Dispose();
            context.Dispose();
        }


        IntPtr GetSharedHandle(Texture2D sharedTexture) {
            using (var resource = sharedTexture.QueryInterface<global::SharpDX.DXGI.Resource>()) {
                IntPtr result = resource.SharedHandle;
                return result;
            }
        }
        static Format TranslateFormat(Texture2D sharedTexture) {
            switch (sharedTexture.Description.Format) {
                case global::SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return Format.A2B10G10R10;

                case global::SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return Format.A16B16G16R16F;

                case global::SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return Format.A8R8G8B8;

                case global::SharpDX.DXGI.Format.R8G8B8A8_UNorm:
                    return Format.A8B8G8R8;

                default:
                    return Format.Unknown;
            }
        }
        static bool IsShareable(Texture2D sharedTexture) {
            return (sharedTexture.Description.OptionFlags & ResourceOptionFlags.Shared) != 0;
        }

        public void Dispose() {
            EndD3D();
        }
    }


    public interface IRenderSurface {
        event Action SurfaceCreated;
        public bool IsVisible { get; }
    }
    public class Host : HwndHost, IRenderSurface {
        public event Action SurfaceCreated = () => { };
        public Window InternallWin { get; private set; }

        public Window Window => InternallWin;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent) {
            //new WindowInteropHelper(System.Windows.Application.Current.MainWindow).EnsureHandle();
            this.Loaded += InternallWin_Loaded;
            InternallWin = new System.Windows.Window();
            //InternallWin.Loaded += InternallWin_Loaded;

            InternallWin.Owner = System.Windows.Application.Current.MainWindow;
            var src = new System.Windows.Interop.WindowInteropHelper(InternallWin);
            src.EnsureHandle();
            SetParent(src.Handle, new WindowInteropHelper(InternallWin.Owner).Handle);
            SetWindowLong(src.Handle, -16, 0x40000000);


            //throw new NotImplementedException();
            return new HandleRef(null, src.Handle);
        }

        private void InternallWin_Loaded(object sender, RoutedEventArgs e) {
            SurfaceCreated();
        }

        protected override void DestroyWindowCore(HandleRef hwnd) {
            //throw new NotImplementedException();
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }

    public class FormsHost : System.Windows.Forms.Integration.WindowsFormsHost {
        public event Action<System.Windows.Forms.Control> SurfaceCreated = x => { };

        public FormsHost() {
            this.Loaded += OnLoaded;
            this.Unloaded += Unloded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            var control = new WinFormsD3DControl();
            control.HandleCreated += OnHandleCreated;
            Child = control;
        }

        private void OnHandleCreated(object sender, EventArgs e) {
            SurfaceCreated((System.Windows.Forms.Control)sender);
            //            Viewport = new D3DViewport();
            //            Viewport.Run();
        }


        private void Unloded(object sender, RoutedEventArgs e) {
            //            Viewport.Stop();
        }

    }
    public sealed class WinFormsD3DControl : System.Windows.Forms.UserControl {
        // public event Action HandleCreated = () => { };
        public WinFormsD3DControl() {
            BackColor = System.Drawing.Color.Black;
        }

        protected override void CreateHandle() {
            base.CreateHandle();
        }

        protected override void DestroyHandle() {

            base.DestroyHandle();
        }
    }
}
