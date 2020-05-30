using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace D3DLab.ECS {
    public interface IEngineSubscriber { }
    public interface IManagerChangeSubscriber<T> : IEngineSubscriber {
        void Add(ref T obj);
        void Remove(ref T obj);
    }

    public interface IEntityRenderSubscriber : IEngineSubscriber {
        void Render(IEnumerable<GraphicEntity> entities);
    }

    public interface IEngineSubscribe {
        void Subscribe(IEngineSubscriber s);
    }

    public interface IManagerChangeNotify {
        void NotifyAdd<T>(T _object);

        void NotifyAdd<T>(ref T _object);
        void NotifyRemove<T>(ref T _object);

    }
    public interface IEntityRenderNotify {
        void NotifyRender(IEnumerable<GraphicEntity> entities);
    }

    public sealed class EngineNotificator : IEngineSubscribe, IManagerChangeNotify, IEntityRenderNotify {
        private readonly List<IEngineSubscriber> subscribers;
        readonly Task runner;
        public EngineNotificator() {
            this.subscribers = new List<IEngineSubscriber>();
            runner = Task.CompletedTask;
        }

        public void Subscribe(IEngineSubscriber s) {
            subscribers.Add(s);
        }

        public void NotifyRender(IEnumerable<GraphicEntity> entities) {
            var handlers = subscribers.OfType<IEntityRenderSubscriber>();
            foreach (var handler in handlers) {
                try {
                    handler.Render(entities);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
        }
        public void NotifyAdd<T>(T _object) {
            var handlers = subscribers.OfType<IManagerChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                try {
                    handler.Add(ref _object);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
        }

        public void NotifyAdd<T>(ref T _object) {
            var local = _object;
            //runner.ContinueWith(x => {
            var handlers = subscribers.OfType<IManagerChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                try {
                    handler.Add(ref local);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
            //});
        }

        public void NotifyRemove<T>(ref T _object) {
            var local = _object;
            //runner.ContinueWith(x => {
            var handlers = subscribers.OfType<IManagerChangeSubscriber<T>>();
            foreach (var handler in handlers) {
                try {
                    handler.Remove(ref local);
                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
#if DEBUG
                    throw ex;
#endif
                }
            }
            //});
        }
    }
}
