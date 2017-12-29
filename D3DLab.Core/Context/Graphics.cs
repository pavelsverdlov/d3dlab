using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;

namespace D3DLab.Core.Context {
    public sealed class Graphics {
        public SharpDevice SharpDevice { get; set; }

        public EffectVariables Variables(RenderTechnique technique) {
            return EffectsManager.GetEffect(technique).Variables();
        }
        public EffectsManager EffectsManager { get; set; }
    }
}