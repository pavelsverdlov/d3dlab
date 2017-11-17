using HelixToolkit.Wpf.SharpDX.WinForms;
namespace HelixToolkit.Wpf.SharpDX
{
    using global::SharpDX;

    using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;

	public interface IRenderHostInternal
	{
		/// <summary>
		/// The instance of currently attached IRenderable - this is in general the Viewport3DX
		/// </summary>
		//IRenderer Renderable { get; set; }

		void AddDetachedRenderData(RenderData data);
		void RemoveRenderData(RenderData data);
	}

	public interface IRenderHost {
		RenderContext RenderContext { get; }

		SharpDevice SharpDevice { get; }
//		[System.Obsolete("")]
//		Device Device { get; }
		EffectsManager EffectsManager { get; }
       // Color4 ClearColor { get; }
        bool IsShadowMapEnabled { get; }
      //  IRenderer Renderable { get;  }
        void SetDefaultRenderTargets();

        /// <summary>
        /// This technique is used for the entire render pass 
        /// by all Element3D if not specified otherwise in
        /// the elements itself
        /// </summary>
        RenderTechnique RenderTechnique { get; }

     
        System.Drawing.Bitmap MakePhoto(MakePhotoParameters parameters);
    }

    public interface IFormsHost {
        double ActualHeight { get; set; }
        double ActualWidth { get; set; }
        System.Windows.Input.Cursor Cursor { get; set; }
        System.Windows.Forms.Control Child { get; set; }
        void Measure(System.Windows.Size availableSize);
        void Arrange(System.Windows.Rect finalRect);

    }
}
