using System;
using System.Collections.Generic;

namespace D3DLab.Core.Events {
    public interface IEventData { }
    public interface IEventSubscriber<in TEventData> 
        where TEventData : IEventData  {
        void HandleEvent(TEventData data);
    }
    public interface IEventObservable<in TSubscriber, in TEventData>
        where TEventData : IEventData
        where TSubscriber : IEventSubscriber<TEventData> {

        void Subscribe(TSubscriber subscriber);
        void UnSubscribe(TSubscriber subscriber);
        void Notify(TEventData data);
    }
    public interface IEventProvider<in TEventData> 
        where TEventData : IEventData {
        void Execute(TEventData _event);
    }


    #region Mouse
    
    public interface IMouseEventData : IEventData { }
    public interface IMouseEventSubscriber : IEventSubscriber<IMouseEventData> {}
    public interface IMouseEventProvider : IEventObservable<IMouseEventSubscriber, IMouseEventData>, IEventProvider<IMouseEventData> {}

    #endregion

//    public interface IEventObserverFactoryMethod<in TSubscriber> where TSubscriber : IEventSubscriber {
//        void AddSubscriber(TSubscriber subscriber);
//        void RemoveSubscriber(TSubscriber subscriber);
//    }


    public sealed class EventRegistry {
        private readonly Dictionary<Type, object> observables;
        private readonly Dictionary<Type, object> providers;

        public EventRegistry() {
            this.observables = new Dictionary<Type, object>();
            providers = new Dictionary<Type, object>();
        }

        public void Register<TEventData>(IEventObservable<IEventSubscriber<TEventData>, TEventData> provider) where TEventData : IEventData {
            observables.Add(typeof(IEventObservable<IEventSubscriber<TEventData>, TEventData>), provider);
            providers.Add(typeof(IEventProvider<TEventData>), provider);
        }

        public void Register<TEventData>(IEventSubscriber<TEventData> subscriber) where TEventData : IEventData {
            var type = typeof (IEventObservable<IEventSubscriber<TEventData>, TEventData>);
            if (!observables.ContainsKey(type)) {
                throw new Exception(type.ToString() + " was not resistered.");
            }
            ((IEventObservable<IEventSubscriber<TEventData>, TEventData>)observables[type])
                .Subscribe(subscriber);
        }

        public IEventProvider<TEventData> GetProvider<TEventData>() where TEventData : IEventData {
            var type = typeof(IEventObservable<IEventSubscriber<TEventData>, TEventData>);
            if (!providers.ContainsKey(type)) {
                throw new Exception(type.ToString() + " was not resistered.");
            }
            return (IEventProvider<TEventData>)providers[type];
        }

    }



}
