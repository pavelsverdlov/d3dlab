using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using global::SharpDX.DXGI;
using Device = global::SharpDX.Direct3D11.Device;
using HelixToolkit.Wpf.SharpDX.Render;
using System.Reflection;
using System.Windows.Threading;
using System.Windows.Interop;
using Image = System.Windows.Controls.Image;
using HelixToolkit.Wpf.SharpDX.WinForms;
using System.Runtime.InteropServices;
using WindowsFormsHost = System.Windows.Forms.Integration.WindowsFormsHost;
using System.Windows.Input;
using System.Text;
using System.Windows.Forms.Integration;
using HelixToolkit.Wpf.SharpDX.Controllers;

namespace HelixToolkit.Wpf.SharpDX
{
    /*
    public partial class DPFCanvas : WindowsFormsHost, IRenderHost, IFormsHost, IRenderHostInternal {
		private IRenderer renderRenderable;
		private bool sceneAttached;

        public new double ActualHeight { get { return ActualHeight; } set { Height = value; } }
        public new double ActualWidth { get { return ActualWidth; } set { Width = value; } }

        /// <summary>
        /// 
        /// </summary>
        public Color4 ClearColor { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public bool IsShadowMapEnabled { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public RenderTechnique RenderTechnique { get; private set; }

		private RenderContext renderContext;
		public RenderContext RenderContext
		{
			get { return renderContext; }
		}

		public SharpDevice SharpDevice
		{
			get {
			    return sharpDevice;

//                var ctrl = Child as WinFormsDirectXControl;
//				if (ctrl != null)
//					return ctrl.SharpDevice;
//				return null;
			}
		}

		public Device Device
		{
			get { return SharpDevice != null ? SharpDevice.Device : null; }
		}

		public EffectsManager EffectsManager
		{
			get {
			    return effectsManager;
//				var ctrl = Child as WinFormsDirectXControl;
//				if (ctrl != null)
//					return ctrl.EffectsManager;
//				return null;
			}
		}

		/// <summary>
		/// The instance of currently attached IRenderable - this is in general the Viewport3DX
		/// </summary>
		public IRenderer Renderable
		{
			get { return this.renderRenderable; }
			set
			{
				if (ReferenceEquals(this.renderRenderable, value))
				{
					return;
				}

				if (this.renderRenderable != null)
				{
					this.renderRenderable.Detach();
					this.renderRenderable = null;
				}

				this.sceneAttached = false;
				this.renderRenderable = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the control is in design mode
		/// (running in Blend or Visual Studio).
		/// </summary>
		public static bool IsInDesignMode
		{
			get
			{
				var prop = DesignerProperties.IsInDesignModeProperty;
				return (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
			}
		}

        private SharpDevice sharpDevice;
        private EffectsManager effectsManager;
        public DPFCanvas(System.Windows.Forms.Control control) :this(){
            sharpDevice = new SharpDevice(control);
            effectsManager = new EffectsManager(SharpDevice.Device); 
            
            //DPFCanvas_ChildChanged(this, null);
            WindowLoaded(this, null);
        }

	    /// <summary>
        /// 
        /// </summary>
        public DPFCanvas()
		{
			this.renderTimer = new Stopwatch();
			this.Loaded += this.WindowLoaded;
			this.Unloaded += this.WindowClosing;
			this.ClearColor = global::SharpDX.Color.Gray;
			this.IsShadowMapEnabled = false;
			RenderTechnique = Techniques.RenderBlinn;

			ChildChanged += DPFCanvas_ChildChanged;
		}

		private InputController inputController;
		public InputController InputController
		{
			get { return inputController; }
			set
			{
				if (inputController == value)
					return;

				if (inputController != null)
					inputController.Initialize(null);
				inputController = value;
				if (inputController != null)
					inputController.Initialize(Child);
			}
		}

		void DPFCanvas_ChildChanged(object sender, ChildChangedEventArgs e)
		{
			if (InputController != null)
				InputController.Initialize(Child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			if (IsInDesignMode)
			{
				return;
			}
			this.StartD3D();
			this.StartRendering();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowClosing(object sender, RoutedEventArgs e)
		{
			if (IsInDesignMode)
			{
				return;
			}

			this.StopRendering();
			this.EndD3D();
		}

		/// <summary>
		/// 
		/// </summary>
		private void StartD3D()
		{
			//Child = new WinFormsDirectXControl();
//			if (renderRenderable != null)
//				((Viewport3DX)renderRenderable).RenderControlCrteated();
		}

		private void EndD3D()
		{
			WaitForEndLastRendering();

			if (this.renderRenderable != null)
			{
				this.renderRenderable.Detach();
				this.sceneAttached = false;
			}

			if (Child != null)
				Child.Dispose();
			Child = null;
            Disposer.RemoveAndDispose(ref effectsManager);
            Disposer.RemoveAndDispose(ref sharpDevice);
        }

		public void SetDefaultRenderTargets()
		{
			SharpDevice.Apply();
		}

		#region Detached render datas

		private readonly HashSet<RenderData> detachedDatas = new HashSet<RenderData>();

		void IRenderHostInternal.AddDetachedRenderData(RenderData data)
		{
			lock (detachedDatas)
				detachedDatas.Add(data);
		}
	    void IRenderHostInternal.RemoveRenderData(RenderData data)
		{
			lock (detachedDatas)
				detachedDatas.Remove(data);
		}

		#endregion
	}
    */
	public interface IControlWndMessageRiser
	{
		void WndProc(ref System.Windows.Forms.Message m);
	}

