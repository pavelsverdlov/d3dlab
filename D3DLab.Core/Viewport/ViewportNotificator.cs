using System.Collections.Generic;
using System.Linq;
using D3DLab.Core.Components;
using D3DLab.Core.Visual3D;

namespace D3DLab.Core.Viewport {
    public interface IViewportSubscriber { }
    public interface IViewportAddSubscriber<in T> : IViewportSubscriber{
        void Add(T visual);
    }

    public interface IViewportNotificator {
        void Subscribe(IViewportSubscriber s);
    }

    public sealed class ViewportNotificator : IViewportNotificator {
        private readonly List<IViewportSubscriber> subscribers;
        public ViewportNotificator() {
            this.subscribers = new List<IViewportSubscriber>();
        }

        public void Subscribe(IViewportSubscriber s) {
            subscribers.Add(s);
        }

        public void NotifyAdd<T>(T _object) where T : class{
            var handlers = subscribers.OfType<IViewportAddSubscriber<T>>();
            foreach (var handler in handlers) {
                handler.Add(_object);
            }
        }
    }
}