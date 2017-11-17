// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Viewport3DX.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// <summary>
//   Provides a Viewport control.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Shader;
using HelixToolkit.Wpf.SharpDX.Render;
using SharpDX;
using Bitmap = System.Drawing.Bitmap;

namespace HelixToolkit.Wpf.SharpDX
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

    public partial class Viewport3DX {//ItemsControl
        public List<object> Items { get; private set; }
        public IFormsHost Host { get; set; }

        public double ActualHeight { get { return Host.ActualHeight; } }
        public double ActualWidth { get { return Host.ActualWidth; } }

        public double Height { get { return Host.ActualHeight; } set { Host.ActualHeight = value; } }
        public double Width { get { return Host.ActualWidth; } set { Host.ActualWidth = value; } }

        public string FieldOfViewText { get; set; }

        public string CameraInfo { get; set; }
        public bool ShowFieldOfView { get; set; }
        public bool ShowCameraInfo { get; set; }
        public IList<Tuple<RenderTechnique, Func<MeshGeometryModel3D, bool>>> GlobalRenderTechniques { get; set; }
        public System.Windows.Media.Brush Background { get; set; }
        public bool IsShadowMappingEnabled { get; set; }
        public Color4 BackgroundColor { get; set; }
        public RenderTechnique RenderTechnique { get; set; }
        public Camera Camera { get; set; }
   

        /// <summary>
        /// Initializes static members of the <see cref="Viewport3DX" /> class.
        /// </summary>
        /// 
        private MeshGeometryRenderData backGroundRenderData;

		/// <summary>
		/// Turn on/off viewport gradient background
		/// true - by default
		/// </summary>
		public bool IsBackgroundEnabled { get; set; }

       // public new List<object> Items { get; private set; }

        public Viewport3DX(IFormsHost canvas, Camera camera) :this() {//
            //Items = new List<object>(); 
            Host = canvas;
            //  this.RenderHost = canvas;
            Camera = camera;
            this.OnCameraChanged();
        }

	    /// <summary>
        /// Initializes a new instance of the <see cref="Viewport3DX" /> class.
        /// </summary>
        public Viewport3DX()
		{
			IsBackgroundEnabled = true;
            Items = new List<object>();
                //FpsCounter = new FpsCounter();
                Planes = new List<CuttingPlane>();
			BlockBoundingBoxes = new List<BoundingBoxColoring>();
			IsToothValidationRenderingEnabled = true;
			Background = new LinearGradientBrush(Colors.Transparent, Colors.Transparent,0);
		}

		public IRenderHost RenderHost { get; set; }
        
		CuttingPlane[] planes;
		RenderData[] renderDatas;
	
	

		public static BitmapSource GenerateBackGroundBitmapSource(Brush brush, int height, int width)
		{

			var bmp = new RenderTargetBitmap(width, (int)(height), 96, 96, PixelFormats.Pbgra32);
			var drawingVisual = new DrawingVisual();
			using (var ctx = drawingVisual.RenderOpen())
			{
				ctx.DrawRectangle(brush, null, new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
			}
			bmp.Render(drawingVisual);

			return bmp;
		}
		public void UpdateBackgroundColor()
		{
			if (backGroundRenderData == null)
			{
				backGroundRenderData = new MeshGeometryRenderData(Techniques.RenderBackground);
				var mesh = new MeshGeometry3D();


				var bitmap = GenerateBackGroundBitmapSource(Background, 100, 100);
				mesh.Positions = new Vector3Collection(){
					new Vector3(-1,-1,1),
					new Vector3(1,-1,1),
				    new Vector3(-1, 1, 1),
				    new Vector3(1, 1, 1)
				};

				mesh.Indices = new IntCollection() { 0, 1, 2, 1, 3, 2 };
                mesh.TextureCoordinates = new Vector2Collection() { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
				backGroundRenderData.RenderSources.Positions.Update(mesh);
				backGroundRenderData.RenderSources.Indices.Update(mesh);
				//backGroundRenderData.RenderSources.Colors.Update(mesh);
				backGroundRenderData.RenderSources.TextureCoordinates.Update(mesh);
				backGroundRenderData.RenderTechniqueUserDefinded = Techniques.RenderBackground;
				backGroundRenderData.Material = new PhongMaterial() { DiffuseMap = bitmap };
				backGroundRenderData.Visible = true;
				backGroundRenderData.TextureCoordScale = new Vector2(1);

				backGroundRenderData.Transform = global::SharpDX.Matrix.Identity;
				backGroundRenderData.Attach();
			}
			else
			{
				var bitmap = GenerateBackGroundBitmapSource(Background, 100, 100);
				backGroundRenderData.Material = new PhongMaterial() { DiffuseMap = bitmap };
			}
		}

		/// <summary>
		/// Called when the camera is changed.
		/// </summary>
		protected virtual void OnCameraChanged()
		{
			var projectionCamera = this.Camera as ProjectionCamera;
			if (projectionCamera == null)
			{
				return;
			}

			if (this.ShowFieldOfView)
			{
				this.UpdateFieldOfViewInfo();
			}

			if (this.ShowCameraInfo)
			{
				this.UpdateCameraInfo();
			}
		}

		
		public void ClearBoxes()
		{
			BlockBoundingBoxes.Clear();
		}

		public void AddBlockBox(BoundingBoxColoring box)
		{
			BlockBoundingBoxes.Add(box);
		}

		public bool IsToothValidationRenderingEnabled { get; set; }

		/// <summary>
		/// Handles the change of the render technique        
		/// </summary>
		private void RenderTechniquePropertyChanged()
		{
			if (this.RenderHost != null)
			{
				// remove the scene
//				this.RenderHost.Renderable = null;

				// if new rendertechnique set, attach the scene
				if (this.RenderTechnique != null)
				{
//					this.RenderHost.Renderable = this;
				}
			}
		}
       
		
        

		/// <summary>
		/// Updates the camera info.
		/// </summary>
		private void UpdateCameraInfo()
		{
			this.CameraInfo = this.Camera.GetInfo();
		}

		/// <summary>
		/// The update field of view info.
		/// </summary>
		private void UpdateFieldOfViewInfo()
		{
			var pc = this.Camera as PerspectiveCamera;
			this.FieldOfViewText = pc != null ? string.Format("FoV ∠ {0:0}°", pc.FieldOfView) : null;
		}
        
		private BoundingBoxColoring[] boxesColorings;
		private SphereColoring sphereColoring;
        

//		public bool HittedSomething(MouseEventArgs e)
//		{
//			var hits = this.FindHits(e.GetPosition(this));
//			if (hits.Count > 0)
//				return true;
//			return false;
//		}

        
		public List<CuttingPlane> Planes { get; set; }
		public List<BoundingBoxColoring> BlockBoundingBoxes { get; set; }

		public SphereColoring SphereColoring
		{
			get { return sphereColoring; }
			set { sphereColoring = value; }
		}

		public CuttingPlane AddPlane(Color colors, Vector3 minP, Vector3 maxP, float distanceToCentreOfScene, Vector3 normal)
		{

			normal.Normalize();
			var randomR = new Random();
			var plane = new CuttingPlane(minP, maxP, normal, colors.ToColor4(), distanceToCentreOfScene);
			Planes.Add(plane);
			return plane;
		}

		public void RemovePlane(CuttingPlane plane)
		{
			Planes.Remove(plane);
		}


		public void ClearPlanes()
		{
			Planes.Clear();
		}

		public Bitmap MakePhoto(Action restore)
		{
			var args = new MakePhotoParameters()
			{
				Width = (int)ActualWidth,
				Height = (int)ActualHeight,
				//Camera = new OrthographicCamera()
				//{
				//	
				//};
				RestoreAction = restore,
			};

			return this.MakePhoto(args);
		}

		public Bitmap MakePhoto(MakePhotoParameters parameters)
		{
			var prevCamera = Camera;
			var prevDrawBG = IsBackgroundEnabled;
			var prepareAction = parameters.PrepareAction;
			var restoreAction = parameters.RestoreAction;
			parameters.RestoreAction = () =>
			{
				Camera = prevCamera;
				IsBackgroundEnabled = prevDrawBG;
				if (restoreAction != null)
					restoreAction();
			};
			parameters.PrepareAction = () =>
			{
				if (parameters.Camera != null)
					Camera = parameters.Camera;
				IsBackgroundEnabled = parameters.DrawBackground;
				if (prepareAction != null)
					prepareAction();
			};

			return this.RenderHost.MakePhoto(parameters);
		}
    }

	public class IlluminationSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IlluminationSettings"/> class.
		/// </summary>
		public IlluminationSettings()
		{
			Ambient = 0.32;
			Diffuse = 1.2;
			Light = 0.615;
			Shine = 0.1;
			Specular = 0.3;
		}

		public double Ambient { get; set; }
		public double Diffuse { get; set; }
		public double Specular { get; set; }
		public double Shine { get; set; }
		public double Light { get; set; }

		public void Assign(IlluminationSettings value)
		{
			Ambient = value.Ambient;
			Diffuse = value.Diffuse;
			Specular = value.Specular;
			Shine = value.Shine;
			Light = value.Light;
		}
	}
}