using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using global::SharpDX.DXGI;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using Device = global::SharpDX.Direct3D11.Device;
using System.Collections;

namespace HelixToolkit.Wpf.SharpDX
{
	public partial class DPFCanvas
	{
		//private static ILog log = LogManager.GetLogger(typeof(DPFCanvas));

		private void AttachScene()
		{
			if (this.sceneAttached)
				return;
			try
			{
				this.sceneAttached = true;
				this.ClearColor = this.renderRenderable.BackgroundColor;
				this.IsShadowMapEnabled = this.renderRenderable.IsShadowMappingEnabled;
				this.RenderTechnique = this.renderRenderable.RenderTechnique ?? Techniques.RenderBlinn;

				if (this.renderContext != null)
				{
					this.renderContext.Dispose();
				}
				this.renderContext = new RenderContext(SharpDevice, this,this, EffectsManager, EffectsManager.GetEffect(this.RenderTechnique));
				this.renderRenderable.Attach(this);
			}
			catch (System.Exception ex)
			{
				System.Windows.MessageBox.Show("DPFCanvas: Error attaching scene: " + string.Format(ex.Message), "Error");
				throw;
			}
		}

		private readonly Stopwatch renderTimer = new Stopwatch();
		private void StartRendering()
		{
			if (this.renderTimer.IsRunning)
				return;

			System.Windows.Media.CompositionTarget.Rendering += OnCompositionTargetRendering;
			this.renderTimer.Start();
		}

		private void StopRendering()
		{
			if (!this.renderTimer.IsRunning)
				return;

			this.renderTimer.Stop();
			System.Windows.Media.CompositionTarget.Rendering -= OnCompositionTargetRendering;
		}

		private void DoRenderingUpdate()
		{
			AttachScene();

			this.DoRenderingUpdate(this.renderContext);
		}

		private void DoRenderingUpdate(RenderContext renderContext)
		{
			this.Renderable.Update(renderContext, this.renderTimer.Elapsed);
		}

		private void Render(System.TimeSpan sceneTime, RenderContext _renderContext, ISharpRenderTarget buffer = null)
		{
						
			if (this.Renderable != null)
			{
				_renderContext.LightContext.ClearLights();

#if DEBUG
				DebugFlags.FlagsInt.RemoveAll(i => i.ToString().StartsWith("DoRender"));
#endif

				var bgColor = new Color4(0.2f, 0.2f, 0.2f, 1);
				if (buffer != null)
				{
					buffer.Apply();
					buffer.Clear(new Color4(0, 0, 0, 0));
				}
				else
				{
					SharpDevice.Apply();
					SharpDevice.Clear(bgColor);
				}

				_renderContext.SetCurrentTechnique(RenderTechnique);
				this.renderRenderable.Render(_renderContext);

				RenderData[] _detachedDatas;
				lock (detachedDatas)
				{
					_detachedDatas = detachedDatas.ToArray();
					detachedDatas.Clear();
				}

				foreach (var data in _detachedDatas)
					data.DoProcessAttachDetach(_renderContext);
			}

			//this.Device.ImmediateContext.Flush();
		}

		internal void OnCompositionTargetRendering(object sender, System.EventArgs e)
		{
			if (!this.renderTimer.IsRunning || Renderable == null)
				return;

			//if ((this.renderTimer.ElapsedMilliseconds - prevTick) < 16) // ~60 fps
			//	return;
			//prevTick = this.renderTimer.ElapsedMilliseconds;

#if USE_MT_RENDER
			var _renderTask = renderTask;
			if (_renderTask == null || _renderTask.Status > TaskStatus.Running)
			{
				if (SharpDevice == null)
					return;

				//log.DebugFormat("start rendering");
				DoRenderingUpdate();

				renderTask = Task.Run(() =>
				{
					try
					{
						RenderAsync(this.renderContext);
						SharpDevice.Present();
						//Dispatcher.BeginInvoke((Action)(() => InvalidateVisual()));
					}
					finally
					{
						renderTask = null;
					}
				});
			}
			else
			{
				//log.DebugFormat("waiting for render");
			}
#else
			RenderSync();
#endif
		}
		Task renderTask;

		private void WaitForEndLastRendering()
		{
			if (renderTask != null)
			{
				renderTask.Wait();
				renderTask = null;
			}
		}

		private void RenderAsync(RenderContext renderContext, ISharpRenderTarget buffer = null)
		{
			lock (forceRenderLock)
			{
				using (var queryForCompletion = new Query(renderContext.Device, new QueryDescription() { Type = QueryType.Event, Flags = QueryFlags.None }))
				{
					//device.ImmediateContext.Begin(queryForCompletion);

					this.Render(this.renderTimer.Elapsed, renderContext, buffer);

					renderContext.Device.ImmediateContext.End(queryForCompletion);

					int value;
					while (!renderContext.Device.ImmediateContext.GetData(queryForCompletion, out value) || (value == 0))
						Thread.Sleep(1);
				}
			}
		}

		readonly static object forceRenderLock = new object();

		public System.Drawing.Bitmap MakePhoto(MakePhotoParameters parameters)
		{
			lock (forceRenderLock)
			{
				if (parameters.PrepareAction != null)
					parameters.PrepareAction();

				using (var target = new SharpRenderTarget(parameters.Width, parameters.Height, false))
				{
					using (var rc = new RenderContext(target, this, this, target.EffectsManager, target.EffectsManager.GetEffect(this.RenderTechnique)))
					{
						rc.IsMakingPhoto = true;
						this.DoRenderingUpdate(rc);
						try
						{
							RenderAsync(rc, target);
							var photo = target.MakePhoto();
//#if DEBUG
//							photo.Save(string.Format("d:\\img_{0}.png", DateTime.Now.Ticks));
//#endif
							return photo;
						}
						finally
						{
							if (parameters.RestoreAction != null)
								parameters.RestoreAction();
							this.DoRenderingUpdate();
						}
					}
				}
			}
		}


	}

	
}
