using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.StateMachine.Core
{
	[AttributeUsageAttribute(AttributeTargets.Field, AllowMultiple = false)]
	public class StateTypeAttribute : Attribute
	{
		public StateTypeAttribute(Type stateType)
		{
			Type = stateType;
		}

		public Type Type { get; private set; }
	}
}
