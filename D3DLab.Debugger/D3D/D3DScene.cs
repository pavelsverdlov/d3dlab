using System;
using D3DLab.ECS;
using D3DLab.ECS.Input;
using D3DLab.ECS.Context;
using D3DLab.ECS.Systems;
using D3DLab.Toolkit.Techniques.OrderIndependentTransparency;
using D3DLab.Toolkit.Systems;
using D3DLab.Toolkit.Render;
using D3DLab.Toolkit.Techniques.Lines;
using D3DLab.Toolkit.Input;
using D3DLab.Toolkit.Host;
using System.Windows;
using D3DLab.Toolkit.Input.Publishers;
using D3DLab.Toolkit.D3Objects;
using D3DLab.SDX.Engine.Components;
using D3DLab.Toolkit.Techniques.TriangleColored;

namespace D3DLab.Debugger.D3D {
    public class WFScene : D3DWFScene {
        class EmptyHandler : DefaultInputObserver.ICameraInputHandler {
            public void ChangeRotateCenter(InputStateData state) {
            }

            public void ChangeTransparencyOnObjectUnderCursor(InputStateData state, bool isMmbHolded2sec) {
            }

            public void FocusToObject(InputStateData state) {
            }

            public void HideOrShowObjectUnderCursor(InputStateData state) {
            }
            public void Idle() {
            }
            public void KeywordMove(InputStateData state) {
            }
            public void Pan(InputStateData state) {
            }
            public bool Rotate(InputStateData state) {
                return true;
            }
            public void Zoom(InputStateData state) {
            }
        }

        CameraObject cameraObject;
        BaseInputPublisher publisher;

        public WFScene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, EngineNotificator notify)
            : base(host, overlay, context, notify) {
        }

        protected override DefaultInputObserver CreateInputObserver(WinFormsD3DControl win, FrameworkElement overlay) {
            publisher = new WinFormInputPublisher(win);
            //publisher = new WPFInputPublisher(overlay);
            return new ViewerInputObserver(overlay, publisher, new EmptyHandler());
        }

        protected override void SceneInitialization(IContextState context, RenderEngine engine, ElementTag camera) {
            var smanager = Context.GetSystemManager();

            smanager.CreateSystem<DefaultInputSystem>();
            smanager.CreateSystem<DefaultOrthographicCameraSystem>();
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
                //.CreateNested<OITTriangleColoredVertexRenderTechnique<ToolkitRenderProperties>>()
                .CreateNested<TriangleColoredVertexRenderTechnique<ToolkitRenderProperties>>()
                //.CreateNested<TriangleTexturedVertexRenderTechnique<CustomRenderProperties>>()
                .CreateNested<LineVertexRenderTechnique<ToolkitRenderProperties>>()

                //.CreateNested<LineVertexRenderTechnique>()
                //.CreateNested<SpherePointRenderStrategy>()
                //.CreateNested<AminRenderTechniqueSystem>()
                ;

            //smanager
            //    .CreateSystem<AminRenderSystem>()
            //    .Init(device);Context
            var manager = Context.GetEntityManager();
            cameraObject = CameraObject.UpdateOrthographic<RenderSystem>(camera, Context, Surface);

            LightObject.CreateAmbientLight(manager, 0.2f);//0.05f
            LightObject.CreateFollowCameraDirectLight(manager, System.Numerics.Vector3.UnitZ, 0.8f);//0.95f
        }

        internal void ReCreate(WinFormsD3DControl obj) {
            base.Dispose();

            foreach (var com in Context.GetComponentManager().GetComponents<D3DRenderComponent>()) {
                com.ClearBuffers();
            }

            foreach (var sys in Context.GetSystemManager().GetSystems<RenderSystem>()) {
                sys.Init(engine.Graphics);
            }

            OnHandleCreated(obj);
        }

        public override void Dispose() {
            base.Dispose();
            publisher?.Dispose();
        }
    }
}
