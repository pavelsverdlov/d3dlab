using System;

namespace D3DLab.Core.Input.StateMachine.Core
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class StateTypeAttribute : Attribute
	{
		public StateTypeAttribute(Type stateType)
		{
			Type = stateType;
		}

		public Type Type { get; private set; }
	}
}
