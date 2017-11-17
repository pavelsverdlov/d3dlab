using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public class LightRenderContext
	{
		public const int maxLights = 16;
		public int lightCount = 0;
		public readonly Vector4[] lightDirections = new Vector4[maxLights];
		public readonly Vector4[] lightPositions = new Vector4[maxLights];
		public readonly Vector4[] lightAtt = new Vector4[maxLights];
		public readonly Vector4[] lightSpots = new Vector4[maxLights];
		public readonly Color4[] lightColors = new Color4[maxLights];
		public readonly int[] lightTypes = new int[maxLights];
		public readonly Matrix[] lightViewMatrices = new Matrix[maxLights];
		public readonly Matrix[] lightProjMatrices = new Matrix[maxLights];

		public void ClearLights()
		{
			Array.Clear(lightDirections, 0, lightCount);
			Array.Clear(lightPositions, 0, lightCount);
			Array.Clear(lightAtt, 0, lightCount);
			Array.Clear(lightSpots, 0, lightCount);
			Array.Clear(lightColors, 0, lightCount);
			Array.Clear(lightTypes, 0, lightCount);
			Array.Clear(lightViewMatrices, 0, lightCount);
			Array.Clear(lightProjMatrices, 0, lightCount);
			lightCount = 0;
		}
	}

	public abstract class LightRenderData : RenderData
	{
		protected int lightIndex = 0;

		protected LightRenderData(LightType lightType, RenderTechnique renderTechnique)
			: base()
		{
			LightType = lightType;
		}

		public LightType LightType { get; private set; }

		public Color4 Color { get; set; }

		public Vector3 Direction { get; set; }

		public Vector3 Position { get; set; }

		public Vector3 Attenuation { get; set; }

		public float Range { get; set; }

		public Matrix LightViewMatrix { get; set; }

		public Matrix LightProjectionMatrix { get; set; }

		protected override void AttachCore(RenderContext renderContext)
		{
		}

		protected override void DetachCore()
		{
		}

		protected override void PreRenderCore(RenderContext renderContext)
		{
			base.PreRenderCore(renderContext);

			if (LightType != LightType.Ambient)
			{
				if (renderContext.LightContext.lightCount == LightRenderContext.maxLights - 1)
					return;

				this.lightIndex = renderContext.LightContext.lightCount++;
			}
			renderContext.TechniqueContext.Variables.LightCount.Set(renderContext.LightContext.lightCount);
		}
	}
}
