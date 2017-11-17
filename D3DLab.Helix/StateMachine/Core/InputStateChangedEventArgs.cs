using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelixToolkit.Wpf.SharpDX.StateMachine.Core
{
	public class InputStateChangedEventArgs
	{
		public InputStateChangedEventArgs(Enum prevStateId, Enum newStateId)
		{
			PrevStateId = prevStateId;
			NewStateId = newStateId;
		}
		public Enum PrevStateId { get; set; }
		public Enum NewStateId { get; set; }
	}
}
