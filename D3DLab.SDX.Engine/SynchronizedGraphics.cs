using D3DLab.ECS;
using D3DLab.SDX.Engine.Rendering;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine {
    public interface IFrameRenderableSurface: IRenderableSurface {
        void StartFrame(GraphicsDevice device);
        void EndFrame(GraphicsDevice device);
        void Dispose();
    }

    public class SynchronizedGraphics : ISynchronizationContext {
        struct Size {
            public float Width;
            public float Height;
        }
        internal event Action<GraphicsDevice> Changed;
        public readonly GraphicsDevice Device;
        readonly IRenderableSurface surface;
        readonly SynchronizationContext<SynchronizedGraphics, Size> synchronizer;

        public bool IsChanged => synchronizer.IsChanged;

        public SynchronizedGraphics(IRenderableSurface surface, GraphicsDevice device) {
            Device = device;
            surface.Resized += OnResized;
            surface.Invalidated += OnInvalidated;
            this.surface = surface;
            synchronizer = new SynchronizationContext<SynchronizedGraphics, Size>(this);
        }
        [Obsolete("",true)]
        public SynchronizedGraphics(IRenderableWindow surface) {
            Device = GraphicsDeviceFactory.CreateOutputHandleDevice(surface);
            surface.Resized += OnResized;
            surface.Invalidated += OnInvalidated;
            this.surface = surface;
            synchronizer = new SynchronizationContext<SynchronizedGraphics, Size>(this); 
        }
        [Obsolete("",true)]
        public SynchronizedGraphics(IFrameRenderableSurface surface) {
            Device = GraphicsDeviceFactory.CreateOutputTargetView(surface);//CreateOutputTextureDevice
            surface.Resized += OnResized;
            surface.Invalidated += OnInvalidated;
            this.surface = surface;
            synchronizer = new SynchronizationContext<SynchronizedGraphics, Size>(this);
        }
        //public SynchronizedGraphics(IFrameRenderableSurface surface) 
        //    : this(GraphicsDeviceFactory.CreateOutputTargetView(surface), surface) {
        //}
        SynchronizedGraphics(GraphicsDevice device, IFrameRenderableSurface surface) {
            Device = device;
            surface.Resized += OnResized;
            surface.Invalidated += OnInvalidated;
            this.surface = surface;
            synchronizer = new SynchronizationContext<SynchronizedGraphics, Size>(this);
        }


        private void OnResized() {
            synchronizer.Add((_this, size) => {
                _this.Device.Resize(size.Width, size.Height);
                Changed(_this.Device);
            }, new Size { Height = surface.Height, Width = surface.Width });
        }

        private void OnInvalidated() {
            synchronizer.Add((_, __) => { }, new Size());
        }

        public void Dispose() {
            Device.Dispose();
        }

        public void Synchronize(int theadId) {
            synchronizer.Synchronize(theadId);
        }

        public GraphicsFrame FrameBegin() {
            switch (surface) {
                case IFrameRenderableSurface surf:
                    return new GraphicsFrameWithSurface(this.Device, surf);
                default:
                    return new GraphicsFrame(this.Device);
            }
        }

        public void GetBackBufferBitmapInvokeAsync(Action<System.Drawing.Bitmap> callback) {
            synchronizer.Add((_this, size) => {
                var btm = this.Device.CopyBackBufferTexture();
                Task.Run(()=>callback.Invoke(btm));
            }, new Size());
        }
    }
}
