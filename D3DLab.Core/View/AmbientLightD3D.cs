using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Components;
using D3DLab.Core.Visual3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using RenderContext = D3DLab.Core.Render.Context.RenderContext;

namespace D3DLab.Core.View {
    public sealed class AmbientLightD3D : LightD3D, IRender {
        private DirectionalLightRenderData data;

        public AmbientLightD3D() {
            RenderTechnique = Techniques.RenderPhong;
            this.Color = new global::SharpDX.Color4(0.2f, 0.2f, 0.2f, 1f);
            //            this.LightType = LightType.Ambient;
            data = new DirectionalLightRenderData(Techniques.RenderPhong);
        }

        public void Update(SharpDevice device) {
//            var renderData = data;
//            renderData.Visible = true;
//            renderData.Color = Color;
          //  renderData.Direction = Direction;
        }
        public void Render(RenderContext context) {
            context.TechniqueContext.Variables.LightCount.Set(context.LightContext.lightCount);
            context.TechniqueContext.Variables.LightAmbient.Set(Color);
        }
    }
}
