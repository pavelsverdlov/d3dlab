using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.Rendering {
    public class RenderSystem : ContainerSystem<IRenderTechniqueSystem>, IGraphicSystem, IShadersContainer {
        class RenderTechniqueRegistrator {
            public IEnumerable<IRenderTechniqueSystem> Techniques { get { return dic.Values; } }
            readonly Dictionary<Type, IRenderTechniqueSystem> dic;
            readonly List<IRenderTechniqueSystem> allTechniques;

            public RenderTechniqueRegistrator(List<IRenderTechniqueSystem> techniques) {
                dic = new Dictionary<Type, IRenderTechniqueSystem>();
                this.allTechniques = techniques;
            }

            public void Register(GraphicEntity entity) {
                foreach (var tech in allTechniques) {
                    if (tech.IsAplicable(entity)) {
                        GetOrCreate(tech).RegisterEntity(entity);
                    }
                }
            }
            IRenderTechniqueSystem GetOrCreate(IRenderTechniqueSystem obj) {
                var type = obj.GetType();
                if (!dic.TryGetValue(type, out var tech)) {
                    dic[type] = obj;
                    tech = obj;
                }
                return tech;
            }
            public void Cleanup() {
                foreach (var s in Techniques) {
                    s.Cleanup();
                }
            }
        }

        SynchronizedGraphics graphics;

        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;
        
        public RenderSystem() {
        }

        public RenderSystem Init(SynchronizedGraphics graphics) {
            this.graphics = graphics;
            graphics.Changed += UpdateBuffers;
            UpdateBuffers(graphics.Device);
            return this;
        }

        void UpdateBuffers(GraphicsDevice device) {
            //camera
            var gamebuff = new GameStructBuffer(Matrix4x4.Identity, Matrix4x4.Identity, -Vector3.UnitZ);
            gameDataBuffer = device.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);

            //lights
            var dinamicLightbuff = new LightStructBuffer[3];
            lightDataBuffer = device.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructBuffer>() * dinamicLightbuff.Length);
        }

        public void Execute(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            var ticks = (float)snapshot.FrameRateTime.TotalMilliseconds;

            Synchronize();
            var registrator = new RenderTechniqueRegistrator(nested);
            try {
                using (var frame = graphics.Device.FrameBegin()) {

                    foreach (var entity in emanager.GetEntities().OrderBy(x => x.GetOrderIndex<RenderSystem>())) {
                        var renders = entity.GetComponents<D3DRenderComponent>();
                        if (renders.Any() && renders.All(x => x.CanRender)) {
                            if (!entity.Has<IGeometryComponent>() || !entity.Has<D3DTransformComponent>()) {
                                throw new Exception("There are not all necessary components in entity to render.");
                            }
                            registrator.Register(entity);
                        }
                    }

                    var camera = snapshot.Camera;
                    var lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray();
                    var gamebuff = new GameStructBuffer(Matrix4x4.Transpose(camera.ViewMatrix), Matrix4x4.Transpose(camera.ProjectionMatrix),camera.LookDirection);

                    frame.Graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameStructBuffer.RegisterResourceSlot);
                    frame.Graphics.UpdateDynamicBuffer(lights, lightDataBuffer, LightStructBuffer.RegisterResourceSlot);

                    foreach (var str in registrator.Techniques) {
                        str.Render(frame.Graphics, gameDataBuffer, lightDataBuffer);
                    }
                }
            } catch (SharpDX.CompilationException cex) {
                System.Diagnostics.Trace.WriteLine($"CompilationException[\n{cex.Message.Trim()}]");
            } catch (SharpDX.SharpDXException shex) {
                //var reason = frame.Graphics.D3DDevice.DeviceRemovedReason;
                System.Diagnostics.Trace.WriteLine(shex.Message);
                throw shex;
            } catch (Exception ex) {
                throw ex;
            } finally {
                registrator.Cleanup();
                Pass = registrator.Techniques.Select(x => x.GetPass()).ToArray();
            }
        }

        #region IShaderEditingSystem

        public IRenderTechniquePass[] Pass { get; private set; }

        public IShaderCompilator GetCompilator() {
            return graphics.Device.Compilator;
        }

        #endregion
    }
}
