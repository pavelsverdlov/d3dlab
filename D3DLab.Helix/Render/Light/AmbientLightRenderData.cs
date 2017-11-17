using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	internal class AmbientLightRenderData : LightRenderData
	{
		public AmbientLightRenderData(IRenderHost host)
			: base(LightType.Ambient, Techniques.RenderPhong)
		{
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			renderContext.TechniqueContext.Variables.LightAmbient.Set(Color);
		}
	}
}
