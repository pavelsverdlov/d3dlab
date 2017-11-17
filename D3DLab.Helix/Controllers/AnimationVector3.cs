using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Vector3 = global::SharpDX.Vector3;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public class AnimationVector3 : AnimationMathValueBase<Vector3>
	{
		public AnimationVector3(Vector3 from, Vector3 to, Action<Vector3> nextValueHandler, double durationMs = 1000, AnimateAttenuation type = AnimateAttenuation.Linear)
			: base(from, to, nextValueHandler, durationMs, type)
		{
		}

		protected override Vector3 Sum(Vector3 v1, Vector3 v2)
		{
			return v1 + v2;
		}

		protected override Vector3 Diff(Vector3 v1, Vector3 v2)
		{
			return v1 - v2;
		}

		protected override Vector3 Mult(Vector3 v, double k)
		{
			return v * (float)k;
		}

		protected override bool CheckForStop(Vector3 step, double lastDistance)
		{
			return step.Length() * 1000 < lastDistance;
		}
	}
}
