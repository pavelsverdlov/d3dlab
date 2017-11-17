using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	internal class SpotLightRenderData : LightRenderData
	{
		public SpotLightRenderData(IRenderHost host)
			: base(LightType.Spot, Techniques.RenderPhong)
		{
		}

		public float OuterAngle { get; set; }

		public float InnerAngle { get; set; }

		public float Falloff { get; set; }


		protected override void RenderCore(RenderContext renderContext)
		{
#if DEFERRED  
	            if (host.RenderTechnique == Techniques.RenderDeferred || host.RenderTechnique == Techniques.RenderGBuffer)
	            {
	                return;
	            }
#endif

			/// --- Set light type
			renderContext.LightContext.lightTypes[lightIndex] = (int)Light3D.Type.Spot;
			/// --- turn-on the light            
			renderContext.LightContext.lightColors[this.lightIndex] = Color;

			/// --- Set lighting parameters
			renderContext.LightContext.lightPositions[this.lightIndex] = Position.ToVector4();
			renderContext.LightContext.lightDirections[this.lightIndex] = Direction.ToVector4();
			renderContext.LightContext.lightSpots[this.lightIndex] = new Vector4(
				(float)Math.Cos(OuterAngle / 360.0 * Math.PI),
				(float)Math.Cos(InnerAngle / 360.0 * Math.PI),
				Falloff,
				0);
			renderContext.LightContext.lightAtt[this.lightIndex] = new Vector4(Attenuation, Range);

			/// --- Update lighting variables    
			renderContext.TechniqueContext.Variables.LightPos.Set(renderContext.LightContext.lightPositions);
			renderContext.TechniqueContext.Variables.LightDir.Set(renderContext.LightContext.lightDirections);
			renderContext.TechniqueContext.Variables.LightSpot.Set(renderContext.LightContext.lightSpots);
			renderContext.TechniqueContext.Variables.LightColor.Set(renderContext.LightContext.lightColors);
			renderContext.TechniqueContext.Variables.LightAtt.Set(renderContext.LightContext.lightAtt);
			renderContext.TechniqueContext.Variables.LightType.Set(renderContext.LightContext.lightTypes);
		}
	}
}
