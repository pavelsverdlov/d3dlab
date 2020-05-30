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
            var width = (int)window.Width;
            var height = (int)window.Height;
            var proxy = new RenderToHandleDeviceProxy(GetAdapter(), window.Handle, width, height);
            return new GraphicsDevice(proxy, width, height);
        }

        public static GraphicsDevice CreateOutputTextureDevice(IRenderableSurface window) {
            var width = (int)window.Width;
            var height = (int)window.Height;
            var proxy = new RenderToTextureDeviceProxy(GetAdapter(), width, height);
            return new GraphicsDevice(proxy, width, height);
        }

        public static GraphicsDevice CreateOutputTargetView(IFrameRenderableSurface surface) {
            var width = (int)surface.Width;
            var height = (int)surface.Height;
            var proxy = new RenderToTargetViewDeviceProxy(GetAdapter(), width, height);
            return new GraphicsDevice(proxy, width, height);
        }


    }

}
