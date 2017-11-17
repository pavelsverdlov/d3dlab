using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;
using Point = System.Windows.Point;

namespace HelixToolkit.Wpf.SharpDX
{
	public class MouseUp3DEventArgs : Mouse3DEventArgs
	{
		public MouseUp3DEventArgs(object source, HitTestResult hitTestResult, Point position, Viewport3DX viewport = null)
			: base(GeometryModel3D.MouseUp3DEvent, source, hitTestResult, position, viewport)
		{ }
	}
}
