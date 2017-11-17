using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace D3DLab.Core.Input.StateMachine.Core {
    public abstract class StateControllerBase<TContext, TListener> : IStateControllerBase<TContext> {
        private readonly Enum generalState;
        public StateControllerBase(TContext context, Enum generalState) {
            this.generalState = generalState;
            this.context = context;
            
            SwitchToState(generalState);
        }

        protected StateBase<TContext> CurrentState { get; private set; }
        internal readonly TContext context;

        public TContext Context {
            get { return context; }
        }

        public bool SwitchToState(Enum newStateId) {
            if (CurrentState != null && object.Equals(CurrentState.Id, newStateId))
                return false;

            var newState = CreateState(newStateId);

            if (CurrentState != null)
                CurrentState.LeaveState(newState);

            var prevState = CurrentState;
            CurrentState = newState;
            newState.EnterState(prevState);
            
            return true;
        }

        private static readonly Dictionary<object, Type> stateTypes = new Dictionary<object, Type>();
        private D3DLab.Core.Input.StateMachine.Core.StateBase<TContext> CreateState(Enum stateId) {
            Type type;
            if (!stateTypes.TryGetValue(stateId, out type)) {
                var stateIdType = stateId.GetType();
                var field = stateIdType.GetField(stateId.ToString());
                var attrib = field.GetCustomAttributes(false)
                    .OfType<StateTypeAttribute>()
                    .FirstOrDefault();
                if (attrib == null)
                    throw new ArgumentException(string.Format("Cann't find state's type for '{0}.{1}'", stateIdType.Name, stateId));

                type = attrib.Type;
                stateTypes.Add(stateId, type);
            }

            var newState = (D3DLab.Core.Input.StateMachine.Core.StateBase<TContext>)Activator.CreateInstance(type, this);
            newState.Id = stateId;
            return newState;
        }

        private bool active = true;
        public virtual bool Active {
            get { return active; }
            set {
                if (active == value)
                    return;
                active = value;
                if (!value)
                    SwitchToState(generalState);
            }
        }

        protected void DoEvent(object sender, Func<TListener, bool> handler) {
            if (Active) {
                Debug.Assert(CurrentState != null);
                if (CurrentState != null && handler((TListener)(object)CurrentState))
                    return;
            }
        }

        
    }
}
