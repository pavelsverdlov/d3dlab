using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using SharpDX.Direct3D11;

namespace D3DLab.Core.Render.Context {
    public class EffectContext : IDisposable {
        private EffectsManager effectsManager;

        public EffectContext(EffectsManager effectsManager, Effect effect) {
            Effect = effect;
            this.effectsManager = effectsManager;
        }

        // public ISharpRenderTarget RenderTarget { get; private set; }
        public Effect Effect { get; private set; }

        public EffectsManager EffectsManager {
            get { return effectsManager; }
        }

        public void Dispose() {
            Disposer.RemoveAndDispose(ref effectsManager);
        }
    }
}
