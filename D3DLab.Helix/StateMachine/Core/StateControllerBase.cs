using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HelixToolkit.Wpf.SharpDX.StateMachine.Core
{
	public abstract class StateControllerBase<TContext, TListener, TListenerToEvents> : IStateControllerBase<TContext>
		where TListenerToEvents : TListener, new()
	{
		private StateControllerBase()
		{
			throw new NotImplementedException();
		}

		private readonly Enum generalState;
		public StateControllerBase(TContext context, Enum generalState)
		{
			this.generalState = generalState;
			this.context = context;

			Listeners = new List<TListener>();
			Events = new TListenerToEvents();
			Listeners.Add(Events);

			PreviewListeners = new List<TListener>();
			PreviewEvents = new TListenerToEvents();
			PreviewListeners.Add(PreviewEvents);

			SwitchToState(generalState);
		}

		protected StateBase<TContext> CurrentState { get; private set; }
		internal readonly TContext context;

		public TContext Context
		{
			get { return context; }
		}

		public List<TListener> PreviewListeners { get; private set; }

		public List<TListener> Listeners { get; private set; }

		public bool SwitchToState(Enum newStateId)
		{
			if (CurrentState != null && object.Equals(CurrentState.Id, newStateId))
				return false;

			var newState = CreateState(newStateId);

			if (CurrentState != null)
				CurrentState.LeaveState(newState);

			var prevState = CurrentState;
			CurrentState = newState;
			newState.EnterState(prevState);

			OnStateChanged(prevState != null ? prevState.Id : null, newState != null ? newState.Id : null);

			return true;
		}

		private static readonly Dictionary<object, Type> stateTypes = new Dictionary<object, Type>();
		private StateBase<TContext> CreateState(Enum stateId)
		{
			Type type;
			if (!stateTypes.TryGetValue(stateId, out type))
			{
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

			var newState = (StateBase<TContext>)Activator.CreateInstance(type, this);
			newState.Id = stateId;
			return newState;
		}

		private bool active = true;
		public virtual bool Active
		{
			get { return active; }
			set
			{
				if (active == value)
					return;
				active = value;
				if (!value)
					SwitchToState(generalState);
			}
		}

		protected void DoEvent(object sender, Func<TListener, bool> handler)
		{
			foreach (var listener in PreviewListeners)
			{
				if (handler(listener))
					return;
			}

			if (Active)
			{
				Debug.Assert(CurrentState != null);
				if (CurrentState != null && handler((TListener)(object)CurrentState))
					return;
			}

			foreach (var listener in Listeners)
				handler(listener);
		}

		public TListenerToEvents Events { get; private set; }

		public TListenerToEvents PreviewEvents { get; private set; }

        public event EventHandler<InputStateChangedEventArgs> StateChanged;
		protected virtual void OnStateChanged(Enum prevStateId, Enum newStateId)
		{
			if (StateChanged != null)
				StateChanged(this, new InputStateChangedEventArgs(prevStateId, newStateId));
		}
	}
}
