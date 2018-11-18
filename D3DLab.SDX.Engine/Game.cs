using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Render;
using D3DLab.Std.Engine.Core.Systems;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.SDX.Engine {
    public class D3DEngine : EngineCore {
        readonly SynchronizedGraphics device;

        public D3DEngine(IAppWindow window, IContextState context) : base(window, context) {
            device = new SynchronizedGraphics(window);
        }

        protected override void OnSynchronizing() {
            device.Synchronize();
            base.OnSynchronizing();
        }


        public override void Dispose() {
            base.Dispose();
            device.Dispose();
        }

        protected override void Initializing() {
            var cameraTag = new ElementTag("CameraEntity");

            {   //systems creating
                var smanager = Context.GetSystemManager();

                smanager.CreateSystem<InputSystem>();
                smanager.CreateSystem<CameraSystem>();
                smanager.CreateSystem<LightsSystem>();
                smanager.CreateSystem<RenderSystem>().Init(device);
                
            }
            {   //default entities
                var em = Context.GetEntityManager();

                EngineInfoBuilder.Build(em);

                em.CreateEntity(cameraTag)
                    .AddComponent(new D3DCameraComponent(Window.Width, Window.Height));

                em.CreateEntity(new ElementTag("AmbientLight"))
                    .AddComponent(new D3DLightComponent {
                        Index = 0,
                        Intensity = 0.2f,
                        //Position = Vector3.Zero + Vector3.UnitZ * 1000,
                        Type = LightTypes.Ambient })
                    .AddComponent(new ColorComponent { Color = new Vector4(1,1,1,1) });

                em.CreateEntity(new ElementTag("PointLight"))
                    .AddComponent(new D3DLightComponent {
                        Index = 1,
                        Intensity = 0.6f,
                        //Position = new Vector3(2, 1, 0),
                        Position = Vector3.Zero + Vector3.UnitZ * 1000,
                        Type = LightTypes.Point
                    })
                    .AddComponent(new ColorComponent { Color = new Vector4(1, 1, 1, 1) });

                em.CreateEntity(new ElementTag("DirectionLight"))
                    .AddComponent(new D3DLightComponent {
                        Index = 2,
                        Intensity = 0.2f,
                        Direction = new Vector3(1, 4, 4).Normalize(),
                        Type = LightTypes.Directional
                    })
                    .AddComponent(new ColorComponent { Color = new Vector4(1, 1, 1, 1) });


            }
            {//entities ordering 
                Context.EntityOrder
                       .RegisterOrder<RenderSystem>(cameraTag, 0)
                       .RegisterOrder<InputSystem>(cameraTag, 0);
            }
        }
    }
}
