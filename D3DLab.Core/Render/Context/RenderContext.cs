using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX.Direct3D11;

namespace D3DLab.Core.Render.Context {
    public sealed class RenderContext {
        public RenderContext(
            SharpDevice renderTarget,
            EffectContext effectContext,
            CameraContext cameraContext,
            LightRenderContext lightContext,
            TechniqueContext techniqueContext,
            IlluminationSettings illuminationSettings) {

            RenderTarget = renderTarget;
            EffectContext = effectContext;
            CameraContext = cameraContext;
            LightContext = lightContext;
            TechniqueContext = techniqueContext;
            IlluminationSettings = illuminationSettings;
        }

        public SharpDX.Direct3D11.Device Device {
            get { return RenderTarget.Device; }
        }

        public EffectContext EffectContext { get; private set; }
        public LightRenderContext LightContext { get; private set; }
        public CameraContext CameraContext { get; private set; }
        public TechniqueContext TechniqueContext { get; private set; }
        public IlluminationSettings IlluminationSettings { get; private set; }
        public SharpDevice RenderTarget { get; private set; }

        public bool IsShadowPass { get; set; }
        public bool IsDeferredPass { get; set; }

        public void Dispose() { }

        public void SetCurrentTechnique(RenderTechnique renderTechnique) {
            return;/*
            if (renderTechnique == null)
                throw new ArgumentNullException("renderTechnique", "renderTechnique is null.");

            renderTechnique.UpdateVariables(Effect);

            TechniqueContext ctx;
            if (!contexts.TryGetValue(renderTechnique.Name, out ctx))
                ctx = new TechniqueContext(renderTechnique, EffectsManager);
            TechniqueContext = ctx;*/
        }
    }
}
