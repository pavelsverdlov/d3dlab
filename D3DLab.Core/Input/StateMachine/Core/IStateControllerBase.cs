using System;

namespace D3DLab.Core.Input.StateMachine.Core
{
	public interface IStateControllerBase<TContext>
	{
		TContext Context { get; }
		bool SwitchToState(Enum newStateId);
	}
}
