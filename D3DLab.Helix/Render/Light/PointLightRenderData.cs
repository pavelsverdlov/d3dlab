using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	internal class PointLightRenderData : LightRenderData
	{
		public PointLightRenderData(IRenderHost host)
			: base( LightType.Point, Techniques.RenderPhong)
		{
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			/// --- turn-on the light            
			renderContext.LightContext.lightColors[this.lightIndex] = Color * (float)renderContext.IlluminationSettings.Light;
			/// --- Set light type
			renderContext.LightContext.lightTypes[lightIndex] = (int)Light3D.Type.Point;

			/// --- Set lighting parameters
			renderContext.LightContext.lightPositions[lightIndex] = Position.ToVector4();
			renderContext.LightContext.lightAtt[lightIndex] = new Vector4(Attenuation, Range);

			/// --- Update lighting variables    
			renderContext.TechniqueContext.Variables.LightPos.Set(renderContext.LightContext.lightPositions);
			renderContext.TechniqueContext.Variables.LightColor.Set(renderContext.LightContext.lightColors);
			renderContext.TechniqueContext.Variables.LightAtt.Set(renderContext.LightContext.lightAtt);
			renderContext.TechniqueContext.Variables.LightType.Set(renderContext.LightContext.lightTypes);
		}
	}
}
