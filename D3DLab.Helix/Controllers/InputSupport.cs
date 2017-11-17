using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputSupportToEvents : IInputSupport
	{
		bool IInputSupport.OnMouseDown(MouseEventArgs ea)
		{
			var hea = new HandledMouseEventArgs(ea.Button, ea.Clicks, ea.X, ea.Y, ea.Delta);
			if (MouseDown != null)
				MouseDown(null, hea);
			return hea.Handled;
		}

		bool IInputSupport.OnMouseUp(MouseEventArgs ea)
		{
			var hea = new HandledMouseEventArgs(ea.Button, ea.Clicks, ea.X, ea.Y, ea.Delta);
			if (MouseUp != null)
				MouseUp(null, hea);
			return hea.Handled;
		}

		bool IInputSupport.OnMouseWheel(MouseEventArgs ea)
		{
			var hea = new HandledMouseEventArgs(ea.Button, ea.Clicks, ea.X, ea.Y, ea.Delta);
			if (MouseWheel != null)
				MouseWheel(null, hea);
			return hea.Handled;
		}

		bool IInputSupport.OnMouseMove(MouseEventArgs ea)
		{
			var hea = new HandledMouseEventArgs(ea.Button, ea.Clicks, ea.X, ea.Y, ea.Delta);
			if (MouseMove != null)
				MouseMove(null, hea);
			return hea.Handled;
		}

		bool IInputSupport.OnMouseDoubleClick(MouseEventArgs ea)
		{
			var hea = new HandledMouseEventArgs(ea.Button, ea.Clicks, ea.X, ea.Y, ea.Delta);
			if (MouseDoubleClick != null)
				MouseDoubleClick(null, hea);
			return hea.Handled;
		}

		bool IInputSupport.OnKeyDown(KeyEventArgs ea)
		{
			if (KeyDown != null)
				KeyDown(null, ea);
			return ea.Handled;
		}

		bool IInputSupport.OnKeyUp(KeyEventArgs ea)
		{
			if (KeyUp != null)
				KeyUp(null, ea);
			return ea.Handled;
		}

		public event EventHandler<HandledMouseEventArgs> MouseDown;
		public event EventHandler<HandledMouseEventArgs> MouseUp;
		public event EventHandler<HandledMouseEventArgs> MouseWheel;
		public event EventHandler<HandledMouseEventArgs> MouseMove;
		public event EventHandler<HandledMouseEventArgs> MouseDoubleClick;
		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;
	}
}
