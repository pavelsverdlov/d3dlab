using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.StateMachine.Core
{
	public abstract class StateBase<TContext>
	{
		protected StateBase(IStateControllerBase<TContext> controller)
		{
			Controller = controller;
		}

		public IStateControllerBase<TContext> Controller { get; private set; }
		public TContext Context { get { return Controller.Context; } }

		private Enum id;
		public Enum Id
		{
			get { return id; }
			internal set
			{
				if (id != null)
					throw new ArgumentException("Id already initialized");
				id = value;
			}
		}

		protected bool SwitchToState(Enum newState)
		{
			return Controller.SwitchToState(newState);
		}

		internal void EnterState(StateBase<TContext> fromState)
		{
			OnEnterState(fromState);
		}

		internal void LeaveState(StateBase<TContext> toState)
		{
			OnLeaveState(toState);
		}

		protected virtual void OnEnterState(StateBase<TContext> fromState) { }

		protected virtual void OnLeaveState(StateBase<TContext> toState) { }
	}
}