	public sealed class MouseKeyboardMessageFilter : System.Windows.Forms.IMessageFilter, IDisposable
	{
		enum WndMessages
		{
			WM_KEYDOWN = 0x0100,
			WM_KEYUP = 0x0101,
			WM_MOUSEMOVE = 0x0200,
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205,
			WM_MBUTTONDOWN = 0x0207,
			WM_MBUTTONUP = 0x0208,
			WM_MOUSEWHEEL = 0x020A,
		}

		private readonly System.Windows.Forms.Control owner;
		public MouseKeyboardMessageFilter(System.Windows.Forms.Control owner)
		{
			this.owner = owner;
			System.Windows.Forms.Application.AddMessageFilter(this);
			owner.Disposed += owner_Disposed;
		}

		public void Dispose()
		{
			System.Windows.Forms.Application.RemoveMessageFilter(this);
		}

		void owner_Disposed(object sender, EventArgs e)
		{
			Dispose();
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		private bool IsChild(IntPtr hwnd1, IntPtr hwnd2)
		{
			IntPtr cur = hwnd1;
			while (cur != IntPtr.Zero)
			{
				if (cur == hwnd2)
					return true;
				cur = GetParent(cur);
			}
			return false;
		}

		public bool PreFilterMessage(ref System.Windows.Forms.Message m)
		{
			if (!owner.Visible || !owner.IsHandleCreated)
				return false;

			var msg = (WndMessages)m.Msg;
			if (msg != WndMessages.WM_MOUSEMOVE && msg != WndMessages.WM_MOUSEWHEEL && msg != WndMessages.WM_RBUTTONDOWN)
				return false;

			System.Drawing.Point p;
			var wnd = GetWindowUnderCursor(out p);

			bool focused = owner.Focused;
			if (msg == WndMessages.WM_MOUSEWHEEL || msg == WndMessages.WM_RBUTTONDOWN)
			{
				if (m.HWnd != wnd)
				{
					SendMessage(wnd, m.Msg, m.WParam, m.LParam);
					//SetFocus(wnd);
					return true;
				}
				return false;
			}

			if (msg == WndMessages.WM_MOUSEMOVE && !focused && owner.Handle == wnd)
			{
				((IControlWndMessageRiser)owner).WndProc(ref m);
				return true;
			}

			return false;
		}

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		internal static extern IntPtr SetFocus(IntPtr hwnd);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(System.Drawing.Point lpPoint);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

		public static IntPtr GetWindowUnderCursor(out System.Drawing.Point p)
		{
			if (!GetCursorPos(out p))
				return IntPtr.Zero;

			return WindowFromPoint(p);
		}
	}

	public class WinFormsDirectXControl1 : System.Windows.Forms.UserControl, IControlWndMessageRiser
	{
		public WinFormsDirectXControl1()
		{
			BackColor = System.Drawing.Color.DimGray;
			new MouseKeyboardMessageFilter(this);

			base.MouseMove += WinFormsDirectXControl_MouseMove;

		}

		bool isInSetOverrideCursor;
		System.Windows.Forms.Cursor savedCursor;
		void WinFormsDirectXControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			isInSetOverrideCursor = true;
			try
			{
				if (Mouse.OverrideCursor != null)
				{
					this.Cursor = Mouse.OverrideCursor.ToWFCursor();
				}
				else if (savedCursor != null)
				{
					this.Cursor = savedCursor;
					savedCursor = null;
				}
			}
			finally
			{
				isInSetOverrideCursor = false;
			}
		}

		public override System.Windows.Forms.Cursor Cursor
		{
			get { return base.Cursor; }
			set
			{
				if (base.Cursor == value)
					return;
				base.Cursor = value;

				if (!isInSetOverrideCursor)
					savedCursor = this.Cursor;

#if DEBUG
				//try
				//{
				//	var c = value.ToWPFCursor();
				//	Debug.WriteLine("=== set_Cursor: " + (c ?? Cursors.None).ToString());
				//}
				//catch (Exception ex)
				//{
				//}
#endif
			}
		}

		protected override System.Windows.Forms.CreateParams CreateParams
		{
			get
			{
				var prms = base.CreateParams;
				prms.Style |= 0x00010000; //WS_TABSTOP
				return prms;
			}
		}

		private EffectsManager effectsManager;
		public EffectsManager EffectsManager
		{
			get { return effectsManager; }
		}

		private SharpDevice sharpDevice;
		public SharpDevice SharpDevice
		{
			get { return sharpDevice; }
		}

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
		{
			if (base.DesignMode)
			{
				base.OnPaintBackground(e);
			}
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			//base.OnPaint(e);
			//var p1 = new System.Drawing.Point(0, 0);
			//var p2 = new System.Drawing.Point(1900, 1400);
			//var b = new System.Drawing.Drawing2D.LinearGradientBrush(p1, p2, System.Drawing.Color.Red, System.Drawing.Color.Blue);
			//e.Graphics.FillRectangle(b, p1.X, p1.Y, p2.X, p2.Y);
		}

		protected override void CreateHandle()
		{
			base.CreateHandle();
			DestroyDevice();

//#if DEBUG
//			bool isDebug = true;
//#else
//			bool isDebug = false;
//#endif
			sharpDevice = new SharpDevice(this/*, isDebug*/);
			effectsManager = new EffectsManager(SharpDevice.Device);
		}

		protected override void DestroyHandle()
		{
			DestroyDevice();
			base.DestroyHandle();
		}

		private void DestroyDevice()
		{
			Disposer.RemoveAndDispose(ref effectsManager);
			Disposer.RemoveAndDispose(ref sharpDevice);
		}

		void IControlWndMessageRiser.WndProc(ref System.Windows.Forms.Message m)
		{
			WndProc(ref m);
		}
	}
}
