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
	public abstract class Mouse3DEventArgs : RoutedEventArgs
	{
		public HitTestResult HitTestResult { get; private set; }
		public Viewport3DX Viewport { get; private set; }
		public Point Position { get; private set; }

		public Mouse3DEventArgs(RoutedEvent routedEvent, object source, HitTestResult hitTestResult, Point position, Viewport3DX viewport = null)
			: base(routedEvent, source)
		{
			this.HitTestResult = hitTestResult;
			this.Position = position;
			this.Viewport = viewport;
		}
	}
}
