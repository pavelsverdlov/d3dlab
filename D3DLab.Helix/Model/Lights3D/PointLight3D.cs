using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public sealed class PointLight3D : PointLightBase3D
    {
        public PointLight3D()
        {            
            this.LightType = LightType.Point;
        }

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new PointLightRenderData(host);
		}
    }
}
