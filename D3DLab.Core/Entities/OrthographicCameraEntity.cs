using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace D3DLab.Core.Entities {
    public struct CameraData {
        public Vector3 Position { get; set; }
        public Vector3 LookDirection { get; set; }
        public Vector3 UpDirection { get; set; }
        public float NearPlaneDistance { get; set; }
        public int FarPlaneDistance { get; set; }
        public float Width { get; set; }

        public Matrix CreateViewMatrix() {
            if (false) {// this.CreateLeftHandSystem) {
                return global::SharpDX.Matrix.LookAtLH(Position,Position + LookDirection,UpDirection);
            }
            return global::SharpDX.Matrix.LookAtRH(Position,Position + LookDirection,UpDirection);
        }

        public Matrix CreateProjectionMatrix( double aspectRatio) {
            if (false) {// this.CreateLeftHandSystem) {
                return Matrix.OrthoLH((float)Width,(float)(Width / aspectRatio),(float)NearPlaneDistance,(float)FarPlaneDistance);
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
    }

    public interface ICameraComponent {
        CameraData Data { get; }
    }
    public sealed class OrthographicCameraEntity : Entity<CameraData> , ICameraComponent {
        public OrthographicCameraEntity() : base("OrthographicCamera") {
        }
    }

   
}
