using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public class DirectionalLightRenderData : LightRenderData {

        public DirectionalLightRenderData(RenderTechnique renderTechnique)
			: base(LightType.Directional, renderTechnique) {

        }

		protected override void RenderCore(RenderContext renderContext)
		{
			/// --- set lighting parameters
			renderContext.LightContext.lightColors[lightIndex] = Color * (float)renderContext.IlluminationSettings.Light;
			renderContext.LightContext.lightTypes[lightIndex] = (int)Light3D.Type.Directional;

			//else
			//{
			//    // --- turn-off the light
			//    lightColors[lightIndex] = new global::SharpDX.Color4(0, 0, 0, 0);
			//}

			/// --- set lighting parameters
			renderContext.LightContext.lightDirections[lightIndex] = -Direction.ToVector4();

			/// --- update lighting variables               
			renderContext.TechniqueContext.Variables.LightDir.Set(renderContext.LightContext.lightDirections);
			renderContext.TechniqueContext.Variables.LightColor.Set(renderContext.LightContext.lightColors);
			renderContext.TechniqueContext.Variables.LightType.Set(renderContext.LightContext.lightTypes);


			/// --- if shadow-map enabled
			if (false)
			{
				/// update shader
				renderContext.TechniqueContext.Variables.LightView.SetMatrix(renderContext.LightContext.lightViewMatrices);
				renderContext.TechniqueContext.Variables.LightProj.SetMatrix(renderContext.LightContext.lightProjMatrices);
			}
		}
	}
}
