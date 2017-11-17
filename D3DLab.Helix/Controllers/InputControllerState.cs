using System;
using System.Collections.Generic;
using System.Linq;
using HelixToolkit.Wpf.SharpDX.StateMachine.Core;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public enum InputControllerState
	{
		[StateType(typeof(InputControllerStateGeneral))]
		General,
		[StateType(typeof(InputControllerStateRotate))]
		Rotate,
		[StateType(typeof(InputControllerStatePan))]
		Pan,
		[StateType(typeof(InputControllerStateDownLeft))]
		DownLeft,
		[StateType(typeof(InputControllerStateDownRight))]
		DownRight,
        [StateType(typeof(InputControllerStateDownMiddle))]
        DownMiddle,
		[StateType(typeof(InputControllerStateCustomMove))]
		CustomMove,
	}
}
