using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX.StateMachine.Core;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateCustomMove : InputControllerStateBase
	{
		public InputControllerStateCustomMove(InputController manager) : base(manager) { }

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Right)
				return SwitchToState(InputControllerState.Pan);

			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
			return SwitchToState(InputControllerState.General);
		}

		public override bool OnMouseMove(MouseEventArgs ea)
		{
			Controller.OnCustomMove(customMoveTarget, ea);
			return true;
		}

		object customMoveTarget;
		protected override void OnEnterState(StateBase<InputControllerStateContext> fromState)
		{
			base.OnEnterState(fromState);
			var ea = Context.DownPointLeft.ToMouseArgsAllow();
			customMoveTarget = Controller.OnCustomMoveBegin(ea);
			if (!ea.Allow)
				SwitchToState(InputControllerState.DownLeft);
		}

		protected override void OnLeaveState(StateBase<InputControllerStateContext> toState)
		{
			Controller.OnCustomMoveEnd(customMoveTarget, Context.PrevMovePoint.ToMouseArgs(MouseButtons.Left));
			base.OnLeaveState(toState);
		}
	}
}
