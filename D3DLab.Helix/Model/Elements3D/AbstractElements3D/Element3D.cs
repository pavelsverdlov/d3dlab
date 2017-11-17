using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HelixToolkit.Wpf.SharpDX.Render;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX
{
	/// <summary>
	/// Base class for renderable elements.
	/// </summary>    
	public abstract class Element3D : FrameworkElement, IDisposable, IRenderable, INotifyPropertyChanged
	{
		protected IRenderHost renderHost;

		protected RenderData renderData;
		public RenderData RenderData
		{
			get { return renderData; }
            set { renderData = value; }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="Element3D"/> class.
		/// </summary>
		public Element3D()
		{
		}

		public bool IsAttached
		{
			get { return renderHost != null; }
		}

		public void ReAttach()
		{
			if (renderHost == null)
				throw new InvalidOperationException("You can call ReAttach only if model is attached");
			if (renderData != null)
				renderData.ReAttach();
		}

		internal protected virtual Matrix CalcRenderMatrix(RenderContext renderContext, Matrix matrix)
		{
			return matrix;
		}

		string renderDataTag;
		public virtual string RenderDataTag
		{
			get { return renderDataTag ?? GetType().Name; }
			set { renderDataTag = value; }
		}

		/// <summary>
		/// Attaches the element to the specified host.
		/// </summary>
		/// <param name="host">The host.</param>
		public virtual void Attach(IRenderHost host)
		{
			if (renderData == null)
				renderData = CreateRenderData(host);

			if (renderData != null)
			{
				renderData.Name = RenderDataTag;
				renderData.Attach();
			}

			this.renderHost = host;
		}

		internal virtual RenderData CreateRenderData(IRenderHost host)
		{
			return null;
		}

		/// <summary>
		/// Detaches the element from the host.
		/// </summary>
		public virtual void Detach()
		{
			if (renderData != null)
				renderData.Detach();
			renderHost = null;
		}

		/// <summary>
		/// Updates the element by the specified time span.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="timeSpan">The time since last update.</param>
		public virtual void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			if (RenderData == null)
				return;

#if DEBUG
            renderData.Name = RenderDataTag;
#endif

			RenderData.Visible = Visible;
			RenderData.Transform = this.CalcRenderMatrix(renderContext, Matrix.Identity);
		}

		/// <summary>
		/// Renders the element in the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void Render(RenderContext context)
		{
			if (renderData != null)
			{
				renderData.Render(context);
			}
		}

		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Disposes the Element3D. Frees all DX resources.
		/// </summary>
		public virtual void Dispose()
		{
			this.Detach();
			renderData = null;
			var handler = Disposed;
			if (handler != null)
				handler(this, EventArgs.Empty);

			IsDisposed = true;
		}

		public event EventHandler Disposed;

		public static T FindVisualAncestor<T>(DependencyObject obj) where T : DependencyObject
		{
			var parent = System.Windows.Media.VisualTreeHelper.GetParent(obj);
			while (parent != null)
			{
				var typed = parent as T;
				if (typed != null)
				{
					return typed;
				}

				parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
			}

			return null;
		}

		public bool VisibleWithParent
		{
			get
			{
				if (!Visible)
					return false;

				var parent = Parent as Element3D;
				while (parent != null)
				{
					if (!parent.Visible)
						return false;
					parent = parent.Parent as Element3D;
				}
				return true;
			}
		}

		public virtual bool Visible
		{
			get { return Visibility == Visibility.Visible; }
			set
			{
				if (Dispatcher != System.Windows.Threading.Dispatcher.CurrentDispatcher)
					Dispatcher.Invoke(new Action<bool>(SetVisibleCore), value);
				else
					SetVisibleCore(value);
			}
		}

		private void SetVisibleCore(bool value)
		{
			if (Visible == value)
				return;
			Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			OnPropertyChanged("Visible");
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
