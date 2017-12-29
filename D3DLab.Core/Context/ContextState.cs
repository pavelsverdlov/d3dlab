using D3DLab.Core.Test;
using D3DLab.Core.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Context {
    public interface IContextState {
        void BeginState();
        void EndState();
        IComponentManager GetComponentManager();
        IEntityManager GetEntityManager();
        IInputManager GetInutManager();
        ISystemManager GetSystemManager();
        void SwitchTo(int stateTo);
    }

    internal sealed class Managers {
        public readonly ISystemManager SManager;
        public readonly IInputManager IManager;
        public readonly IComponentManager CManager;
        public readonly IEntityManager EManager;
        internal Managers(IViewportChangeNotify notify) {
            this.SManager = new SystemManager(notify);
            this.IManager = new InputManager();
            var encom = new EntityComponentManager(notify);
            this.CManager = encom;
            this.EManager = encom;
        }
    }

    public abstract class BaseContextState : IContextState {

        protected ISystemManager SystemManager => processor.Managers.SManager;
        protected IInputManager InputManager => processor.Managers.IManager;
        protected IComponentManager ComponentManager => processor.Managers.CManager;
        protected IEntityManager EntityManager => processor.Managers.EManager;

        readonly ContextStateProcessor processor;

        public BaseContextState(ContextStateProcessor processor) {
            this.processor = processor;
        }

        public virtual void SwitchTo(int stateTo) {
            processor.SwitchTo(stateTo);
        }
        public virtual void EndState() { }
        public virtual void BeginState() { }

        public virtual IComponentManager GetComponentManager() { return ComponentManager; }
        public virtual IEntityManager GetEntityManager() { return EntityManager; }
        public virtual IInputManager GetInutManager() { return InputManager; }
        public virtual ISystemManager GetSystemManager() { return SystemManager; }
    }
    
    public sealed class ContextStateProcessor : IContextState {
        private sealed class EmptyContextState : IContextState {
            public void BeginState() {}
            public void EndState() {}
            public IComponentManager GetComponentManager() { throw new NotImplementedException();}
            public IEntityManager GetEntityManager() { throw new NotImplementedException(); }
            public IInputManager GetInutManager() { throw new NotImplementedException(); }
            public ISystemManager GetSystemManager() { throw new NotImplementedException(); }
            public void SwitchTo(int stateTo) { throw new NotImplementedException(); }
        }

        IContextState currentState;
        private readonly Dictionary<int, Func<ContextStateProcessor, IContextState>> states;

        private static Managers managers;
        private static readonly object _loker = new object();

        internal Managers Managers { get { return managers; } }

        public ContextStateProcessor(IViewportChangeNotify notify) {
            states = new Dictionary<int, Func<ContextStateProcessor, IContextState>>();
            states.Add(-1, x => new EmptyContextState());
            currentState = new EmptyContextState();

            if (managers == null) {
                lock (_loker) {
                    if (managers == null) {
                        managers = new Managers(notify);
                    }
                }
            }
        }

        public void AddState(int stateTo, Func<ContextStateProcessor, IContextState> func) {
            states.Add(stateTo, func);
        }

        public void SwitchTo(int stateTo) {
            currentState.EndState();
            currentState = states[stateTo](this);
            currentState.BeginState();
        }
        
        public IInputManager GetInutManager() {
            return currentState.GetInutManager();
        }

        public ISystemManager GetSystemManager() {
            return currentState.GetSystemManager();
        }

        public IComponentManager GetComponentManager() {
            return currentState.GetComponentManager();
        }

        public IEntityManager GetEntityManager() {
            return currentState.GetEntityManager();
        }

        public void BeginState() {
            currentState.BeginState();
        }

        public void EndState() {
            currentState.EndState();
        }
    }    
}
