using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTerrainRenderComponent : D3DRenderComponent {
        [IgnoreDebuging]
        internal EnumerableDisposableSetter<ShaderResourceView[]> TextureResources { get; set; }
        [IgnoreDebuging]
        internal DisposableSetter<SamplerState> SampleState { get; set; }

        public D3DTerrainRenderComponent() {
            SampleState = new DisposableSetter<SamplerState>(disposer);
            TextureResources = new EnumerableDisposableSetter<ShaderResourceView[]>(disposer);
        }

        public override void Dispose() {
            base.Dispose();
        }
    }
}
