using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HelixToolkit.Wpf.SharpDX.Controllers
{
	public abstract class AnimationBase : IDisposable
	{
		protected AnimationBase(double duration, AnimateAttenuation animateAttenuation)
		{
			prevTime = DateTime.Now;
			Duration = duration;
		}

		public object Id { get; internal set; }
		public double Duration { get; private set; }

		public AnimateAttenuation Attenuation { get; private set; }

		DateTime prevTime;
		public bool Step()
		{
			var now = DateTime.Now;
			var dif = (now - prevTime).TotalMilliseconds;
			if (!DoStep(dif < Duration ? dif : Duration))
				return false;
			prevTime = now;
			Duration -= dif;
			return true;
		}

		protected abstract bool DoStep(double stepTimeMs);

		public virtual void Dispose() { Duration = 0; }
	}

	public abstract class AnimationBase<T> : AnimationBase
	{
		protected AnimationBase(double duration, Action<T> nextValueHandler, AnimateAttenuation animateAttenuation)
			: base(duration, animateAttenuation)
		{
			if (nextValueHandler != null)
				NextValue += nextValueHandler;
		}

		public override void Dispose()
		{
			foreach (Action<T> item in NextValue.GetInvocationList())
				NextValue -= item;

			base.Dispose();
		}

		protected virtual void OnNextValue(T nextValue)
		{
			var handler = NextValue;
			if (handler != null)
			{
				Debug.WriteLine(string.Format("- NextValue({0}) - {1}", Id, nextValue));
				handler(nextValue);
			}
		}
		public event Action<T> NextValue;
	}
}
