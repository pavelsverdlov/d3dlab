using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	[ContentProperty("Children")]
	public abstract class GroupElement3D : Element3D
	{
		public Element3DCollection Children
		{
			get { return (Element3DCollection)this.GetValue(ChildrenProperty); }
			set { this.SetValue(ChildrenProperty, value); }
		}

		public static readonly DependencyProperty ChildrenProperty =
			DependencyProperty.Register("Children", typeof(Element3DCollection), typeof(GroupElement3D), new UIPropertyMetadata(new Element3DCollection()));

		public GroupElement3D()
		{
			this.Children = new Element3DCollection();
		}

		public override void Attach(IRenderHost host)
		{
			base.Attach(host);
			foreach (var c in this.Children)
			{
				if (c.Parent == null)
					this.AddLogicalChild(c);

				c.Attach(host);
			}
		}

		public override void Detach()
		{
			base.Detach();
			foreach (var c in this.Children)
			{
				c.Detach();
				if (c.Parent == this)
					this.RemoveLogicalChild(c);
			}
		}

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new ItemsRenderData();
		}

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (ItemsRenderData)RenderData;

			if (Children == null)
			{
				renderData.Items = Children.Cast<Element3D>().Select(i => i.RenderData).ToArray();

				foreach (var c in this.Children)
					c.Update(renderContext, timeSpan);
			}
			else
				renderData.Items = null;
		}
	}
}
