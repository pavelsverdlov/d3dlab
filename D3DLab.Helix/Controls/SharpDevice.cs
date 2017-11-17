using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Device11 = SharpDX.Direct3D11.Device;

namespace HelixToolkit.Wpf.SharpDX.WinForms
{
	/// <summary>
	/// Encapsulate All DirectX Elements
	/// </summary>
	public class SharpDevice : ISharpRenderTarget, IDisposable
	{
		//private 
		private Device11 device;
		private DeviceContext deviceContext;
		private SwapChain swapChain;

		private Texture2D backBufferTexture;
		private RenderTargetView backBufferView;
		private Texture2D zbufferTexture;
		private DepthStencilView zbufferView;

		/// <summary>
		/// Device
		/// </summary>
		public Device11 Device { get { return device; } }

		/// <summary>
		/// Device Context
		/// </summary>
		public DeviceContext DeviceContext { get { return deviceContext; } }

		/// <summary>
		/// Swapchain
		/// </summary>
		public SwapChain SwapChain { get { return swapChain; } }

		/// <summary>
		/// Rendering Form
		/// </summary>
		public System.Windows.Forms.Control Control { get; private set; }

		/// <summary>
		/// View to BackBuffer
		/// </summary>
		public RenderTargetView BackBufferView { get { return backBufferView; } }

		/// <summary>
		/// View to Depth Buffer
		/// </summary>
		public DepthStencilView ZBufferView { get { return zbufferView; } }

		public int BufferCount { get; private set; }

		public int Width
		{
			get { return Control.Width; }
		}

		public int Height
		{
			get { return Control.Height; }
		}

		/// <summary>
		/// Init all object to start rendering
		/// </summary>
		/// <param name="control">Rendering form</param>
		/// <param name="debug">Active the debug mode</param>
		public SharpDevice(System.Windows.Forms.Control control, bool debug = false, int bufferCount = 1)
		{
			BufferCount = bufferCount;

			Control = control;
			var desc = new SwapChainDescription()
			{
				BufferCount = BufferCount,//buffer count
				ModeDescription = new ModeDescription(Control.ClientSize.Width, Control.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = true,
				OutputHandle = Control.Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };

			//create device and swapchain
			DeviceCreationFlags flag = DeviceCreationFlags.None;
			if (debug)
				flag = DeviceCreationFlags.Debug;

			var adapter = AdapterFactory.GetBestAdapter();
			if (adapter == null)
				Device11.CreateWithSwapChain(global::SharpDX.Direct3D.DriverType.Hardware, flag, levels, desc, out device, out swapChain);
			else
				Device11.CreateWithSwapChain(adapter, flag, levels, desc, out device, out swapChain);

			//get context to device
			deviceContext = Device.ImmediateContext;

			//Ignore all windows events
			var factory = SwapChain.GetParent<Factory>();
			factory.MakeWindowAssociation(Control.Handle, WindowAssociationFlags.IgnoreAll);

			//Setup handler on resize form
			Control.Resize += (sender, args) => mustResize = true;

			//Resize all items
			Resize();
		}

		bool mustResize = true;
		public void ResizeIfNeed()
		{
			if (!mustResize)
				return;
			Resize();
		}

		/// <summary>
		/// Create and Resize all items
		/// </summary>
		private void Resize()
		{
			// Dispose all previous allocated resources
			DisposeBuffers();

			Debug.WriteLine(string.Format("Resize :: ({0}, {1})", Control.ClientSize.Width, Control.ClientSize.Height));

			// Resize the backbuffer
			SwapChain.ResizeBuffers(BufferCount, Control.ClientSize.Width, Control.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

			// Get the backbuffer from the swapchain
			backBufferTexture = SwapChain.GetBackBuffer<Texture2D>(0);

			// Backbuffer
			backBufferView = new RenderTargetView(Device, backBufferTexture);

			// Depth buffer

			zbufferTexture = new Texture2D(Device, new Texture2DDescription()
			   {
				   Format = Format.D32_Float_S8X24_UInt,
				   ArraySize = 1,
				   MipLevels = 1,
				   Width = Control.ClientSize.Width,
				   Height = Control.ClientSize.Height,
				   SampleDescription = new SampleDescription(1, 0),
				   Usage = ResourceUsage.Default,
				   BindFlags = BindFlags.DepthStencil,
				   CpuAccessFlags = CpuAccessFlags.None,
				   OptionFlags = ResourceOptionFlags.None
			   });

			// Create the depth buffer view
			zbufferView = new DepthStencilView(Device, zbufferTexture);

			// End resize
			mustResize = false;

			Apply();
		}

		/// <summary>
		/// Set default render and depth buffer inside device context
		/// </summary>
		public void Apply()
		{
			ResizeIfNeed();
			// Setup targets and viewport for rendering
			DeviceContext.Rasterizer.SetViewport(0, 0, Control.ClientSize.Width, Control.ClientSize.Height);
			DeviceContext.OutputMerger.SetTargets(this.zbufferView, this.backBufferView);
		}

		private void DisposeBuffers()
		{
			Disposer.RemoveAndDispose(ref backBufferTexture);
			Disposer.RemoveAndDispose(ref zbufferTexture);
			Disposer.RemoveAndDispose(ref backBufferView);
			Disposer.RemoveAndDispose(ref zbufferView);
		}

		/// <summary>
		/// Dispose element
		/// </summary>
		public void Dispose()
		{
			DisposeBuffers();
			swapChain.Dispose();
			deviceContext.Dispose();
			device.Dispose();
		}

		/// <summary>
		/// Clear backbuffer and zbuffer
		/// </summary>
		/// <param name="color">background color</param>
		public void Clear(Color4 color)
		{
			DeviceContext.ClearRenderTargetView(backBufferView, color);
			DeviceContext.ClearDepthStencilView(zbufferView, DepthStencilClearFlags.Depth, 1.0F, 0);
		}

		/// <summary>
		/// Present scene to screen
		/// </summary>
		public void Present()
		{
			SwapChain.Present(0, PresentFlags.None);
		}

		/// <summary>
		/// Update constant buffer
		/// </summary>
		/// <typeparam name="T">Data Type</typeparam>
		/// <param name="buffer">Buffer to update</param>
		/// <param name="data">Data to write inside buffer</param>
		public void UpdateData<T>(Buffer11 buffer, T data) where T : struct
		{
			DeviceContext.UpdateSubresource(ref data, buffer);
		}

		/// <summary>
		/// Apply multiple targets to device
		/// </summary>
		/// <param name="targets">List of targets. Depth Buffer is taken from first one</param>
		public void ApplyMultipleRenderTarget(params SharpRenderTarget[] targets)
		{
			var targetsView = targets.Select(t => t.Target).ToArray();
			DeviceContext.OutputMerger.SetTargets(targets[0].Zbuffer, targetsView);
			DeviceContext.Rasterizer.SetViewport(0, 0, targets[0].Width, targets[0].Height);
		}


		/// <summary>
		/// DirectX11 Support Avaiable
		/// </summary>
		/// <returns>Supported</returns>
		public static bool IsDirectX11Supported()
		{
			return global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
		}

		public System.Drawing.Bitmap MakePhoto()
		{
			using (var stream = new MemoryStream())
			{
				this.backBufferTexture.Save(stream, this.Device);
				stream.Position = 0;
				var bmp = new System.Drawing.Bitmap(stream);
				return bmp;
			}
		}
	}
}
