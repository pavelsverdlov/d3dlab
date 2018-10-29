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

namespace D3DLab.SDX.Engine.Rendering {

    public class RenderSystem : IComponentSystem {
        GraphicsDevice graphics;
        D3DShaderCompilator compilator;

        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;
        RenderFrameStrategiesVisitor visiter;

        internal void Init(GraphicsDevice graphics) {
            this.graphics = graphics;

            compilator = new D3DShaderCompilator();
            compilator.AddIncludeMapping("Game", "D3DLab.SDX.Engine.Rendering.Shaders.Game.hlsl");
            compilator.AddIncludeMapping("Light", "D3DLab.SDX.Engine.Rendering.Shaders.Light.hlsl");

            //camera
            var gamebuff = new GameResourceBuffer(Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity);
            gameDataBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);

            //lights
            var dinamicLightbuff = new LightStructLayout[3];
            lightDataBuffer = graphics.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructLayout>() * dinamicLightbuff.Length);

            visiter = new RenderFrameStrategiesVisitor(compilator);
        }

        public void Execute(SceneSnapshot snapshot) {
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();
            var Ticks = (float)snapshot.FrameRateTime.TotalMilliseconds;

            visiter.SetContext(snapshot.ContextState);

            using (var frame = graphics.FrameBegin()) {
                try {
                    foreach (var entity in emanager.GetEntities().OrderBy(x => x.GetOrderIndex<RenderSystem>())) {
                        foreach (var com in entity.GetComponents<ID3DRenderableComponent>()) {
                            com.Accept(visiter);
                        }
                    }

                    graphics.Refresh();

                    {
                        var camera = snapshot.Camera;
                        var lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray();

                        graphics.UpdateDynamicBuffer(lights, lightDataBuffer, LightStructLayout.RegisterResourceSlot);
                    }

                    foreach (var str in visiter.Strategies) {
                        str.Update(graphics, gameDataBuffer, lightDataBuffer, snapshot.Camera);
                    }

                    graphics.Present();
                } catch (Exception ex) {
                    ex.ToString();
                } finally {
                    visiter.Cleanup();
                }
            }
        }
    }

    

    internal class RenderFrameStrategiesVisitor {
        IContextState contextState;
        readonly D3DShaderCompilator compilator;

        public IEnumerable<IRenderStrategy> Strategies { get { return dic.Values; } }
        readonly Dictionary<Type, IRenderStrategy> dic;

        public RenderFrameStrategiesVisitor(D3DShaderCompilator compilator) {
            this.compilator = compilator;
            dic = new Dictionary<Type, IRenderStrategy>();
        }

        public void SetContext(IContextState contextState) {
            this.contextState = contextState;
        }

        public void Visit(Components.D3DColoredVertexesRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            GetOrCreate(() => new ColoredVertexesRenderStrategy(compilator))
                .RegisterEntity(component, geometry);
        }

        public void Visit(Components.D3DLineVertexRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            GetOrCreate(() => new LineVertexRenderStrategy(compilator))
                .RegisterEntity(component, geometry);
        }

        T GetOrCreate<T>(Func<T> creator) where T : IRenderStrategy {
            var type = typeof(T);
            if (!dic.TryGetValue(type, out var str)) {
                dic[type] = creator();
                str = dic[type];
            }

            return (T)str;
        }

        public void Cleanup() {
            foreach( var s in Strategies) {
                s.Cleanup();
            }
        }
    }
}
