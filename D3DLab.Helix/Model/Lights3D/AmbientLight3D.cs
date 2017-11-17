using System.Windows;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class AmbientLight3D : Light3D
	{
		public AmbientLight3D()
		{
			this.Color = new global::SharpDX.Color4(0.2f, 0.2f, 0.2f, 1f);
			this.LightType = LightType.Ambient;
		}

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new AmbientLightRenderData(host);
		}
	}
}
