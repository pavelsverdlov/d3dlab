using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixToolkit.Wpf.SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public class ItemsRenderData : GeometryRenderData
	{
		public ItemsRenderData()
			: base(null)
		{
		}

		static readonly RenderData[] _empty = new RenderData[0];

		private RenderData[] items = _empty;
		public RenderData[] Items
		{
			get { return items; }
			set
			{
				var _value = value ?? _empty;
				if (items == _value)
					return;

				items = _value;
			}
		}

		protected override void AttachCore(RenderContext renderContext)
		{
		}

		protected override void DetachCore()
		{
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			for (int i = 0; i < items.Length; i++)
				items[i].Render(renderContext);
		}

		public override string ToString()
		{
			return ToStringShort() + string.Join(", ", Items.Select(i => i.ToStringShort()));
		}

		public override string ToStringShort()
		{
			return "Items[" + Items.Length + "]";
		}
	}
}
