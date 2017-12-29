using System.Collections.Generic;
using System.Linq;
using D3DLab.Core.Entities;
using D3DLab.Core.Test;

namespace D3DLab.Core.Viewport {
    public interface IViewportSubscriber { }
    public interface IViewportChangeSubscriber<in T> : IViewportSubscriber{
        void Change(T visual);
    }

    public interface IViewportRenderSubscriber : IViewportSubscriber {
        void Render(IEnumerable<Entity> entities);
    }

    public interface IViewportSubscribe {
        void Subscribe(IViewportSubscriber s);
    }

    public interface IViewportChangeNotify {
        void NotifyChange<T>(T _object) where T : class;
    }
    public interface IViewportRendeNotify {
        void NotifyRender(IEnumerable<Entity> entities);
    }

    public sealed class ViewportNotificator : IViewportSubscribe, IViewportChangeNotify , IViewportRendeNotify {
        private readonly List<IViewportSubscriber> subscribers;
        public ViewportNotificator() {
            this.subscribers = new List<IViewportSubscriber>();
        }

        public void Subscribe(IViewportSubscriber s) {
            subscribers.Add(s);
        }

        public void NotifyChange<T>(T _object) where T : class{
            var handlers = subscribers.OfType<IViewportChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                handler.Change(_object);
            }
        }
        public void NotifyRender(IEnumerable<Entity> entities) {
            var handlers = subscribers.OfType<IViewportRenderSubscriber>();
            foreach (var handler in handlers) {
                handler.Render(entities);
            }
        }
    }
}