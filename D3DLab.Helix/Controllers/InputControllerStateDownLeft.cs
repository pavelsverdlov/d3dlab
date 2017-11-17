using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateDownLeft : InputControllerStateBase
	{
		public InputControllerStateDownLeft(InputController manager) : base(manager) { }

		protected override void OnEnterState(StateMachine.Core.StateBase<InputControllerStateContext> fromState)
		{
			base.OnEnterState(fromState);
			canSwitchToCustomMove = !object.Equals(fromState.Id, InputControllerState.CustomMove);
			hasMove = false;
		}

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Right)
				return SwitchToState(InputControllerState.Pan);

			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Left)
			{
				if (!hasMove)
					Controller.OnClick(ea);
				Controller.SwitchToState(InputControllerState.General);
			}
			return base.OnMouseUp(ea);
		}

		bool hasMove;
		bool canSwitchToCustomMove;
		public override bool OnMouseMove(MouseEventArgs ea)
		{
			if (!hasMove)
				hasMove = Distance(Context.DownPointLeft.Point, ea.Location) > 1;
			if (canSwitchToCustomMove)
			{
				if (hasMove)
					return Controller.SwitchToState(InputControllerState.CustomMove);
			}

			return base.OnMouseMove(ea);
		}
	}
}
