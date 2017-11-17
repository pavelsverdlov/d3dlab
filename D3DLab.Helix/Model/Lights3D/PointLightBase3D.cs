using System;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX.Render;
using global::SharpDX;

namespace HelixToolkit.Wpf.SharpDX
{
	public abstract class PointLightBase3D : Light3D
	{
		public static readonly DependencyProperty AttenuationProperty =
			DependencyProperty.Register("Attenuation", typeof(Vector3), typeof(PointLightBase3D), new UIPropertyMetadata(new Vector3(1.0f, 0.0f, 0.0f)));

		public static readonly DependencyProperty RangeProperty =
			DependencyProperty.Register("Range", typeof(double), typeof(PointLightBase3D), new UIPropertyMetadata(1000.0));

		private static readonly DependencyPropertyKey PositionPropertyKey =
			DependencyProperty.RegisterReadOnly("Position", typeof(Point3D), typeof(PointLightBase3D), new UIPropertyMetadata(new Point3D()));

		public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

		/// <summary>
		/// Initializes a new instance of the <see cref="PointLightBase3D"/> class.
		/// </summary>
		public PointLightBase3D()
		{
		}

		/// <summary>
		/// The position of the model in world space.
		/// </summary>
		public Point3D Position
		{
			get { return (Point3D)this.GetValue(PositionProperty); }
			private set { this.SetValue(PositionPropertyKey, value); }
		}

		protected override void OnTransformChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnTransformChanged(e);
			this.Position = this.modelMatrix.TranslationVector.ToPoint3D();
		}

		/// <summary>
		/// Attenuation coefficients:
		/// X = constant attenuation,
		/// Y = linar attenuation,
		/// Z = quadratic attenuation.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb172279(v=vs.85).aspx
		/// </summary>
		public Vector3 Attenuation
		{
			get { return (Vector3)this.GetValue(AttenuationProperty); }
			set { this.SetValue(AttenuationProperty, value); }
		}

		/// <summary>
		/// Range of this light. This is the maximum distance 
		/// of a pixel being lit by this light.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb172279(v=vs.85).aspx
		/// </summary>
		public double Range
		{
			get { return (double)this.GetValue(RangeProperty); }
			set { this.SetValue(RangeProperty, value); }
		}

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (PointLightRenderData)RenderData;
			renderData.Attenuation = Attenuation;
			renderData.Range = (float)Range;
			renderData.Position = Position.ToVector3();
		}
	}
}
