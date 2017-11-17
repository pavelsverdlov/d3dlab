using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	internal class OrthographicCameraViewControllerHandler : CameraViewControllerHandler<OrthographicCamera>
	{
		public OrthographicCameraViewControllerHandler(CameraViewController controller)
			: base(controller)
		{
			MinWidth = 1;
			MaxWidth = 3000;
		}

		public float Width
		{
			get { return (float)Camera.Width; }
			set
			{
				if (Camera.Width == (double)value)
					return;
				if (float.IsNaN(value))
					return;

				CheckMinMax(ref value);
				Camera.Width = value;
			}
		}

		public float MinWidth { get; set; }
		public float MaxWidth { get; set; }

		protected override float PanK
		{
			get { return (float)(Width / ViewportWidth); }
		}

		private void CheckMinMax(ref float value)
		{
			if (value < MinWidth)
				value = MinWidth;
			if (value > MaxWidth)
				value = MaxWidth;
		}

		public override void Zoom(int delta, int x, int y)
		{
			if (delta == 0)
				return;

			var kx = x / ViewportWidth;
			var ky = y / ViewportHeight;

			var p1 = new Vector2(x * PanK, y * PanK);
			var p0 = new Vector2(ViewportWidth * 0.5f * PanK, ViewportHeight * 0.5f * PanK);

			var d = 1 - delta * 0.001f;
			var prevWidth = Width;
			//Width *= d;
			var newWidth = Width * d;
			CheckMinMax(ref newWidth);
			d = newWidth / prevWidth;

			var pan = (p1 - p0) * (d - 1);

			//PanCore(pan.X, pan.Y, i => AnimationTimer.Add("pan", new AnimationVector3(Position, i, v => Position = v)));
			//AnimationTimer.Add("width", new AnimationDouble(Width, newWidth, v => Width = (float)v));
			PanCore(pan.X, pan.Y);
			Width = newWidth;
		}

		public override void Zoom(float delta)
		{
			var newWidth = Width + delta;
			CheckMinMax(ref newWidth);
			Width = newWidth;
		}
	}
}
