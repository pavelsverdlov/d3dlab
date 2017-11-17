using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class AllowMouseEventArgs : MouseEventArgs
	{
		public AllowMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
			: base(button, clicks, x, y, delta)
		{
		}

		public bool Allow { get; set; }
	}
}
