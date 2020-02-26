using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace D3DLab.Viewer.D3D {
    using ECS;
    using Render;
    using ECS.Input;
    using ECS.Context;
    using System.Windows.Interop;
    using System.Threading.Tasks;
    using System.Threading;
    using D3DLab.SDX.Engine;
    using D3DLab.Std.Engine.Core.Utilities;
    using System.Numerics;
    using D3DLab.ECS.Systems;
    using D3DLab.Std.Engine.Core.Common;
    using D3DLab.ECS.Ext;
    using D3DLab.ECS.Components;
    using D3DLab.Toolkit.Techniques.TriangleColored;
    using D3DLab.Toolkit.Techniques.OrderIndependentTransparency;
    using D3DLab.Toolkit.Systems;
    using D3DLab.Toolkit.Techniques.TriangleTextured;

    public sealed class GenneralContextState : BaseContextState {
        public GenneralContextState(ContextStateProcessor processor, EngineNotificator notificator) : base(processor, new ManagerContainer(notificator, processor)) {
        }
    }
    public abstract class D3DScene {
        public event Action SceneLoaded = () => { };
       // protected readonly FrameworkElement overlay;
        protected readonly EngineNotificator notify;

        

        /// <summary>
        /// DO NOT CHANGE, PASS OR MAKE SOMETHING STUPID
        /// </summary>
        protected RenderEngine engine;

        public IContextState Context { get; }
        public ISDXSurface Window { get; set; }
        public IInputManager Input => Window.InputManager;

        public D3DScene(ContextStateProcessor context) :
            this( context, new EngineNotificator()) {
        }

        public D3DScene(ContextStateProcessor context, EngineNotificator notify) {
          //  this.overlay = overlay;
            this.Context = context;
            this.notify = notify;
        }

        protected void InitScene() {
           
           
            {   //systems creating
                var smanager = Context.GetSystemManager();

                smanager.CreateSystem<DefaultInputSystem>();
                smanager.CreateSystem<D3DCameraSystem>();
                smanager.CreateSystem<LightsSystem>();
                // smanager.CreateSystem<CollidingSystem>();
                //  smanager.CreateSystem<MovementSystem>();
                //  smanager.CreateSystem<EmptyAnimationSystem>();
                //  smanager.CreateSystem<MeshAnimationSystem>();
                //  smanager.CreateSystem<StickOnHeightMapSystem>();
                //     smanager.CreateSystem<ObjectMovementSystem>();
                //    smanager.CreateSystem<Systems.TerrainGeneratorSystem>();
                //   smanager.CreateSystem<Physics.Engine.PhysicalSystem>();

                smanager
                    .CreateSystem<RenderSystem>()
                    .Init(engine.Graphics)
                     // .CreateNested<SkyGradientColoringRenderTechnique>()
                     //  .CreateNested<SkyPlaneWithParallaxRenderTechnique>()
                     //   .CreateNested<TerrainRenderTechnique>()//
                    
                    //.CreateNested<Toolkit.D3D.CameraViews.CameraViewsRenderTechnique<CustomRenderProperties>>()
                  ///  .CreateNested<OITTriangleColoredVertexRenderTechnique<CustomRenderProperties>>()
                    .CreateNested<TriangleColoredVertexRenderTechnique<CustomRenderProperties>>()
                    .CreateNested<TriangleTexturedVertexRenderTechnique<CustomRenderProperties>>()
                    
                    //.CreateNested<LineVertexRenderTechnique>()
                    //.CreateNested<SpherePointRenderStrategy>()
                    //.CreateNested<AminRenderTechniqueSystem>()
                    ;

                //smanager
                //    .CreateSystem<AminRenderSystem>()
                //    .Init(device);
            }
            {
                //var engine = EngineInfoBuilder.Build(em, Window);
            }

           

            // Toolkit.D3D.CameraViews.CameraViewsObject.Create(em);

        }


        public virtual void Dispose() {
            engine.Dispose();
            //Window.Dispose();
            Context.Dispose();
        }

    }
}
