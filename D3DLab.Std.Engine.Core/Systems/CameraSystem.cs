using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Linq;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Systems {   
    public class CameraSystem : BaseComponentSystem, IComponentSystem {

        protected class OrthographicCameraMoveHandler : IMovementComponentHandler {
            protected readonly OrthographicCameraComponent camera;
            protected readonly SceneSnapshot snapshot;

            public OrthographicCameraMoveHandler(OrthographicCameraComponent camera, SceneSnapshot snapshot) {
                this.camera = camera;
                this.snapshot = snapshot;
            }

            public void Execute(RotationComponent component) {
                var state = component.State;
                var data  = component.MovementData;

                //Utilities.Helix.CameraMath.RotateTrackball(Utilities.Helix.CameraMode.Inspect,
                //    ref p11, ref p2, ref rotp, 1f, 960, 540, ccom, 1, out var newpos, out var newlook, out var newup);
                //Utilities.Helix.CameraMath.RotateTurnball(Utilities.Helix.CameraMode.Inspect,
                //    ref p11, ref p2, ref rotp, 1f, 960, 540, ccom, 1, out var newpos, out var newlook, out var newup);
                //Utilities.Helix.CameraMath.RotateTurntable(Utilities.Helix.CameraMode.Inspect,
                //    ref moveV, ref rotp, 1f, 960, 540, new  , 1, UpDirection.Value, out var newpos, out var newlook, out var newup);

                var rotateAround = camera.RotatePoint;
                var delta = data.End - data.Begin;
                var relativeTarget = rotateAround - state.Target;
                var relativePosition = rotateAround - state.Position;

                var cUp = Vector3.Normalize(state.UpDirection);
                var up = state.UpDirection;
                var dir = Vector3.Normalize(state.LookDirection);
                var right = Vector3.Cross(dir, cUp);

                float d = -0.5f;
                d *= 1;

                var xangle = d * 1 * delta.X / 180 * (float)Math.PI;
                var yangle = d * delta.Y / 180 * (float)Math.PI;

                System.Diagnostics.Trace.WriteLine($"up: {up}/{xangle}, right: {right}/{yangle}");

                var q1 = Quaternion.CreateFromAxisAngle(up, xangle);
                var q2 = Quaternion.CreateFromAxisAngle(right, yangle);
                Quaternion q = q1 * q2;

                var m = Matrix4x4.CreateFromQuaternion(q);

                var newRelativeTarget = Vector3.Transform(relativeTarget, m);
                var newRelativePosition = Vector3.Transform(relativePosition, m);

                var newTarget = rotateAround - newRelativeTarget;
                var newPosition = rotateAround - newRelativePosition;

                camera.UpDirection = Vector3.TransformNormal(cUp, m);
                camera.LookDirection = (newTarget - newPosition);
                camera.Position = newPosition;
            }

            public virtual void Execute(ZoomComponent component) {

            }
        }

        protected virtual IMovementComponentHandler CreateHandlerOrthographicHandler(OrthographicCameraComponent com, SceneSnapshot snapshot) {
            return new OrthographicCameraMoveHandler(com, snapshot);
        }

        public void Execute(SceneSnapshot snapshot) {
            var window = snapshot.Window;
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();

            try {
                foreach (var entity in emanager.GetEntities()) {
                    foreach (var com in entity.GetComponents<OrthographicCameraComponent>()) {
                        entity.GetComponents<MovementComponent>().DoFirst(movment=> {
                            movment.Execute(CreateHandlerOrthographicHandler(com,snapshot));
                        });

                        com.UpdateViewMatrix();
                        com.UpdateProjectionMatrix(window.Width, window.Height);

                        snapshot.UpdateCamera(com.GetState());
                    }
                }
            } catch (Exception ex) {
                ex.ToString();
                throw ex;
            }
        }

       
    }
}
