using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public static class AnimationTimer
	{
		static AnimationTimer()
		{
			System.Windows.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
		}

		private readonly static Dictionary<object, AnimationBase> animations = new Dictionary<object, AnimationBase>();

		public static void Add(object id, AnimationBase animate)
		{
			AnimationBase prevAnimate;
			if (animations.TryGetValue(id, out prevAnimate))
			{
				animations.Remove(id);
			}

			animate.Id = id;
			animations.Add(id, animate);
		}

		private static void OnCompositionTargetRendering(object sender, EventArgs e)
		{
			foreach (var animation in animations.ToArray())
			{
				if (animation.Value.Duration <= 0 || !animation.Value.Step())
				{
					animations.Remove(animation.Key);
					animation.Value.Dispose();
				}
			}
		}
	}
}
