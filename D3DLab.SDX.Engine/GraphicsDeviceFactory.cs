using D3DLab.ECS;
using D3DLab.SDX.Engine.ProxyDevice;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine {
    public static class GraphicsDeviceFactory {

        static Adapter GetAdapter() {
            var factory = new Factory1();
            return AdapterFactory.GetBestAdapter(factory);
        }

        public static GraphicsDevice CreateOutputHandleDevice(IRenderableWindow window) {
            var proxy = new RenderToHandleDeviceProxy(GetAdapter(), window.Handle, window.Size);
            return new GraphicsDevice(proxy, window.Size);
        }

        public static GraphicsDevice CreateOutputTextureDevice(IRenderableSurface window) {
            var proxy = new RenderToTextureDeviceProxy(GetAdapter(), window.Size);
            return new GraphicsDevice(proxy, window.Size);
        }

        public static GraphicsDevice CreateOutputTargetView(IFrameRenderableSurface surface) {
            var proxy = new RenderToTargetViewDeviceProxy(GetAdapter(),surface.Size);
            return new GraphicsDevice(proxy, surface.Size);
        }


    }

}
