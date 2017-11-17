using System;
using System.Collections.Generic;
using System.Linq;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Shader;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;

namespace HelixToolkit.Wpf.SharpDX
{
 

  

   



    /// <summary>
    /// The render-context is currently generated per frame
    /// Optimizations might be possible
    /// </summary>
    public class RenderContext : IDisposable
	{
		public Matrix worldMatrix = Matrix.Identity;
		public Matrix viewMatrix;
		public Matrix projectionMatrix;

	    public Effect Effect { get; }

	    public Device Device
		{
			get { return Effect.Device; }
		}

		public EffectsManager EffectsManager { get; private set; }

		public Matrix ViewMatrix { get { return this.viewMatrix; } }

		public Matrix ProjectrionMatrix { get { return this.projectionMatrix; } }

		public Vector3 CameraPosition { get; private set; }

		public Vector3 CameraLookDirection { get; private set; }

		public Vector3 CameraUpDirection { get; private set; }

		public double ActualWidth { get; private set; }

		public double ActualHeight { get; private set; }

		public float PixelSize { get; private set; }

		private Camera camera;
		public Camera Camera
		{
			get { return this.camera; }
			set
			{
				this.camera = value;

				CameraPosition = camera.Position.ToVector3();
				CameraLookDirection = camera.LookDirection.ToVector3();
				CameraUpDirection = camera.UpDirection.ToVector3();

				//var ortho = camera as OrthographicCamera;
				this.viewMatrix = this.camera.CreateViewMatrix();
				ActualWidth = this.RenderTarget.Width;
				ActualHeight = this.RenderTarget.Height;

				var orphCamera = camera as OrthographicCamera;
				PixelSize = orphCamera != null ? (float)(orphCamera.Width / ActualWidth) : 1f;

				var aspectRatio = ActualWidth / this.RenderTarget.Height;
				this.projectionMatrix = this.camera.CreateProjectionMatrix(aspectRatio);

				var projCamera = this.camera as ProjectionCamera;
				isProjCamera = projCamera != null;
				if (projCamera != null)
				{
					// viewport: W,H,0,0
					this.viewport = new Vector4((float)this.RenderTarget.Width, (float)this.RenderTarget.Height, 0, 0);
					var ar = viewport.X / viewport.Y;

					var pc = projCamera as PerspectiveCamera;
					var fov = (pc != null) ? pc.FieldOfView : 90f;

					var zn = projCamera.NearPlaneDistance > 0 ? projCamera.NearPlaneDistance : 0.1;
					var zf = projCamera.FarPlaneDistance + 0.0;
					// frustum: FOV,AR,N,F
					this.frustum = new Vector4((float)fov, (float)ar, (float)zn, (float)zf);
				}
			}
		}
		private bool isProjCamera;
		private Vector4 viewport, frustum;

		public bool IsMakingPhoto { get; set; }

		private IlluminationSettings illuminationSettings;
		public IlluminationSettings IlluminationSettings
		{
			get { return illuminationSettings; }
			set
			{
				illuminationSettings = value;

				this.Effect.Variables().LightAmbient.Set(new Color4((float)value.Ambient).ChangeAlpha(1f));
				this.Effect.Variables().IllumDiffuse.Set(new Color4((float)value.Diffuse).ChangeAlpha(1f));
				this.Effect.Variables().IllumShine.Set((float)value.Shine);
				this.Effect.Variables().IllumSpecular.Set(new Color4((float)value.Specular).ChangeAlpha(1f));
			}
		}

		//public IFormsHost Canvas { get; private set; }
      //  public IRenderHost RenderHost { get; private set; }

        public ISharpRenderTarget RenderTarget { get; private set; }

		public bool IsShadowPass { get; set; }

		public bool IsDeferredPass { get; set; }

		public RenderContext(ISharpRenderTarget renderTarget,EffectsManager effectsManager, Effect effect, LightRenderContext lightRenderContext)
		{
			if (renderTarget == null)
				throw new ArgumentNullException("renderTarget", "renderTarget is null.");
		
			if (effectsManager == null)
				throw new ArgumentNullException("effectsManager", "effectsManager is null.");
			if (effect == null)
				throw new ArgumentNullException("effect", "effect is null.");
			this.RenderTarget = renderTarget;
			//this.Canvas = canvas;
		  //  RenderHost = rhost;
			this.EffectsManager = effectsManager;
			this.IsShadowPass = false;
			this.IsDeferredPass = false;
			this.Effect = effect;
			this.LightContext = lightRenderContext;
		}

		~RenderContext()
		{
			this.Dispose();
		}

