using D3DLab.SDX.Engine.Rendering;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine.Components {
    public class D3DSpherePointRenderComponent : D3DRenderComponent {
        public D3DSpherePointRenderComponent() {
            RasterizerStateDescription = new D3DRasterizerState(new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            });
        }
    }

}
