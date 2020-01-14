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
    using System.Threading;
    using System.Windows.Interop;

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

        public void Present(IGraphicsDevice device) {
            //DO NOTHING IN FW
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

        public void Present(IGraphicsDevice device) {
           
        }
    }

    public class WpfD3DImageSurface : IAppWindow, ISDXSurface {
        readonly DX11ImageSource surface;
        readonly FrameworkElement overlay;

        public WpfD3DImageSurface(DX11ImageSource control, FrameworkElement overlay, CurrentInputObserver input) {
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

        public void Present(IGraphicsDevice device) {
          //  var texture = device.GetBackBuffer();
            surface.Dispatcher.InvokeAsync(() => {

                surface.InvalidateD3DImage();// ((GraphicsDevice)device).CopyBackBufferTexture().Save(@"D:\Zirkonzahn\MB_Database\back.png");

                surface.SetRenderTargetDX11(device.GetBackBuffer());
            });
        }

        public void SetTitleText(string txt) {
            
        }
    }
}
