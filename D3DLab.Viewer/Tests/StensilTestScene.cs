using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Systems;
using D3DLab.Toolkit;
using D3DLab.SDX.Engine;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.D3Objects;
using D3DLab.Toolkit.Host;
using D3DLab.Toolkit.Render;
using D3DLab.Toolkit.Systems;
using D3DLab.Toolkit.Techniques.Lines;
using D3DLab.Toolkit.Techniques.TriangleColored;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.D3D.Systems;

using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows;

namespace D3DLab.Viewer.Tests {
    class StensilTestScene : WFScene {
        public StensilTestScene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, EngineNotificator notify) : base(host, overlay, context, notify) {
        }


        protected override void SceneInitialization(IContextState context, RenderEngine engine, ElementTag camera) {

            var smanager = Context.GetSystemManager();

            smanager.CreateSystem<DefaultInputSystem>();
            smanager.CreateSystem<ZoomToAllObjectsSystem>();
            smanager.CreateSystem<MovingSystem>();
            smanager.CreateSystem<CollidingSystem>();
            smanager.CreateSystem<DefaultOrthographicCameraSystem>();
            smanager.CreateSystem<LightsSystem>();

            smanager
                .CreateSystem<RenderSystem>()
                .Init(engine.Graphics)
                .CreateNested<StensilTest_TriangleColoredVertexRenderTechnique<ToolkitRenderProperties>>()
                .CreateNested<LineVertexRenderTechnique<ToolkitRenderProperties>>()
                ;

            var manager = Context.GetEntityManager();
            cameraObject = CameraObject.UpdateOrthographic<RenderSystem>(camera, Context, Surface);

            LightObject.CreateAmbientLight(manager, 0.2f);//0.05f
            LightObject.CreateFollowCameraDirectLight(manager, System.Numerics.Vector3.UnitZ, 0.8f);//0.95f

            InvokeLoaded();

            {
                var pos0 = new[]{
                new Vector3(0,40,0),
                new Vector3(-30,-20,0),
                new Vector3(30,-20,0),
                };
                var en0 = EntityBuilders.BuildColored(context, pos0,
                   new int[] { 0, 1, 2 }, new[] { Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ },
                     V4Colors.Red, SharpDX.Direct3D11.CullMode.Front);

                var dd = D3DDepthStencilDefinition.Default(0).Description;
                //dd.DepthComparison = Comparison.LessEqual;

                var _default = new D3DDepthStencilDefinition(dd, 0);

                en0.UpdateComponent(RenderableComponent.AsTriangleColoredList(SharpDX.Direct3D11.CullMode.None, _default));
            }
            {
                var pos = new[]{
                    new Vector3(0,20,0),
                    new Vector3(-20,0,0),
                    new Vector3(20,0,0),
                };

                var en1 = EntityBuilders.BuildColored(context, pos,
                    new int[] { 0, 1, 2 }, new[] { Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ },
                      V4Colors.Blue, SharpDX.Direct3D11.CullMode.Front);

                var dd = new DepthStencilStateDescription() {
                    IsDepthEnabled = true,
                    IsStencilEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.LessEqual,
                    StencilWriteMask = 0xFF,
                    StencilReadMask = 0xFF,
                    BackFace = new DepthStencilOperationDescription() {
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Never,
                        DepthFailOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Replace
                    },
                    FrontFace = new DepthStencilOperationDescription() {
                        PassOperation = StencilOperation.Increment,
                        Comparison = Comparison.Never,
                        DepthFailOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Replace
                    }
                };

                var stensilNever = new D3DDepthStencilDefinition(dd, 2);

                en1.UpdateComponent(RenderableComponent.AsTriangleColoredList(SharpDX.Direct3D11.CullMode.None, stensilNever));
            }
            {
                var pos2 = new[]{
                    new Vector3(20,10,0),
                    new Vector3(-20,10,0),
                    new Vector3(0,-10,0),
                };
                var en2 = EntityBuilders.BuildColored(context, pos2,
                    new int[] { 0, 1, 2 }, new[] { Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ },
                      V4Colors.Green, SharpDX.Direct3D11.CullMode.Front);

                var ddd = new DepthStencilStateDescription() {
                    IsDepthEnabled = true,
                    IsStencilEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.LessEqual,
                    StencilWriteMask = 0xFF,
                    StencilReadMask = 0xFF,
                    BackFace = new DepthStencilOperationDescription() {
                        PassOperation = StencilOperation.Replace,
                        Comparison = Comparison.Greater,
                        DepthFailOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Replace
                    },
                    FrontFace = new DepthStencilOperationDescription() {
                        PassOperation = StencilOperation.Increment,
                        Comparison = Comparison.Greater,
                        DepthFailOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Replace
                    }
                };

                var stensilGrater = new D3DDepthStencilDefinition(ddd, 1);

                en2.UpdateComponent(RenderableComponent.AsTriangleColoredList(SharpDX.Direct3D11.CullMode.None, stensilGrater));
            }

        }
    }
}
