using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using HelixToolkit.Wpf.SharpDX.StateMachine.Core;
using SharpDX;
using Control = System.Windows.Forms.Control;
using Cursor = System.Windows.Forms.Cursor;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class InputControllerStateRotate : InputControllerStateBase
	{
		public InputControllerStateRotate(InputController manager) : base(manager) { }

		public override bool OnMouseDown(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Left)
				return SwitchToState(InputControllerState.Pan);

			return base.OnMouseDown(ea);
		}

		public override bool OnMouseUp(MouseEventArgs ea)
		{
			if (ea.Button == MouseButtons.Right)
				return Controller.SwitchToState(InputControllerState.General);

			return base.OnMouseUp(ea);
		}

		public override bool OnMouseMove(MouseEventArgs ea)
		{
			if (!IsPressed(MouseButtons.Right))
			{
				Controller.SwitchToState(InputControllerState.General);
				return false;
			}

			//System.Drawing.Point p = ea.Location;
			return MouseMoveCore(Controller.Control.PointToClient(Cursor.Position));
		}

		private bool MouseMoveCore(System.Drawing.Point p)
		{
			var p1 = Controller.Control.PointToClient(rotateAtPoint).ToVector2();
			var p2 = (Control.ModifierKeys & Keys.Shift) == Keys.Shift
				? p.ToVector2()
				: GetSmoothedPoint(p);

			var moveV = p2 - p1;
			if (moveV.Length() < 0.5f)
				return false;

			Cursor.Position = rotateAtPoint;

			Controller.CameraViewController.Rotate(moveV.X, moveV.Y, rotateMode);

			return true;
		}

		List<Vector2> prevMovePoints = new List<Vector2>();
		private Vector2 GetSmoothedPoint(System.Drawing.Point p)
		{
			prevMovePoints.Add(p.ToVector2());
			while (prevMovePoints.Count > 2)
				prevMovePoints.RemoveAt(0);

			var p2 = prevMovePoints.First();
			for (int i = 1; i < prevMovePoints.Count; i++)
				p2 += prevMovePoints[i];
			p2 /= (float)prevMovePoints.Count;
			return p2;
		}

		CameraRotateMode rotateMode;
		System.Drawing.Point rotateAtPoint;

		protected override void OnEnterState(StateBase<InputControllerStateContext> fromState)
		{
			base.OnEnterState(fromState);
			
			rotateMode = CameraRotateMode.RotateAroundZ;
			rotateAtPoint = Cursor.Position;
			Cursor.Hide();

			prevMovePoints.Clear();

			var rotateMargins = Controller.OnGetCustomRotateMargins();

		    if (rotateMargins.Left.IsNaN() || rotateMargins.Top.IsNaN() || rotateMargins.Bottom.IsNaN() ||
		        rotateMargins.Right.IsNaN()) {
                rotateMode = CameraRotateMode.Rotate3D;
                return;
		    }

			var w = (int)Controller.Control.Width;
			var h = (int)Controller.Control.Height;
			var rect = new global::SharpDX.Rectangle();
			rect.Left = ConvertValue(rotateMargins.Left, w);
			rect.Top = ConvertValue(rotateMargins.Top, h);
			rect.Right = ConvertValue(rotateMargins.Right, w, true);
			rect.Bottom = ConvertValue(rotateMargins.Bottom, h, true);

			var p = Context.DownPointRight.Point;

			if (rect.Contains(p.X, p.Y))
			{
				rotateMode = CameraRotateMode.Rotate3D;
				return;
			}

			if(p.X < rect.Left || p.Y < rect.Top){
				rotateMode = CameraRotateMode.RotateAroundZInverted;
			} else if(p.Y > rect.Bottom) {
				rotateMode = CameraRotateMode.RotateAroundY;
			}
			//var dx = Math.Min(Context.DownPointRight.Point.X, w - Context.DownPointRight.Point.X);
			//var dy = Math.Min(Context.DownPointRight.Point.Y, h - Context.DownPointRight.Point.Y);

			//rotateMode = dx < dy ? CameraRotateMode.RotateAroundX : CameraRotateMode.RotateAroundY;
		}

		protected override void OnLeaveState(StateBase<InputControllerStateContext> toState)
		{
			Cursor.Show();
			base.OnLeaveState(toState);
		}

		private static int ConvertValue(double value, double fullSize, bool isRightBottom = false)
		{
			if (value < 0)
			{
				var result = Math.Abs(fullSize * value);
				if (isRightBottom)
					result = fullSize - result;
				return (int)result;
			}
			return (int)value;
		}
	}

	public enum CameraRotateMode
	{
		Rotate3D,
		RotateAroundX,
		RotateAroundY,
		RotateAroundZ,
		RotateAroundZInverted,
	}
}
