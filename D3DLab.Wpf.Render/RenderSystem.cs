using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Toolkit;
using D3DLab.Toolkit._CommonShaders;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace D3DLab.Render {
    public class CustomRenderProperties : IRenderProperties, D3DLab.Toolkit.IToolkitFrameProperties {
        public CameraState CameraState { get; set; }
        SharpDX.Direct3D11.Buffer D3DLab.Toolkit.IToolkitFrameProperties.Game => Game;
        SharpDX.Direct3D11.Buffer D3DLab.Toolkit.IToolkitFrameProperties.Lights => Lights;

        public readonly SharpDX.Direct3D11.Buffer Game;
        public readonly SharpDX.Direct3D11.Buffer Lights;
        public CustomRenderProperties(SharpDX.Direct3D11.Buffer game, SharpDX.Direct3D11.Buffer light, CameraState cameraState) {
            Game = game;
            Lights = light;
            CameraState = cameraState;
        }
    }

    public class RenderSystem : BaseRenderSystem<CustomRenderProperties> {
        CameraState prevCameraState;
        
        SharpDX.Direct3D11.Buffer gameDataBuffer;
        SharpDX.Direct3D11.Buffer lightDataBuffer;

        public RenderSystem() {
            prevCameraState = new CameraState() {
                ViewMatrix = Matrix4x4.Identity,
                ProjectionMatrix = Matrix4x4.Identity,
                LookDirection = -Vector3.UnitZ,
                Position = Vector3.Zero
            };
        }

        protected override void RenderFrame(GraphicsFrame frame, ISceneSnapshot snapshot, RenderTechniqueRegistrator<CustomRenderProperties> registrator) {
            var emanager = ContextState.GetEntityManager();
            var ticks = (float)snapshot.FrameRateTime.TotalMilliseconds;

            foreach (var entity in emanager.GetEntities().OrderBy(x => x.GetOrderIndex<RenderSystem>())) {
                var renders = entity.GetComponents<IRenderableComponent>();
                if (renders.Any() && renders.All(x => x.CanRender)) {
                    //if (!entity.Has<IGeometryComponent>() || !entity.Has<TransformComponent>()) {
                    //    throw new Exception("There are not all necessary components in entity to render.");
                    //}
                    registrator.Register(entity);
                }
            }

            prevCameraState = snapshot.Camera;
            var lights = snapshot.Lights.Select(x => LightStructBuffer.From(x)).ToArray();
            var gamebuff = GameStructBuffer.FromCameraState(prevCameraState);

            frame.Graphics.UpdateDynamicBuffer(ref gamebuff, gameDataBuffer, GameStructBuffer.RegisterResourceSlot);
            frame.Graphics.UpdateDynamicBuffer(lights, lightDataBuffer, LightStructBuffer.RegisterResourceSlot);

            foreach (var str in registrator.Techniques) {
                str.Render(frame.Graphics, new CustomRenderProperties(gameDataBuffer, lightDataBuffer, prevCameraState));
            }
        }

        protected override void UpdateBuffers(GraphicsDevice device) {
            //camera
            var gamebuff = GameStructBuffer.FromCameraState(prevCameraState);
            gameDataBuffer = device.CreateDynamicBuffer(ref gamebuff, GameStructBuffer.Size);
            //.CreateBuffer(BindFlags.ConstantBuffer, ref gamebuff);

            //lights
            var dinamicLightbuff = new LightStructBuffer[3];
            lightDataBuffer = device.CreateDynamicBuffer(dinamicLightbuff,
                Unsafe.SizeOf<LightStructBuffer>() * dinamicLightbuff.Length);
        }

        

    }
}
