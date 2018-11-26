using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Systems;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.SDX.Engine {
    public class D3DCameraSystem : CameraSystem {

        class D3DMoveHandler : OrthographicCameraMoveHandler {
            public D3DMoveHandler(OrthographicCameraComponent camera, SceneSnapshot snapshot) : base(camera, snapshot) {}

            public override void Execute(ZoomComponent component) {
                var winW = snapshot.Window.Width;
                var winH = snapshot.Window.Height;
                float delta = component.Delta;
                var screen = component.MovementData.End;

                var c = ViewportEx.UnProject(camera, winW, winH, screen);

                var plane = new SharpDX.Plane(camera.Position.ToSDXVector3(), camera.LookDirection.ToSDXVector3());
                var ray = new SharpDX.Ray(c.Origin.ToSDXVector3(), -c.Direction.ToSDXVector3());
                var inter = plane.Intersects(ref ray, out SharpDX.Vector3 point);

                var zoomAround = new Vector3(point.X, point.Y, point.Z);
                delta = delta * 0.001f;
                if (ChangeCameraDistance(ref delta, zoomAround)) {
                    // Modify the camera width
                    camera.Width *= (float)Math.Pow(2.5f, delta);
                    System.Diagnostics.Trace.WriteLine($"W:{camera.Width}, D: {delta}, Center:{point}");
                }

                base.Execute(component);
            }
            bool ChangeCameraDistance(ref float delta, Vector3 zoomAround) {
                // Handle the 'zoomAround' point
                var target = camera.Position + camera.LookDirection;
                var relativeTarget = zoomAround - target;
                var relativePosition = zoomAround - camera.Position;
                if (relativePosition.Length() < 1e-4) {
                    if (delta > 0) //If Zoom out from very close distance, increase the initial relativePosition
                    {
                        relativePosition.Normalize();
                        relativePosition /= 10;
                    } else//If Zoom in too close, stop it.
                      {
                        return false;
                    }
                }
                var f = Math.Pow(2.5, delta);
                var newRelativePosition = relativePosition * (float)f;
                var newRelativeTarget = relativeTarget * (float)f;

                var newTarget = zoomAround - newRelativeTarget;
                var newPosition = zoomAround - newRelativePosition;

                var newDistance = (newPosition - zoomAround).Length();
                var oldDistance = (camera.Position - zoomAround).Length();

                if (newDistance > camera.FarPlaneDistance && (oldDistance < camera.FarPlaneDistance || newDistance > oldDistance)) {
                    var ratio = (newDistance - camera.FarPlaneDistance) / newDistance;
                    f *= 1 - ratio;
                    newRelativePosition = relativePosition * (float)f;
                    newRelativeTarget = relativeTarget * (float)f;

                    newTarget = zoomAround - newRelativeTarget;
                    newPosition = zoomAround - newRelativePosition;
                    delta = (float)(Math.Log(f) / Math.Log(2.5));
                }

                if (newDistance < camera.NearPlaneDistance && (oldDistance > camera.NearPlaneDistance || newDistance < oldDistance)) {
                    var ratio = (camera.NearPlaneDistance - newDistance) / newDistance;
                    f *= (1 + ratio);
                    newRelativePosition = relativePosition * (float)f;
                    newRelativeTarget = relativeTarget * (float)f;

                    newTarget = zoomAround - newRelativeTarget;
                    newPosition = zoomAround - newRelativePosition;
                    delta = (float)(Math.Log(f) / Math.Log(2.5));
                }

                var newLookDirection = newTarget - newPosition;
                camera.LookDirection = newLookDirection;
                camera.Position = newPosition;
                return true;
            }

        }

        protected override IMovementComponentHandler CreateHandlerOrthographicHandler(OrthographicCameraComponent com, SceneSnapshot snapshot) {
            return new D3DMoveHandler(com, snapshot);
        }
    }
}
