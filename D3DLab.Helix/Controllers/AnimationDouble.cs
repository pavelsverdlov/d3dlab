using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class AnimationDouble : AnimationMathValueBase<double>
	{
		public AnimationDouble(double from, double to, Action<double> nextValueHandler, double durationMs = 1000, AnimateAttenuation type = AnimateAttenuation.Linear)
			: base(from, to, nextValueHandler, durationMs, type)
		{
		}

		protected override double Sum(double v1, double v2)
		{
			return v1 + v2;
		}

		protected override double Diff(double v1, double v2)
		{
			return v1 - v2;
		}

		protected override double Mult(double v, double k)
		{
			return v * k;
		}

		protected override bool CheckForStop(double step, double lastDistance)
		{
			return step * 1000 < lastDistance;
		}
	}
}
