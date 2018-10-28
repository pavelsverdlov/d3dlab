using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine {

    internal class RenderState : IRenderState {
        public IContextState ContextState { get; set; }
        public GraphicsDevice Graphics;
        public float Ticks { get; set; }
        public IAppWindow Window { get; set; }
        public CameraState Camera { get; set; }
        public LightStructLayout[] Lights;
        public RenderState() {

        }

        public void Dispose() {
        }
    }


    public class RenderSystem : IComponentSystem {
        GraphicsDevice graphics;
        D3DShaderCompilator compilator;
        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;

        internal void Init(GraphicsDevice graphics) {
            this.graphics = graphics;

            compilator = new D3DShaderCompilator();
            compilator.AddResources("Light.hlsl", "D3DLab.SDX.Engine.Rendering.Shaders.Light.hlsl");

            //camera
            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            gameDataBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);

            //lights
            var dinamicLightbuff = new LightStructLayout[3];
            lightDataBuffer = graphics.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructLayout>() * dinamicLightbuff.Length);
        }

        public void Execute(SceneSnapshot snapshot) {
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();

            var visiter = new RenderFrameStrategiesVisitor(snapshot.ContextState);

            using (var state = new RenderState() {
                ContextState = snapshot.ContextState,
                Graphics = graphics,
                Window = snapshot.Window,
                Ticks = (float)snapshot.FrameRateTime.TotalMilliseconds,
                Camera = snapshot.Camera,
                Lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray(),
            }) {
                try {
                    graphics.Refresh();
                    
                    foreach (var entity in emanager.GetEntities().OrderBy(x => x.GetOrderIndex<RenderSystem>())) {
                        foreach (var com in entity.GetComponents<ID3DRenderableComponent>()) {
                            if (!com.Pass.IsCompiled) {
                                com.Pass.Compile(compilator);
                            }
                            com.Accept(visiter);
                        }
                    }

                    foreach (var render in visiter.Strategies) {
                        render.Update(graphics, gameDataBuffer, lightDataBuffer);
                    }

                    {
                        var camera = snapshot.Camera;
                        var lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray();

                        var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, camera.ViewMatrix, camera.ProjectionMatrix);
                        var newlightData = lights;

                        graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameResourceBuffer.RegisterResourceSlot);
                        graphics.UpdateDynamicBuffer(newlightData, lightDataBuffer, LightStructLayout.RegisterResourceSlot);
                    }

                    foreach (var render in visiter.Strategies) {
                        render.Render(graphics);
                    }

                    graphics.Present();
                } catch (Exception ex) {
                    ex.ToString();
                }
            }
        }
    }


    internal class RenderStrategy {
        public void Update(GraphicsDevice Graphics) {

        }
        public void Render(GraphicsDevice Graphics) {

        }
    }

    internal class LineVertexRenderStrategy : IRenderStrategy {
        readonly ElementTag entityTag;
        public LineVertexRenderStrategy(ElementTag entityTag) {
            this.entityTag = entityTag;
        }

        public void Update(GraphicsDevice Graphics) { }
        public void Render(GraphicsDevice Graphics) { }
    }

    internal interface IVisitFactoryMethod<TCom> {
        void Visit(TCom com);
    }

    internal class RenderFrameStrategiesVisitor : IVisitFactoryMethod<Components.D3DColoredVertexesRenderComponent> {
        readonly IContextState contextState;

        public List<IRenderStrategy> Strategies { get; set; }

        public RenderFrameStrategiesVisitor() {
            Strategies = new List<IRenderStrategy>();
        }

        public RenderFrameStrategiesVisitor(IContextState contextState) {
            this.contextState = contextState;
        }

        public void Visit(Components.D3DColoredVertexesRenderComponent component) {
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            Strategies.Add(new ColoredVertexesRenderStrategy(component, geometry));
        }

        public void Visit(Components.D3DLineVertexRenderComponent component) {
            Strategies.Add(new LineVertexRenderStrategy(component.EntityTag));

        }
    }
}
