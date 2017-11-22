using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;

namespace D3DLab.Core.Render {
    public sealed class DirectionalLightRenderComponent : Component, IRenderComponent,
         IAttachTo<DirectionalLightEntity> {
//        private DirectionalLightRenderData data;

        public DirectionalLightRenderComponent() {
//            data = new DirectionalLightRenderData(Techniques.RenderPhong);
//            data.Name = "DirectionalLight";
//            data.Attach();
        }

        public void Update(Graphics graphics) {
            base.Update();
//            data.Visible = true;
//            data.Color = parent.Data.Color;
//            data.Direction = parent.Data.Direction;
        }

        public void Render(World world, Graphics graphics) {
            world.LightCount++;
            var variables = graphics.Variables(Techniques.RenderPhong);
            variables.LightCount.Set(world.LightCount);//context.LightContext.lightCount
                                                       /// --- set lighting parameters
            //			context.LightContext.lightColors[0] = data.Color *1 /*(float)context.IlluminationSettings.Light*/;
            //            context.LightContext.lightTypes[0] = (int)Light3D.Type.Directional;

            //else
            //{
            //    // --- turn-off the light
            //    lightColors[lightIndex] = new global::SharpDX.Color4(0, 0, 0, 0);
            //}

            /// --- set lighting parameters
            //context.LightContext.lightDirections[0] = -Direction.ToVector4();

            /// --- update lighting variables               
            
            //variables.LightDir.Set(-world.Camera.LookDirection);

            variables.LightColor.Set(new[] { parent.Data.Color });//context.LightContext.lightColors);
            variables.LightType.Set(new[] { 1 /* (int)Light3D.Type.Directional*/ });//context.LightContext.lightTypes);


            /// --- if shadow-map enabled
            if (false) {
                /// update shader
                //                graphics.Variables.LightView.SetMatrix(context.LightContext.lightViewMatrices);
                //                graphics.Variables.LightProj.SetMatrix(context.LightContext.lightProjMatrices);
            }
            //data.Render(context);
        }

        private DirectionalLightEntity parent;
        public void OnAttach(DirectionalLightEntity parent) {
            this.parent = parent;
        }
    }
}
