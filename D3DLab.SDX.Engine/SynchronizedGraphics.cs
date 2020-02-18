using D3DLab.ECS;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine {
    public interface ISDXSurface : IAppWindow {
        void StartFrame(IGraphicsDevice device);
        void EndFrame(IGraphicsDevice device);
    }

    public class GraphicsFrame : IDisposable {
        public readonly GraphicsDevice Graphics;
        readonly ISDXSurface surface;
        readonly Stopwatch sw;
        TimeSpan spendTime;

        public GraphicsFrame(GraphicsDevice device, ISDXSurface surface) {
            this.Graphics = device;
            this.surface = surface;
            sw = new Stopwatch();
            sw.Start();
            device.Refresh();
            surface.StartFrame(Graphics);
        }


        public void Dispose() {
            Graphics.Present();
            surface.EndFrame(Graphics);
            sw.Stop();
            spendTime = sw.Elapsed;
        }
        
    }
    public class SynchronizedGraphics : ISynchronizationContext {
        struct Size {
            public float Width;
            public float Height;
        }
        internal event Action<GraphicsDevice> Changed;
        public readonly GraphicsDevice Device;
        readonly ISDXSurface surface;
        readonly SynchronizationContext<SynchronizedGraphics, Size> synchronizer;

        public bool IsChanged => synchronizer.IsChanged;

        public SynchronizedGraphics(ISDXSurface surface) {
            Device = new GraphicsDevice(surface.Handle, surface.Width, surface.Height);
            surface.Resized += OnResized;
            this.surface = surface;
            synchronizer = new SynchronizationContext<SynchronizedGraphics, Size>(this); 
        }

        private void OnResized() {
            synchronizer.Add((_this, size) => {
                _this.Device.Resize(size.Width, size.Height);
                Changed(_this.Device);
            }, new Size { Height = surface.Height, Width = surface.Width });
        }

        public void Dispose() {
            Device.Dispose();
        }

        public void Synchronize(int theadId) {
            synchronizer.Synchronize(theadId);
        }

        public GraphicsFrame FrameBegin() {
            return new GraphicsFrame(this.Device, surface);
        }

        public void GetBackBufferBitmapInvokeAsync(Action<System.Drawing.Bitmap> callback) {
            synchronizer.Add((_this, size) => {
                var btm = this.Device.CopyBackBufferTexture();
                Task.Run(()=>callback.Invoke(btm));
            }, new Size());
        }
    }
}
