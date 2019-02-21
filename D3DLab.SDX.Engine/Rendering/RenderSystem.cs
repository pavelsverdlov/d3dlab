using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.Rendering {
    public class RenderSystem : BaseComponentSystem, IComponentSystem, IShadersContainer {
        SynchronizedGraphics graphics;

        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;
        RenderStrategyRegistrator visiter;

        internal void Init(SynchronizedGraphics graphics) {
            this.graphics = graphics;
            graphics.Changed += UpdateBuffers;

            UpdateBuffers(graphics.Device);

            visiter = new RenderStrategyRegistrator(graphics.Device.Compilator);
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
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();
            var Ticks = (float)snapshot.FrameRateTime.TotalMilliseconds;

            try {
                using (var frame = graphics.Device.FrameBegin()) {

                    foreach (var entity in emanager.GetEntities().OrderBy(x => x.GetOrderIndex<RenderSystem>())) {
                        visiter.Register(entity);
                    }

                    var camera = snapshot.Camera;
                    var lights = snapshot.Lights.Select(x => x.GetStructLayoutResource()).ToArray();
                    var gamebuff = new GameStructBuffer(camera.ViewMatrix, camera.ProjectionMatrix, camera.LookDirection);

                    frame.Graphics.UpdateSubresource(ref gamebuff, gameDataBuffer, GameStructBuffer.RegisterResourceSlot);
                    frame.Graphics.UpdateDynamicBuffer(lights, lightDataBuffer, LightStructBuffer.RegisterResourceSlot);

                    foreach (var str in visiter.Strategies) {
                        // frame.Graphics.Refresh();
                        str.Render(frame.Graphics, gameDataBuffer, lightDataBuffer);
                        // frame.Graphics.Present();
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
                visiter.Cleanup();
            }

        }

        #region IShaderEditingSystem

        public IRenderTechniquePass[] Pass => visiter.Strategies.SelectMany(x => x.GetPass()).ToArray();

        public IShaderCompilator GetCompilator() {
            return graphics.Device.Compilator;
        }

        #endregion
    }
}
