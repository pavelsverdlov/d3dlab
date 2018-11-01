using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Rendering {
    public class RenderSystem : BaseComponentSystem, IComponentSystem, IShaderEditingSystem {
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
            var gamebuff = new GameStructBuffer(Matrix4x4.Identity, Matrix4x4.Identity);
            gameDataBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);

            //lights
            var dinamicLightbuff = new LightStructBuffer[3];
            lightDataBuffer = graphics.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructBuffer>() * dinamicLightbuff.Length);

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

                    var camera = snapshot.Camera;
                    var lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray();
                    var gamebuff = new GameStructBuffer(camera.ViewMatrix, camera.ProjectionMatrix);

                    frame.Graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameStructBuffer.RegisterResourceSlot);
                    frame.Graphics.UpdateDynamicBuffer(lights, lightDataBuffer, LightStructBuffer.RegisterResourceSlot);

                    foreach (var str in visiter.Strategies) {
                        str.Render(frame.Graphics, gameDataBuffer, lightDataBuffer);
                    }

                } catch (Exception ex) {
                    ex.ToString();
                } finally {
                    visiter.Cleanup();
                }
            }
        }

        #region IShaderEditingSystem

        public IRenderTechniquePass[] Pass => visiter.Strategies.Select(x => x.GetPass()).ToArray();

        public IShaderCompilator GetCompilator() {
            return compilator;
        }

        #endregion
    }
}
