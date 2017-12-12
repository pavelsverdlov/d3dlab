using System;
using System.Collections.Generic;
using System.Linq;
using D3DLab.Core.Components;
using D3DLab.Core.Test;
using D3DLab.Core.Visual3D;

namespace D3DLab.Core.Viewport {
    public interface IViewportSubscriber { }
    public interface IViewportChangeSubscriber<in T> : IViewportSubscriber{
        void Add(T visual);
    }

    public interface IViewportRenderSubscriber : IViewportSubscriber {
        void Render(IEnumerable<Entity> entities);
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

        public void NotifyChange<T>(T _object) where T : class{
            var handlers = subscribers.OfType<IViewportChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                handler.Add(_object);
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