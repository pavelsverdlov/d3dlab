using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public abstract class AnimationMathValueBase<T> : AnimationBase<T>
	{
		protected AnimationMathValueBase(T from, T to, Action<T> nextValueHandler, double duration, AnimateAttenuation type = AnimateAttenuation.Linear)
			: base(duration, nextValueHandler, type)
		{
			if (type == AnimateAttenuation.Linear)
			{
				this.step = Mult(Diff(to, from), 1 / duration);
			}
			else if (type == AnimateAttenuation.Exp)
			{
				this.step = Mult(Diff(to, from), 1 / duration);
			}
			else
				throw new NotImplementedException();
			this.current = from;
		}

		protected abstract T Sum(T v1, T v2);
		protected abstract T Diff(T v1, T v2);
		protected abstract T Mult(T v, double k);
		protected abstract bool CheckForStop(T step, double lastDistance);

		private readonly double lastDistance;
		private T step;
		private T current;
		private double k = double.NaN;

		protected override bool DoStep(double stepTimeMs)
		{
			current = Sum(current, Mult(step, stepTimeMs));

			if (k > 0)
				step = Mult(step, k);
			if (CheckForStop(step, lastDistance))
				return false;

			OnNextValue(current);
			return true;
		}
	}
}
