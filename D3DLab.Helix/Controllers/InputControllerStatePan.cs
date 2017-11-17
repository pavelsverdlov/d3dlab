using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStatePan : InputControllerStateBase
	{
		public InputControllerStatePan(InputController manager) : base(manager) { }

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
		    if (ea.Button == MouseButtons.Left) {
		        SwitchToState(InputControllerState.Rotate);
		        return false;
		    }

            if(ea.Button == MouseButtons.Right) {
				SwitchToState(InputControllerState.DownLeft);
                return false;
            }
			return base.OnMouseUp(ea);
		}

		public override bool OnMouseMove(MouseEventArgs ea)
		{
			bool notLeft = !IsPressed(MouseButtons.Left);
			bool notRight = !IsPressed(MouseButtons.Right);
			if (notLeft && notRight)
			{
				Controller.SwitchToState(InputControllerState.General);
				return false;
			}
			if (notLeft)
			{
				Controller.SwitchToState(InputControllerState.DownRight);
				return false;
			}
			if (notRight)
			{
				Controller.SwitchToState(InputControllerState.DownLeft);
				return false;
			}

			var p1 = Context.PrevMovePoint.Point;
			var p2 = ea.Location;
			Controller.CameraViewController.Pan(p2.X - p1.X, p2.Y - p1.Y);
			return true;
		}
	}
}
