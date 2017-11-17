using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public static class EffectExtensions
	{
		static readonly Dictionary<Effect, EffectVariables> effectVariables = new Dictionary<Effect, EffectVariables>();

		public static EffectVariables Variables(this Effect effect)
		{
			EffectVariables values;
			if (!effectVariables.TryGetValue(effect, out values))
				effectVariables[effect] = values = new EffectVariables(effect);
			return values;
		}
	}
}
