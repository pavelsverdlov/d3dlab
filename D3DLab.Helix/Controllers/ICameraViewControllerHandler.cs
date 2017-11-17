using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public interface ICameraViewControllerHandler
	{
		void Pan(int dx, int dy);
		void Pan(Vector2 move);

		void Rotate(float dx, float dy, CameraRotateMode rotateMode);
		void Rotate(Vector3 axis, float angle);

		void Zoom(int delta, int x, int y);
		void Zoom(float delta);
	}
}
