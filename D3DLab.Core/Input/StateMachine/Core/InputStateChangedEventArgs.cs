using System;

namespace D3DLab.Core.Input.StateMachine.Core
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
