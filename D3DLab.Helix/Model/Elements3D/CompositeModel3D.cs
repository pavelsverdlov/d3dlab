using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using global::SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	/// <summary>
	///     Represents a composite Model3D.
	/// </summary>
	[ContentProperty("Children")]
	public class CompositeModel3D : GeometryModel3D, IHitable
	{
		private readonly ObservableElement3DCollection children;

		/// <summary>
		///     Initializes a new instance of the <see cref="CompositeModel3D" /> class.
		/// </summary>
		public CompositeModel3D()
		{
			this.children = new ObservableElement3DCollection();
			this.children.CollectionChanged += this.ChildrenChanged;
		}

		/// <summary>
		///     Gets the children.
		/// </summary>
		/// <value>
		///     The children.
		/// </value>
		public ObservableElement3DCollection Children { get { return this.children; } }

		/// <summary>
		/// Attaches the specified host.
		/// </summary>
		/// <param name="host">
		/// The host.
		/// </param>
		public override void Attach(IRenderHost host)
		{
			base.Attach(host);
			foreach (var model in this.Children)
			{
				if (model.Parent == null)
				{
					this.AddLogicalChild(model);
				}

				model.Attach(host);
			}
		}

		/// <summary>
		///     Detaches this instance.
		/// </summary>
		public override void Detach()
		{
			foreach (var model in this.Children.ToList())
			{
				model.Detach();
				if (model.Parent == this)
				{
					this.RemoveLogicalChild(model);
				}
			}
			base.Detach();
		}

		/// <summary>
		/// Compute hit-testing for all children
		/// </summary>
		public virtual bool HitTest(Ray ray, ref List<HitTestResult> hits)
		{
			bool hit = false;

			foreach (var c in this.Children.ToList())
			{
				var hc = c as IHitable;
				if (hc != null)
				{
					var tc = c as ITransformable;
					if (tc != null)
					{
						tc.PushMatrix(this.modelMatrix);
						if (hc.HitTest(ray, ref hits))
						{
							hit = true;
						}
						tc.PopMatrix();
					}
					else
					{
						if (hc.HitTest(ray, ref hits))
						{
							hit = true;
						}
					}
				}
			}
			return hit;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			this.Detach();
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

			if (Children != null)
			{
				renderData.Items = this.Children.Cast<Element3D>().Select(i => i.RenderData).ToArray();
				foreach (var c in this.Children)
				{
					var model = c as ITransformable;
					if (model != null)
					{
						model.PushMatrix(this.modelMatrix);
						c.Update(renderContext, timeSpan);
						model.PopMatrix();
					}
					else
					{
						c.Update(renderContext, timeSpan);
					}
				}
			}
			else
				renderData.Items = null;
		}

		/// <summary>
		/// Handles changes in the Children collection.
		/// </summary>
		/// <param name="sender">
		/// The sender.
		/// </param>
		/// <param name="e">
		/// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
		/// </param>
		private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
					foreach (Model3D item in e.OldItems)
					{
						// todo: detach?
						// yes, always
						item.Detach();
						if (item.Parent == this)
						{
							this.RemoveLogicalChild(item);
						}
					}

					break;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
					foreach (Model3D item in e.NewItems)
					{
						if (this.IsAttached)
						{
							// todo: attach?
							// yes, always  
							// where to get a refrence to renderHost?
							// store it as private memeber of the class?
							if (item.Parent == null)
							{
								this.AddLogicalChild(item);
							}

							item.Attach(this.renderHost);
						}
					}

					break;
			}

			UpdateBounds();
		}

		/// <summary>
		/// a Model3D does not have bounds, 
		/// if you want to have a model with bounds, use GeometryModel3D instead:
		/// but this prevents the CompositeModel3D containg lights, etc. (Lights3D are Models3D, which do not have bounds)
		/// </summary>
		private void UpdateBounds()
		{
			BoundingBox? bb = null;
			foreach (var item in this.Children)
			{
				var model = item as IBoundable;
				if (model != null)
				{
					if (bb == null)
						bb = model.Bounds;
					else
						bb = BoundingBox.Merge(bb.Value, model.Bounds);
				}
			}

			this.Bounds = bb ?? default(BoundingBox);
		}
	}
}
