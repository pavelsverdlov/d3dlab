using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class DirectionalLight3D : Light3D
	{
		public DirectionalLight3D()
		{
			this.Color = global::SharpDX.Color.White;
			this.LightType = LightType.Directional;
		}

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new DirectionalLightRenderData(Techniques.RenderPhong);
		}
	}
}
