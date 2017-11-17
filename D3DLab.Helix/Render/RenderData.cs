using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public abstract class RenderData
	{
	//	public RenderTechnique originalRenderTechnique;
		

		protected RenderData()
		{
//			if (host == null)
//				throw new ArgumentNullException("host", "host is null.");
//			this.host = host;

			//originalRenderTechnique = _renderTechnique;
		}

		public string Name { get; set; }

		public bool Visible { get; set; }

		public Matrix Transform { get; set; }

		public RenderTechnique RenderTechniqueUserDefinded { get; set; }

		public bool IsAttached { get; set; }

		object stateLock = new object();

		protected bool doDetach;

		public void Attach()
		{
			lock (stateLock)
				IsAttached = true;
//			((IRenderHostInternal)host).RemoveRenderData(this);
		}

		public void Detach()
		{
			lock (stateLock)
			{
				IsAttached = false;
				doDetach = true;
			}
//			((IRenderHostInternal)host).AddDetachedRenderData(this);
		}

		public void ReAttach()
		{
			lock (stateLock)
			{
				IsAttached = true;
				doDetach = true;
			}
		}

		protected abstract void AttachCore(RenderContext renderContext);

		protected abstract void DetachCore();

		protected virtual bool CanRender(RenderContext renderContext)
		{
			return IsAttached && Visible;
		}

		global::SharpDX.Direct3D11.Device attachedToDevice;
		public void Render(RenderContext renderContext)
		{
//			if (!renderContext.TechniqueContext.InitEffect(renderContext.EffectsManager))
//				return;

//			if (attachedToDevice != null && attachedToDevice.NativePointer != renderContext.Device.NativePointer)
//				ReAttach();
//			attachedToDevice = renderContext.Device;

			DoProcessAttachDetach(renderContext);

			//((IRenderHostInternal)host).RemoveRenderData(this);

#if DEBUG
			DebugFlags.FlagsInt["DoRendering - " + Name] = DebugFlags.FlagsInt["DoRendering - " + Name] + 1;
#endif

			if (!CanRender(renderContext))
				return;

#if DEBUG
			DebugFlags.FlagsInt["DoRendered - " + Name] = DebugFlags.FlagsInt["DoRendered - " + Name] + 1;
#endif

			//renderContext.SetCurrentTechnique(RenderTechniqueUserDefinded ?? this.originalRenderTechnique);

			PreRenderCore(renderContext);

			RenderCore(renderContext);

			PostRenderCore(renderContext);
		}

		bool isAttachedInternal;
		public virtual void DoProcessAttachDetach(RenderContext renderContext)
		{
			bool needDetach;
			lock (stateLock)
			{
				needDetach = doDetach;
				doDetach = false;
			}

			if (needDetach)
			{
				isAttachedInternal = false;
				DetachCore();
			}

			if (!isAttachedInternal && IsAttached)
			{
				AttachCore(renderContext);
				isAttachedInternal = true;
			}
		}

		protected virtual void PreRenderCore(RenderContext renderContext)
		{
		}

		protected abstract void RenderCore(RenderContext renderContext);

		protected virtual void PostRenderCore(RenderContext renderContext)
		{
		}

		public virtual string ToStringShort()
		{
			return this.GetType().Name.Replace("GeometryRenderData", "").Replace("LightRenderData", "L");
		}
	}
}
