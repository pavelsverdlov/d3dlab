using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using D3DLab.Core.Components;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using ProjectionCamera = HelixToolkit.Wpf.SharpDX.ProjectionCamera;

namespace D3DLab.Core.Render.Camera {
    public abstract class ProjectionCamera : CameraD3D {
        private float farPlaneDistance;
        private Vector3 lookDirection;
        private float nearPlaneDistance;
        private Vector3 position;
        private Vector3 upDirection;

        public event Action CameraChanged = () => { };
        public bool CreateLeftHandSystem { get; set; }
        public float FarPlaneDistance {
            get { return farPlaneDistance; }
            set {
                farPlaneDistance = value;
                OnCameraChanged();
            }
        }
        public override Vector3 LookDirection {
            get { return lookDirection; }
            set {
                lookDirection = value;
                OnCameraChanged();
            }
        }
        public float NearPlaneDistance {
            get { return nearPlaneDistance; }
            set {
                nearPlaneDistance = value;
                OnCameraChanged();
            }
        }
        public override Vector3 Position {
            get { return position; }
            set {
                position = value;
                OnCameraChanged();
            }
        }
        public override Vector3 UpDirection {
            get { return upDirection; }
            set {
                upDirection = value;
                OnCameraChanged();
            }
        }
        public Vector3 Target {
            get { return this.Position + this.LookDirection; }
        }

        public override Matrix CreateViewMatrix() {
            if (this.CreateLeftHandSystem) {
                return global::SharpDX.Matrix.LookAtLH(
                    this.Position,
                    this.Position + this.LookDirection,
                    this.UpDirection);
            }

            return global::SharpDX.Matrix.LookAtRH(
                this.Position,
                this.Position + this.LookDirection,
                this.UpDirection);
        }

        protected void OnCameraChanged() {
            CameraChanged();
        }
    }

    public class OrthographicCamera : ProjectionCamera {
        private float width;
        public float Width {
            get { return width; }
            set {
                if (width == value)
                    return;
                width = value;
                OnCameraChanged();
            }
        }

        public OrthographicCamera() {
            // default values for near-far must be different for ortho:
            NearPlaneDistance = -10.0f;
            FarPlaneDistance = 100.0f;
        }
        
        public override Matrix CreateProjectionMatrix(double aspectRatio) {
            if (this.CreateLeftHandSystem) {
                return Matrix.OrthoLH(
                    (float)this.Width,
                    (float)(this.Width / aspectRatio),
                    (float)this.NearPlaneDistance,
                    (float)this.FarPlaneDistance);
            }
            float halfWidth = (float)(Width * 0.5f);
            float halfHeight = (float)((this.Width / aspectRatio) * 0.5f);
            Matrix projection;
            OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, (float)this.NearPlaneDistance, (float)this.FarPlaneDistance, out projection);
            return projection;
        }

        //the right way to calculate orthographic projection matrix
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

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.
        /// </summary>
        /// <returns>
        /// The new instance.
        /// </returns>
        //        protected override Freezable CreateInstanceCore() {
        //            return new HelixToolkit.Wpf.SharpDX.OrthographicCamera();
        //        }

        //        public bool TestBlockInView(Viewport3DX viewport, BoundingBox blockBounds) {
        //            var bf = new BoundingFrustum(this.GetViewProjectionMatrix(viewport.ActualWidth / viewport.ActualHeight));
        //            return bf.Intersects(ref blockBounds);
        //        }
        
    }
}
