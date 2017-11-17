using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateContext
	{
		public InputControllerStateContext()
		{
			DownPointLeft = new PointData(MouseButtons.Left);
			DownPointRight = new PointData(MouseButtons.Right);
			DownPointMiddle = new PointData(MouseButtons.Middle);
			PrevMovePoint = new PointData(MouseButtons.None);
		}

		public PointData DownPointLeft { get; private set; }
		public PointData DownPointRight { get; private set; }
		public PointData DownPointMiddle { get; private set; }
		public PointData DownPointLast
		{
			get
			{
				if (DownPointLeft.Time > DownPointRight.Time && DownPointLeft.Time > DownPointMiddle.Time)
					return DownPointLeft;
				else if (DownPointRight.Time > DownPointMiddle.Time)
					return DownPointRight;
				else
					return DownPointMiddle;
			}
		}

		public PointData PrevMovePoint { get; private set; }
	}
}
