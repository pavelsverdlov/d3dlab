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
	public class MouseMove3DEventArgs : Mouse3DEventArgs
	{
		public MouseMove3DEventArgs(object source, HitTestResult hitTestResult, Point position, Viewport3DX viewport = null)
			: base(GeometryModel3D.MouseMove3DEvent, source, hitTestResult, position, viewport)
		{ }
	}
}
