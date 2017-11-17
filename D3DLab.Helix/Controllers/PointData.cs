using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;
using Point = System.Drawing.Point;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3 = global::SharpDX.Vector3;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class PointData
	{
		internal PointData(MouseButtons mouseButtons)
		{
			this.mouseButtons = mouseButtons;			
		}

		private readonly MouseButtons mouseButtons;

		Point point;
		public Point Point
		{
			get { return point; }
			set
			{
				point = value;
				time = DateTime.Now;
			}
		}

		public DateTime time;
		public DateTime Time { get { return time; } }

		public MouseEventArgs ToMouseArgs(MouseButtons button = MouseButtons.None)
		{
			if (button == MouseButtons.None)
				button = mouseButtons;
			return new MouseEventArgs(button, 1, Point.X, Point.Y, 0);
		}

		public AllowMouseEventArgs ToMouseArgsAllow(MouseButtons button = MouseButtons.None)
		{
			if (button == MouseButtons.None)
				button = mouseButtons;
			return new AllowMouseEventArgs(button, 1, Point.X, Point.Y, 0);
		}
	}
}
