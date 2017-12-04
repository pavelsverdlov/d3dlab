using D3DLab.Core.Host;
using HwndExtensions.Host;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using D3DLab.Helix.Render;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace D3DLab {
    class RenderHost : HwndHostPresenter, IRenderSurface {
        public IntPtr Handle {
            get { return HwndHost.Handle; }
        }

        event Action resize;
        public event Action Resize {
            add {
                resize += value;
            }
            remove {
                resize -= value;
            }
        }

        double IRenderSurface.Width {
            get { return ActualWidth; }
        }

        double IRenderSurface.Height {
            get { return ActualHeight; }
        }

        public event Action<D3DSurface> HandleCreated = x => { };

        public RenderHost() {
            HwndHost = new D3DSurfaceHost();

            //this.Loaded += OnLoaded;
            //this.Unloaded += Unloded;

            RegisterToAppShutdown();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            resize?.Invoke();
        }

        //private void OnLoaded(object sender, RoutedEventArgs e) {
        //    var control = new WinFormsD3DControl();
        //    control.HandleCreated += OnHandleCreated;
        //    Child = control;
        //}

        //private void OnHandleCreated(object sender, EventArgs e) {
        //    HandleCreated((WinFormsD3DControl)sender);
        //    //            Viewport = new D3DViewport((System.Windows.Forms.Control)sender);
        //    //            Viewport.Run();
        //}


        //private void Unloded(object sender, RoutedEventArgs e) {
        //    //            Viewport.Stop();
        //}

        //protected override void OnRender(DrawingContext drawingContext) {
        //    base.OnRender(drawingContext);
        //}

        private void RegisterToAppShutdown() {
            Application.Current.Dispatcher.ShutdownStarted += OnShutdownStarted;
        }

        private void OnShutdownStarted(object sender, EventArgs e) {
            Dispose();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                var host = HwndHost;
                if (host != null)
                    host.Dispose();
            }
        }

        class D3DSurfaceHost : WindowsFormsHost {
            public D3DSurface Surface { get; private set; }
            public event Action<D3DSurface> HandleCreated = x => { };

            public D3DSurfaceHost() {
                this.Loaded += OnLoaded;
                this.Unloaded += Unloded;
            }

            void OnLoaded(object sender, RoutedEventArgs e) {
                var control = new D3DSurface();
                control.HandleCreated += OnHandleCreated;
                Child = control;
            }

            void OnHandleCreated(object sender, EventArgs e) {
                HandleCreated?.Invoke((D3DSurface)sender);
            }


            void Unloded(object sender, RoutedEventArgs e) { }

            protected override void OnRender(DrawingContext drawingContext) {
                base.OnRender(drawingContext);
            }
        }
    }
}
