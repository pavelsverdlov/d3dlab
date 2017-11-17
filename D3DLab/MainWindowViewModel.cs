using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core;
using D3DLab.Core.Components;
using D3DLab.Core.Host;
using D3DLab.Core.Viewport;
using D3DLab.Core.Visual3D;

namespace D3DLab {
    public sealed class MainWindowViewModel {
        private D3DEngine engine;
        private readonly ViewportSubscriber subscriber;

        public MainWindowViewModel() {
            subscriber = new ViewportSubscriber(this);
        }

        public void Init(FormsHost host) {
            engine = new D3DEngine(host);
            engine.Notificator.Subscribe(subscriber);
        }

    }

    public sealed class ViewportSubscriber : IViewportAddSubscriber<ComponentContainer> {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Add(ComponentContainer visual) {

        }
    }
}
