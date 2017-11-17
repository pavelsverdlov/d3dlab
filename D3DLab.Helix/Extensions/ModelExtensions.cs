using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HelixToolkit.Wpf.SharpDX.Extensions
{
	public static class ModelExtensions
	{
		public static Matrix GetTransform(this Model3D visual)
		{
			var totalTransform = Matrix.Identity;

			DependencyObject obj = visual;
			while (obj != null)
			{
				var viewport3DVisual = obj as Viewport3DX;
				if (viewport3DVisual != null)
				{
					return totalTransform;
				}

				var mv = obj as Model3D;
				if (mv != null)
					totalTransform *= mv.ModelMatrix;

				obj = System.Windows.Media.VisualTreeHelper.GetParent(obj);
			}

			return totalTransform;
			//throw new InvalidOperationException("The visual is not added to a Viewport3D.");
		}
	}
}
