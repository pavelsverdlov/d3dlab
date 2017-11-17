using System.Windows;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public class ShadowMap3D : Element3D
	{
		public static readonly DependencyProperty ResolutionProperty =
			DependencyProperty.Register("Resolution", typeof(Vector2), typeof(ShadowMap3D), new UIPropertyMetadata(new Vector2(1024, 1024), ResolutionChanged));

		public static readonly DependencyProperty FactorPCFProperty =
				DependencyProperty.Register("FactorPCF", typeof(double), typeof(ShadowMap3D), new UIPropertyMetadata(1.5));

		public static readonly DependencyProperty BiasProperty =
				DependencyProperty.Register("Bias", typeof(double), typeof(ShadowMap3D), new UIPropertyMetadata(0.0015));

		public static readonly DependencyProperty IntensityProperty =
				DependencyProperty.Register("Intensity", typeof(double), typeof(ShadowMap3D), new UIPropertyMetadata(0.5));

		/// <summary>
		/// Initializes a new instance of the <see cref="ShadowMap3D"/> class.
		/// </summary>
		public ShadowMap3D()
		{
		}

		private static void ResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			((ShadowMap3D)d).ReAttach();
		}

		public Vector2 Resolution
		{
			get { return (Vector2)this.GetValue(ResolutionProperty); }
			set { this.SetValue(ResolutionProperty, value); }
		}

		public double FactorPCF
		{
			get { return (double)this.GetValue(FactorPCFProperty); }
			set { this.SetValue(FactorPCFProperty, value); }
		}

		public double Bias
		{
			get { return (double)this.GetValue(BiasProperty); }
			set { this.SetValue(BiasProperty, value); }
		}

		public double Intensity
		{
			get { return (double)this.GetValue(IntensityProperty); }
			set { this.SetValue(IntensityProperty, value); }
		}

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new ShadowMapRenderData(host);
		}

		public override void Update(RenderContext renderContext, System.TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (ShadowMapRenderData)RenderData;

			renderData.Resolution = new Vector2((int)(Resolution.X + 0.5f), (int)(Resolution.Y + 0.5f));
			renderData.FactorPCF = (float)FactorPCF;
			renderData.Bias = (float)Bias;
			renderData.Intensity = (float)Intensity;
		}
	}
}
