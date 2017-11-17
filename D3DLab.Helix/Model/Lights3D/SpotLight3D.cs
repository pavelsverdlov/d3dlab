using System;
using System.Windows;
using global::SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class SpotLight3D : PointLightBase3D
    {
        public static readonly DependencyProperty FalloffProperty =
            DependencyProperty.Register("Falloff", typeof(double), typeof(SpotLight3D), new UIPropertyMetadata(1.0));
        
        public static readonly DependencyProperty InnerAngleProperty =
            DependencyProperty.Register("InnerAngle", typeof(double), typeof(SpotLight3D), new UIPropertyMetadata(5.0));
        
        public static readonly DependencyProperty OuterAngleProperty =
            DependencyProperty.Register("OuterAngle", typeof(double), typeof(SpotLight3D), new UIPropertyMetadata(45.0));


        /// <summary>
        /// Decay Exponent of the spotlight.
        /// The falloff the spotlight between inner and outer angle
        /// depends on this value.
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb174697(v=vs.85).aspx
        /// </summary>
        public double Falloff
        {
            get { return (double)this.GetValue(FalloffProperty); }
			set { this.SetValue(FalloffProperty, value); }
        }
        
        /// <summary>
        /// Full outer angle of the spot (Phi) in degrees
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb174697(v=vs.85).aspx
        /// </summary>
        public double OuterAngle
        {
            get { return (double)this.GetValue(OuterAngleProperty); }
			set { this.SetValue(OuterAngleProperty, value); }
        }
        
        /// <summary>
        /// Full inner angle of the spot (Theta) in degrees. 
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb174697(v=vs.85).aspx
        /// </summary>
        public double InnerAngle
        {
            get { return (double)this.GetValue(InnerAngleProperty); }
			set { this.SetValue(InnerAngleProperty, value); }
        }

        public SpotLight3D()
        {            
            this.LightType = LightType.Spot;
        }

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new SpotLightRenderData(host);
		}

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (SpotLightRenderData)RenderData;

			renderData.Falloff = (float)Falloff;
			renderData.OuterAngle = (float)OuterAngle;
			renderData.InnerAngle = (float)InnerAngle;
		}
	}
}
