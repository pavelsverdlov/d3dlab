using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using Point3D = System.Windows.Media.Media3D.Point3D;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateGeneral : InputControllerStateBase
	{
		public InputControllerStateGeneral(InputController manager) : base(manager) { }

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Left)
				SwitchToState(InputControllerState.DownLeft);

			if (ea.Button == MouseButtons.Right)
				SwitchToState(InputControllerState.DownRight);

            if(ea.Button == MouseButtons.Middle)
                SwitchToState(InputControllerState.DownMiddle);

			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
//			if (ea.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None)
//			{
//				Point3D? point;
//				if (!Controller.OnGetRotateCenter(ea, out point))
//				{
//					var hits = Controller.Viewport.FindHits(new System.Windows.Point(ea.X, ea.Y));
//					if (hits != null)
//					{
//						var hit = hits.Where(i => i.IsValid && i.ModelHit.Visible).FirstOrDefault();
//						if (hit.IsValid)
//							point = hit.PointHit;
//					}
//				}
//				if (point != null)
//				{
//					Controller.CameraViewController.IsManuallyChanged = true;
//					try
//					{
//						Controller.CameraViewController.RotateCenter = point.Value;
//						var p = new Vector2(ea.X, ea.Y);
//						var p0 = new Vector2((float)Controller.Viewport.ActualWidth * 0.5f, (float)Controller.Viewport.ActualHeight * 0.5f);
//						var v = p0 - p;
//						Controller.CameraViewController.Pan((int)v.X, (int)v.Y);
//					}
//					finally
//					{
//						Controller.CameraViewController.IsManuallyChanged = false;
//					}
//				}
//			}

			return base.OnMouseUp(ea);
		}

		public override bool OnMouseMove(MouseEventArgs ea)
		{
			return base.OnMouseMove(ea);
		}
	}
}
