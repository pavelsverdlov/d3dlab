using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Resource11 = SharpDX.Direct3D11.Resource;
using WIC = global::SharpDX.WIC;

namespace HelixToolkit.Wpf.SharpDX.WinForms
{
	public interface ISharpRenderTarget
	{
		void Apply();
		void Clear(Color4 color);

		int Width { get; }
		int Height { get; }
	}

	/// <summary>
	/// Contain Render Targets
	/// </summary>
	public class SharpRenderTarget : ISharpRenderTarget, IDisposable
	{
		public Device11 Device { get; private set; }

		public EffectsManager EffectsManager { get; private set; }

		private Texture2D targetTexture;
		public RenderTargetView Target { get; private set; }

		public DepthStencilView Zbuffer { get; private set; }

		public ShaderResourceView Resource { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public SharpRenderTarget(int width, int height, bool debug)
		{
			FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };

			//create device and swapchain
			DeviceCreationFlags flag = DeviceCreationFlags.None;
			if (debug)
				flag = DeviceCreationFlags.Debug;

			var adapter = AdapterFactory.GetBestAdapter();
			if (adapter == null)
				Device = new Device11(global::SharpDX.Direct3D.DriverType.Hardware, flag, levels);
			else
				Device = new Device11(adapter, flag, levels);

			EffectsManager = new EffectsManager(Device);

			Height = height;
			Width = width;

			targetTexture = new Texture2D(Device, new Texture2DDescription()
			{
				Format = Format.R8G8B8A8_UNorm,
				Width = width,
				Height = height,
				ArraySize = 1,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
			});

			Target = new RenderTargetView(Device, targetTexture);
			Resource = new ShaderResourceView(Device, targetTexture);

			var _zbufferTexture = new Texture2D(Device, new Texture2DDescription()
			{
				Format = Format.D32_Float_S8X24_UInt,
				ArraySize = 1,
				MipLevels = 1,
				Width = width,
				Height = height,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			});

			// Create the depth buffer view
			Zbuffer = new DepthStencilView(Device, _zbufferTexture);
			_zbufferTexture.Dispose();
		}

		/// <summary>
		/// Apply Render Target To Device Context
		/// </summary>
		public void Apply()
		{
			Device.ImmediateContext.Rasterizer.SetViewport(0, 0, Width, Height);
			Device.ImmediateContext.OutputMerger.SetTargets(Zbuffer, Target);
		}

		/// <summary>
		/// Dispose resource
		/// </summary>
		public void Dispose()
		{
			targetTexture.Dispose();
			Resource.Dispose();
			Target.Dispose();
			Zbuffer.Dispose();
			EffectsManager.Dispose();
			Device.Dispose();
		}

		/// <summary>
		/// Clear backbuffer and zbuffer
		/// </summary>
		/// <param name="color">background color</param>
		public void Clear(Color4 color)
		{
			Device.ImmediateContext.ClearRenderTargetView(Target, color);
			Device.ImmediateContext.ClearDepthStencilView(Zbuffer, DepthStencilClearFlags.Depth, 1.0F, 0);
		}

		public System.Drawing.Bitmap MakePhoto()
		{
			using (var stream = new MemoryStream())
			{
				this.targetTexture.Save(stream, Device);
				//Resource11.ToStream(DeviceContext, this.Target.Resource, ImageFileFormat.Png, stream);
				stream.Position = 0;
				var bmp = new System.Drawing.Bitmap(stream);
				return bmp;
			}
		}
	}

	public static class Texture2DExtensions
	{
		public static void Save(this Texture2D texture, Stream stream, Device11 device)
		{
			var desc = new Texture2DDescription
			{
				Width = (int)texture.Description.Width,
				Height = (int)texture.Description.Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = texture.Description.Format,
				Usage = ResourceUsage.Staging,
				SampleDescription = new SampleDescription(1, 0),
				BindFlags = BindFlags.None,
				CpuAccessFlags = CpuAccessFlags.Read,
				OptionFlags = ResourceOptionFlags.None
			};

			using (var factory = new WIC.ImagingFactory())
			{
				using (var textureCopy = new Texture2D(device, desc))
				{
					device.ImmediateContext.CopyResource(texture, textureCopy);

					DataStream dataStream;
					var dataBox = device.ImmediateContext.MapSubresource(
						textureCopy,
						0,
						0,
						MapMode.Read,
						global::SharpDX.Direct3D11.MapFlags.None,
						out dataStream);

					var dataRectangle = new DataRectangle
					{
						DataPointer = dataStream.DataPointer,
						Pitch = dataBox.RowPitch
					};

					using (var bitmap = new WIC.Bitmap(factory, textureCopy.Description.Width, textureCopy.Description.Height,
						WIC.PixelFormat.Format32bppRGBA, dataRectangle))
					{

						stream.Position = 0;
						using (var bitmapEncoder = new WIC.PngBitmapEncoder(factory, stream))
						{
							using (var bitmapFrameEncode = new WIC.BitmapFrameEncode(bitmapEncoder))
							{
								bitmapFrameEncode.Initialize();
								bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
								var pixelFormat = WIC.PixelFormat.FormatDontCare;
								bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
								bitmapFrameEncode.WriteSource(bitmap);
								bitmapFrameEncode.Commit();
								bitmapEncoder.Commit();
							}
						}

					}
					device.ImmediateContext.UnmapSubresource(textureCopy, 0);
				}
			}
		}
	}
}
