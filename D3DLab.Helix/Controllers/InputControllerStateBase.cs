using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX.StateMachine.Core;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public abstract class InputControllerStateBase : StateBase<InputControllerStateContext>, IInputSupport
	{
		public InputControllerStateBase(InputController controller)
			: base(controller)
		{
		}

		new public InputController Controller { get { return (InputController)base.Controller; } }

		public virtual bool OnMouseDown(MouseEventArgs ea)
		{
			return false;
		}

		public virtual bool OnMouseUp(MouseEventArgs ea)
		{
			return false;
		}

		public virtual bool OnMouseWheel(MouseEventArgs ea)
		{
			Controller.CameraViewController.Zoom(ea.Delta, ea.X, ea.Y);
			return false;
		}

		public virtual bool OnMouseMove(MouseEventArgs ea)
		{
			return false;
		}

		public virtual bool OnMouseDoubleClick(MouseEventArgs ea)
		{
			return false;
		}

		public bool OnKeyDown(KeyEventArgs e)
		{
			return false;
		}
		public bool OnKeyUp(KeyEventArgs e)
		{
			return false;
		}
		
		protected static double Distance(System.Drawing.Point p1, System.Drawing.Point p2)
		{
			var x = p2.X - p1.X;
			var y = p2.Y - p1.Y;
			return Math.Sqrt(x * x + y * y);
		}

		protected static bool IsPressed(MouseButtons button)
		{
			return (Control.MouseButtons & button) == button;
		}
	}
}
