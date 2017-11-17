using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public class TechniqueContext : IDisposable
	{
		public TechniqueContext(RenderTechnique renderTechnique, EffectsManager effectsManager)
		{
			if (renderTechnique == null)
				throw new ArgumentNullException("renderTechnique", "renderTechnique is null.");

			this.RenderTechnique = renderTechnique;
			InitEffect(effectsManager);
		}

	    public RenderTechnique RenderTechnique { get; private set; }

	    public Effect Effect { get; private set; }
		public EffectTechnique EffectTechnique { get; private set; }
		public InputLayout VertexLayout { get; private set; }

		public void Dispose()
		{
		}

		public bool InitEffect(EffectsManager effectsManager)
		{
			if (this.Effect != null)
				return true;

			this.Effect = effectsManager.GetEffect(this.RenderTechnique);
			this.VertexLayout = effectsManager.GetLayout(this.RenderTechnique);
			this.EffectTechnique = Effect.GetTechniqueByName(this.RenderTechnique.Name);
			return true;
		}

		public void ApplyPass(global::SharpDX.Direct3D11.Device device, int i)
		{
			EffectTechnique.GetPassByIndex(i).Apply(device.ImmediateContext);
		}

		public EffectVariables Variables
		{
			get { return this.Effect.Variables(); }
		}
	}
}
