using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	internal class PerspectiveCameraViewControllerHandler : CameraViewControllerHandler<PerspectiveCamera>
	{
		public PerspectiveCameraViewControllerHandler(CameraViewController controller)
			: base(controller)
		{
		}

		protected override float PanK
		{
			get { return 1; }
		}

		public override void Zoom(int delta, int x, int y)
		{
			if (delta == 0)
				return;
		}

		public override void Zoom(float delta)
		{
		}
	}
}
