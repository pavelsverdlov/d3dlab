using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateDownRight : InputControllerStateBase
	{
		public InputControllerStateDownRight(InputController manager) : base(manager) { }

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Left)
				return SwitchToState(InputControllerState.Pan);

			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Right && Keyboard.Modifiers == ModifierKeys.None)
			{
				Controller.OnContextMenu(ea);
				return Controller.SwitchToState(InputControllerState.General);
			}
			return base.OnMouseUp(ea);
		}

		public override bool OnMouseMove(MouseEventArgs ea)
		{
			if (Distance(Context.DownPointRight.Point, ea.Location) > 2 && Controller.OnAllowSwitchToState(InputControllerState.DownRight, InputControllerState.Rotate))
				return Controller.SwitchToState(InputControllerState.Rotate);

			return base.OnMouseMove(ea);
		}
	}
}
