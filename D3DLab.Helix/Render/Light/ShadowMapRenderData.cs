using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	internal class ShadowMapRenderData : RenderData
	{
		private Texture2D depthBufferSM;
		private Texture2D colorBufferSM;
		private DepthStencilView depthViewSM;
		private RenderTargetView renderTargetSM;
		private ShaderResourceView texShadowMapView;
		private ShaderResourceView texColorMapView;
		private RenderContext shadowPassContext;

		public ShadowMapRenderData(IRenderHost host)
			: base(host, Techniques.RenderColors)
		{
		}

		public float Intensity { get; set; }

		public Vector2 Resolution { get; set; }

		public float FactorPCF { get; set; }

		public float Bias { get; set; }

		protected override void AttachCore(RenderContext renderContext)
		{
			if (!host.IsShadowMapEnabled)
				return;

			// gen shadow map
			this.depthBufferSM = new Texture2D(renderContext.Device, new Texture2DDescription()
			{
				Format = Format.R32_Typeless, //!!!! because of depth and shader resource
				//Format = global::SharpDX.DXGI.Format.B8G8R8A8_UNorm,
				ArraySize = 1,
				MipLevels = 1,
				Width = (int)Resolution.X,
				Height = (int)Resolution.Y,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource, //!!!!
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
			});

			this.colorBufferSM = new Texture2D(renderContext.Device, new Texture2DDescription
			{
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				Format = Format.B8G8R8A8_UNorm,
				Width = (int)Resolution.X,
				Height = (int)Resolution.Y,
				MipLevels = 1,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				OptionFlags = ResourceOptionFlags.None,
				CpuAccessFlags = CpuAccessFlags.None,
				ArraySize = 1
			});

			this.renderTargetSM = new RenderTargetView(renderContext.Device, colorBufferSM)
			{
			};

			this.depthViewSM = new DepthStencilView(renderContext.Device, depthBufferSM, new DepthStencilViewDescription()
			{
				Format = Format.D32_Float,
				Dimension = DepthStencilViewDimension.Texture2D,
				Texture2D = new DepthStencilViewDescription.Texture2DResource()
				{
					MipSlice = 0
				}
			});

			this.texShadowMapView = new ShaderResourceView(renderContext.Device, depthBufferSM, new ShaderResourceViewDescription()
			{
				Format = Format.R32_Float,
				Dimension = ShaderResourceViewDimension.Texture2D,
				Texture2D = new ShaderResourceViewDescription.Texture2DResource()
				{
					MipLevels = 1,
					MostDetailedMip = 0,
				}
			}); //!!!!

			this.texColorMapView = new ShaderResourceView(renderContext.Device, colorBufferSM, new ShaderResourceViewDescription()
			{
				Format = Format.B8G8R8A8_UNorm,
				Dimension = ShaderResourceViewDimension.Texture2D,
				Texture2D = new ShaderResourceViewDescription.Texture2DResource()
				{
					MipLevels = 1,
					MostDetailedMip = 0,
				}
			});

			this.shadowPassContext = new RenderContext(renderContext.RenderTarget, renderContext.Canvas,host, renderContext.EffectsManager, renderContext.TechniqueContext.Effect);
		}

		protected override void DetachCore()
		{
			Disposer.RemoveAndDispose(ref this.depthBufferSM);
			Disposer.RemoveAndDispose(ref this.depthViewSM);
			Disposer.RemoveAndDispose(ref this.colorBufferSM);

			Disposer.RemoveAndDispose(ref this.texColorMapView);
			Disposer.RemoveAndDispose(ref this.texShadowMapView);

			Disposer.RemoveAndDispose(ref this.shadowPassContext);
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			if (!this.host.IsShadowMapEnabled)
				return;

			/// --- set rasterizes state here with proper shadow-bias, as depth-bias and slope-bias in the rasterizer            
			renderContext.Device.ImmediateContext.Rasterizer.SetViewport(0, 0, Resolution.X, Resolution.Y, 0.0f, 1.0f);
			renderContext.Device.ImmediateContext.OutputMerger.SetTargets(depthViewSM);
			renderContext.Device.ImmediateContext.ClearDepthStencilView(depthViewSM, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

			var root = renderContext.RenderHost.Renderable.Items;
			foreach (var item in root)
			{
				var light = item as Light3D;
				if (light != null)
				{
					Camera lightCamera = null;
					//if (light is PointLightBase3D)
					//{
					//    var plight = (PointLightBase3D)light;
					//    lightCamera = new PerspectiveCamera()
					//    {
					//        Position = plight.Position,
					//        LookDirection = plight.Direction,
					//        UpDirection = Vector3.UnitY.ToVector3D(),
					//    };                        
					//}
					// else 
					if (light is DirectionalLight3D)
					{
						var dlight = (DirectionalLight3D)light;
						var dir = light.Direction.Normalized();
						var pos = -50 * light.Direction;

						//lightCamera = new PerspectiveCamera()
						//{
						//    LookDirection = dir.ToVector3D(),
						//    Position = (System.Windows.Media.Media3D.Point3D)(pos.ToVector3D()),
						//    UpDirection = Vector3.UnitZ.ToVector3D(),                            
						//    NearPlaneDistance = 1,
						//    FarPlaneDistance = 100,
						//    FieldOfView = 10,
						//};

						lightCamera = new OrthographicCamera()
						{
							LookDirection = dir.ToVector3D(),
							Position = (System.Windows.Media.Media3D.Point3D)(pos.ToVector3D()),
							UpDirection = Vector3.UnitZ.ToVector3D(),
							Width = 100,
							NearPlaneDistance = 1,
							FarPlaneDistance = 500,
						};
					}

					if (lightCamera != null)
					{
						var sceneCamera = renderContext.Camera;

						var lightRenderData = (LightRenderData)light.RenderData;
						lightRenderData.LightViewMatrix = CameraExtensions.GetViewMatrix(lightCamera);
						lightRenderData.LightProjectionMatrix = CameraExtensions.GetProjectionMatrix(lightCamera,
                            renderContext.Canvas.ActualWidth / renderContext.Canvas.ActualHeight);

						this.shadowPassContext.IsShadowPass = true;
						this.shadowPassContext.Camera = lightCamera;
						foreach (var e in root)
						{
							var smodel = e as IThrowingShadow;
							if (smodel != null)
							{
								if (smodel.IsThrowingShadow)
									((IRenderable)smodel).Render(this.shadowPassContext);
							}
						}
						renderContext.Camera = sceneCamera;
					}
				}
			}

			renderContext.TechniqueContext.Variables.TexShadowMapVariable.SetResource(this.texShadowMapView);
			renderContext.TechniqueContext.Variables.ShadowMapInfoVariable.Set(new Vector4(Intensity, FactorPCF, Bias, 0));
			renderContext.TechniqueContext.Variables.ShadowMapSizeVariable.Set(Resolution);

			renderContext.RenderHost.SetDefaultRenderTargets();
		}
	}
}
