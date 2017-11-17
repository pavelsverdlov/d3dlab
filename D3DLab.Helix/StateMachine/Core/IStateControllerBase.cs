using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.StateMachine.Core
{
	public interface IStateControllerBase<TContext>
	{
		TContext Context { get; }
		bool SwitchToState(Enum newStateId);
	}
}
