using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Components;
using D3DLab.Core.Components.Render;
using D3DLab.Core.Visual3D;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using Component = D3DLab.Core.Components.Component;
using HelixToolkit.Wpf.SharpDX;
using RenderContext = D3DLab.Core.Render.Context.RenderContext;

namespace D3DLab.Core.View {
    public abstract class LightD3D : ComponentContainer {
        public Color4 Color { get; set; }
     
        public HelixToolkit.Wpf.SharpDX.RenderTechnique RenderTechnique { get; set; }
    }

    public class DirectionalLight : LightD3D, IRender {

        public Vector3 Direction { get; set; }
        public DirectionalLight() {
            RenderTechnique = Techniques.RenderPhong;
            this.Color = global::SharpDX.Color.White;
            //  this.LightType = LightType.Directional;
            data = new DirectionalLightRenderData(Techniques.RenderPhong);
            data.Name = "DirectionalLight";
            data.Attach();
        }

        private DirectionalLightRenderData data;
      
      
        public void Update(SharpDevice device) {
            var renderData = data;
            renderData.Visible = true;
            renderData.Color = Color;
            renderData.Direction = Direction;
        }
        public void Render(RenderContext context) {
            context.LightContext.lightCount++;
            context.TechniqueContext.Variables.LightCount.Set(context.LightContext.lightCount);
            context.TechniqueContext.InitEffect(context.EffectContext.EffectsManager);
           // context.TechniqueContext.Variables.LightCount.Set(1);
            context.SetCurrentTechnique(RenderTechnique);
            /// --- set lighting parameters
            context.LightContext.lightColors[0] = Color*1/*(float)context.IlluminationSettings.Light*/;
            context.LightContext.lightTypes[0] = (int)Light3D.Type.Directional;

            //else
            //{
            //    // --- turn-off the light
            //    lightColors[lightIndex] = new global::SharpDX.Color4(0, 0, 0, 0);
            //}

            /// --- set lighting parameters
            context.LightContext.lightDirections[0] = -Direction.ToVector4();

            /// --- update lighting variables               
            context.TechniqueContext.Variables.LightDir.Set(context.LightContext.lightDirections);
            context.TechniqueContext.Variables.LightColor.Set(context.LightContext.lightColors);
            context.TechniqueContext.Variables.LightType.Set(context.LightContext.lightTypes);


            /// --- if shadow-map enabled
            if (false) {
                /// update shader
                context.TechniqueContext.Variables.LightView.SetMatrix(context.LightContext.lightViewMatrices);
                context.TechniqueContext.Variables.LightProj.SetMatrix(context.LightContext.lightProjMatrices);
            }
            //data.Render(context);
        }
        
    }


   
}
