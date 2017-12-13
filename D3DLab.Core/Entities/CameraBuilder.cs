using D3DLab.Core.Render;
using SharpDX;
using System;
using D3DLab.Core.Common;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;

namespace D3DLab.Core.Test {
    public static class CameraBuilder {
        public enum CameraTypes {
            Perspective,
            Orthographic,
        }
        public sealed class CameraComponent : D3DComponent {
            public Vector3 Position { get; set; }
            public Vector3 LookDirection { get; set; }
            public Vector3 UpDirection { get; set; }
            public float NearPlaneDistance { get; set; }
            public int FarPlaneDistance { get; set; }
            public float Width { get; set; }
            public CameraTypes CameraType = CameraTypes.Orthographic;

            public Matrix CreateViewMatrix() {
                if (false) {// this.CreateLeftHandSystem) {
                    return global::SharpDX.Matrix.LookAtLH(Position, Position + LookDirection, UpDirection);
                }
                return global::SharpDX.Matrix.LookAtRH(Position, Position + LookDirection, UpDirection);
            }

            public Matrix CreateProjectionMatrix(double aspectRatio) {
                if (false) {// this.CreateLeftHandSystem) {
                    return Matrix.OrthoLH((float)Width, (float)(Width / aspectRatio), (float)NearPlaneDistance, (float)FarPlaneDistance);
                }
                float halfWidth = (float)(Width * 0.5f);
                float halfHeight = (float)(Width / aspectRatio) * 0.5f;
                Matrix projection;
                OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, (float)NearPlaneDistance, (float)FarPlaneDistance, out projection);
                return projection;
            }
            public static void OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result) {
                float zRange = -2.0f / (zfar - znear);

                result = Matrix.Identity;
                result.M11 = 2.0f / (right - left);
                result.M22 = 2.0f / (top - bottom);
                result.M33 = zRange;
                result.M41 = ((left + right) / (left - right));
                result.M42 = ((top + bottom) / (bottom - top));
                result.M43 = -znear * zRange;
            }

            public Matrix GetFullMatrix(double aspectRatio) {
                return Matrix.Add(CreateViewMatrix(), CreateProjectionMatrix(aspectRatio));
            }

            public override string ToString() {
                return $"[Position:{Position};LookDirection:{LookDirection};UpDirection:{UpDirection};Width:{Width}]";
            }


            public Ray UnProject(Vector2 point2d, IContext ctx)//, out Vector3 pointNear, out Vector3 pointFar)
            {
                var p = new Vector3((float)point2d.X, (float)point2d.Y, 1);

                var vp = ctx.World.ViewMatrix * ctx.World.ProjectionMatrix * ctx.World.GetViewportMatrix();
                var vpi = Matrix.Invert(vp);
                p.Z = 0;
                Vector3.TransformCoordinate(ref p, ref vpi, out Vector3 zn);
                p.Z = 1;
                Vector3.TransformCoordinate(ref p, ref vpi, out Vector3 zf);
                Vector3 r = zf - zn;
                r.Normalize();

                switch (this.CameraType) {
                    case CameraBuilder.CameraTypes.Orthographic:
                        if (double.IsNaN(zn.X) || double.IsNaN(zn.Y) || double.IsNaN(zn.Z)) {
                            zn = new Vector3(0, 0, 0);
                        }
                        if (double.IsNaN(r.X) || double.IsNaN(r.Y) || double.IsNaN(r.Z) ||
                            (r.X == 0 && r.Y == 0 && r.Z == 0)) {
                            r = new Vector3(0, 0, 1);
                        }
                        //fix for not valid inverted matrix
                        return new Ray(zn, r);
                    case CameraBuilder.CameraTypes.Perspective:
                        return new Ray(this.Position, r);
                    default:
                        throw new NotImplementedException();
                }
            }


        }
        public sealed class CameraTechniqueRenderComponent : PhongTechniqueRenderComponent {

            public void Update(Graphics graphics, World world, CameraComponent camera) {
                var variables = graphics.Variables(this.RenderTechnique);
                var aspectRatio = (float)graphics.SharpDevice.Width / graphics.SharpDevice.Height;

                var projectionMatrix = camera.CreateProjectionMatrix(aspectRatio);
                var viewMatrix = camera.CreateViewMatrix();
                var viewport = Vector4.Zero;
                var frustum = Vector4.Zero;
                variables.EyePos.Set(camera.Position);
                variables.Projection.SetMatrix(ref projectionMatrix);
                variables.View.SetMatrix(ref viewMatrix);
                //if (isProjCamera) {
                variables.Viewport.Set(ref viewport);
                variables.Frustum.Set(ref frustum);
                variables.EyeLook.Set(camera.LookDirection);


                world.Position = camera.Position;
                world.LookDirection = camera.LookDirection;
                world.ViewMatrix = viewMatrix;
                world.ProjectionMatrix = projectionMatrix;
            }
        }
        public static Entity BuildOrthographicCamera(IEntityManager context) {
            var entity = context.CreateEntity("OrthographicCamera");

            entity.AddComponent(new CameraComponent {
                Position = new Vector3(0, 0, 300),//50253
                LookDirection = new Vector3(0, 0, -300),
                UpDirection = new Vector3(0, 1, 0),
                NearPlaneDistance = 0,
                FarPlaneDistance = 100500,
                Width = 300
            });
            entity.AddComponent(new CameraTechniqueRenderComponent());

            return entity;
        }
    }


}