		const int INT_PlaneCountMax = 8;
		public void SetPlanes(CuttingPlane[] planes)
		{
			planes = planes.Where(i => i.Activated).ToArray();
			if (planes.Length > INT_PlaneCountMax)
				Array.Resize(ref planes, INT_PlaneCountMax);

			// set plane count
			Effect.Variables().PlaneCount.Set(planes.Length);

			// set plane line width
			var orthoCam = Camera as OrthographicCamera;
			var lineWidth = orthoCam.Width * 2 / ActualWidth;
			if (lineWidth > 1)
			{
				lineWidth = 1;
			}
			Effect.Variables().PlaneLineWidth.Set((float)(lineWidth));

			// set planes distances to 0,0,0
			var pDistances = planes.Select(x => x.Plane.D).ToArray();
			Effect.Variables().PlaneDs.Set(AlignArray(pDistances, INT_PlaneCountMax));

			// set planes normales
			var pNormals = planes.Select(x => x.Plane.Normal.ToVector4()).ToArray();
			Effect.Variables().PlaneNormals.Set(AlignArray(pNormals, INT_PlaneCountMax));

			// set planes colors
			var pColors = planes.Select(x => x.Color).ToArray();
			Effect.Variables().PlaneColors.Set(AlignArray(pColors, INT_PlaneCountMax));
		}

		private static T[] AlignArray<T>(T[] array, int size)
		{
			if (array.Length != INT_PlaneCountMax)
				Array.Resize(ref array, size);
			return array;
		}

		public LightRenderContext LightContext { get; set; }

		public void Dispose()
		{
			foreach (var context in contexts)
				context.Value.Dispose();
			contexts.Clear();
		}

		public void RenderCamera()
		{
			this.Effect.Variables().EyePos.Set(CameraPosition);
			this.Effect.Variables().Projection.SetMatrix(ref projectionMatrix);
			this.Effect.Variables().View.SetMatrix(ref viewMatrix);
			if (isProjCamera)
			{
				this.Effect.Variables().Viewport.Set(ref viewport);
				this.Effect.Variables().Frustum.Set(ref frustum);
				this.Effect.Variables().EyeLook.Set(this.CameraLookDirection);
			}
		}

		private const int INT_BOXES_COUNT_MAX = 8;
		public void SetBoxes(BoundingBoxColoring[] boxes)
		{

			// set boxes count
			Effect.Variables().BoxCount.Set(boxes.Length);

			// set boxes maximums
			var maximums = boxes.Select(x => x.Box.Maximum.ToVector4()).ToArray();
			Effect.Variables().BoxesMaximums.Set(AlignArray(maximums, INT_BOXES_COUNT_MAX));

			// set boxes minimums
			var minimums = boxes.Select(x => x.Box.Minimum.ToVector4()).ToArray();
			Effect.Variables().BoxesMinimums.Set(AlignArray(minimums, INT_BOXES_COUNT_MAX));

			// set boxes colors
			var colors = boxes.Select(x => x.InvalidPartsColor.ToColor4()).ToArray();
			Effect.Variables().BoxesColors.Set(AlignArray(colors, INT_BOXES_COUNT_MAX));

			// set block radius
			var floatRadius = boxes.Select(x => x.BlockRadius).ToArray();
			Effect.Variables().BoxBlockRadius.Set(AlignArray(floatRadius, INT_BOXES_COUNT_MAX));

			if(boxes.Any()){
				var boxAxis = boxes.Select(x=>(int)x.CilidricalColoringAxis).ToArray();
				Effect.Variables().CilidricalColoringAxis.Set(AlignArray(boxAxis,INT_BOXES_COUNT_MAX));
			}

		}

		private readonly Dictionary<string, TechniqueContext> contexts = new Dictionary<string, TechniqueContext>();
		public void SetCurrentTechnique(RenderTechnique renderTechnique)
		{
			if (renderTechnique == null)
		        throw new ArgumentNullException("renderTechnique", "renderTechnique is null.");
            
            renderTechnique.UpdateVariables(Effect);

		    TechniqueContext ctx;
			if (!contexts.TryGetValue(renderTechnique.Name, out ctx))
		        ctx = new TechniqueContext(renderTechnique, EffectsManager);
		    TechniqueContext = ctx;
		}

		public TechniqueContext TechniqueContext { get; set; }

		public IList<Tuple<RenderTechnique, Func<MeshGeometryModel3D, bool>>> GlobalRenderTechniques { get; set; }
		public RenderTechnique GlobalRenderTechnique(MeshGeometryModel3D model)
		{
			if (GlobalRenderTechniques == null || GlobalRenderTechniques.Count == 0)
				return null;

			var item = GlobalRenderTechniques.FirstOrDefault(i => i.Item2 == null || i.Item2(model));
			if (item == null)
				return null;

			return item.Item1;
		}

//		public void SetSphere(SphereColoring sphereColoring)
//		{
//			if(sphereColoring != null)
//			{
//				Effect.Variables().SphereColoringColor.Set(sphereColoring.Color.ToColor4());
//				Effect.Variables().SphereColoringPosition.Set(sphereColoring.Position.ToVector4());
//				Effect.Variables().SphereColoringRadius.Set(sphereColoring.SphereRadius);
//				Effect.Variables().SphereColoringOffset.Set(sphereColoring.SphereOffset);
//				Effect.Variables().SphereEnabled.Set(true);
//			}
//		}
	}
}
