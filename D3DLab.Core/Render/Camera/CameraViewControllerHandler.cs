using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using D3DLab.Core.Input.StateMachine;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Render.Camera {
    public abstract class CameraViewControllerHandler<T> where T : CameraD3D {
        protected CameraViewControllerHandler(CameraViewController controller) {
            Controller = controller;
        }

        //TODO: remove all refferenses to CameraViewController
        public CameraViewController Controller { get; set; }


        protected T Camera { get { return (T)Controller.Camera; } }

        public float ViewportWidth { get { return Controller.Control.Width; } }

        public float ViewportHeight { get { return Controller.Control.Height; } }

        public Vector3 RotateCenter { get { return Controller.RotateCenter; } }

        protected abstract float PanK { get; }

        public virtual void Pan(int dx, int dy) {
            var kx = PanK * dx;
            var ky = PanK * dy;
            PanCore(kx, ky);
        }

        public virtual void Pan(Vector2 move) {
            var kx = PanK * move.X;
            var ky = PanK * move.Y;
            PanCore(kx, ky);
        }

        public Vector3 Look {
            get {
                var value = Camera.LookDirection;
                value.Normalize();
                return value;
            }
            set {
                if (value.IsNaN())
                    return;
                value.Normalize();
                Camera.LookDirection = value;
            }
        }

        public Vector3 Up {
            get {
                var value = Camera.UpDirection;
                value.Normalize();
                return value;
            }
            set {
                if (value.IsNaN())
                    return;
                value.Normalize();
                Camera.UpDirection = value;
            }
        }

        public Vector3 Position {
            get {
                var value = Camera.Position;
                return value;
            }
            set {
                if (value.IsNaN())
                    return;
                Camera.Position = value;
            }
        }

        protected void PanCore(float kx, float ky, Action<Vector3> setNewPosition = null) {
            var left = Vector3.Cross(Up, Look);
            left.Normalize();

            var panVector = left * kx + Up * ky;

            if (setNewPosition != null)
                setNewPosition(Position + panVector);
            else
                Position += panVector;
        }

        static float kr = -0.35f;
        public virtual void Rotate(float dx, float dy, CameraRotateMode rotateMode) {
            //return;
            if (dx == 0 && dy == 0)
                return;

            var matrixRotate = rotateMode == CameraRotateMode.Rotate3D ? GetMatrixRotate3D(dx, dy) :
                rotateMode == CameraRotateMode.RotateAroundX ? GetMatrixRotateAroundX(dx, dy) :
                rotateMode == CameraRotateMode.RotateAroundY ? GetMatrixRotateAroundY(dx, dy) :
                rotateMode == CameraRotateMode.RotateAroundZ ? GetMatrixRotateAroundZ(dx, dy) :
                rotateMode == CameraRotateMode.RotateAroundZInverted ? GetMatrixRotateAroundZ(-dx, -dy)
                : Matrix.Identity;

            if (matrixRotate.IsIdentity)
                return;

            Up = Vector3.TransformNormal(Up, matrixRotate);
            Look = Vector3.TransformNormal(Look, matrixRotate);
            Position = Position.TransformToV3(matrixRotate);

        }

        public virtual void Rotate(Vector3 axis, float angle) {
            axis.Normalize();
            axis = Look * axis.Z
                + Up * axis.Y
                + Vector3.Cross(Look, Up).Normalized() * axis.X;
            axis.Normalize();

            var matrixRotate = axis.RotateAroundAxis(angle, RotateCenter);
            if (matrixRotate.IsIdentity)
                return;

            Up = Vector3.TransformNormal(Up, matrixRotate);
            Look = Vector3.TransformNormal(Look, matrixRotate);
            Position = Position.TransformToV3(matrixRotate);

        }

        private Matrix GetMatrixRotate3D(float dx, float dy) {
            var v2Up = new Vector2(0, -1);
            var mouseMove = new Vector2(-dx, -dy);
            var angleLook = Vector2Extensions.AngleBetween(v2Up, mouseMove.Normalized()).ToRad();

            var axis = Vector3.TransformNormal(Up, Matrix.RotationAxis(Look, angleLook + MathUtil.PiOverTwo));
            var angle = mouseMove.Length() * kr;

            var orthCamera = Camera as HelixToolkit.Wpf.SharpDX.OrthographicCamera;

            if (orthCamera != null && orthCamera.Width < 500) {
                var k = ((float)orthCamera.Width / 500f) * 0.7f + 0.3f;
                angle *= k;
            }

            return ToRotateMatrix(axis, angle);
        }

        private Matrix GetMatrixRotateAroundX(float dx, float dy) {
            return ToRotateMatrix(Vector3.Cross(Up, Look), -dy * kr);
        }

        private Matrix GetMatrixRotateAroundY(float dx, float dy) {
            return ToRotateMatrix(Up, dx * kr);
        }

        private Matrix GetMatrixRotateAroundZ(float dx, float dy) {
            var d = Math.Abs(dx) > Math.Abs(dy) ? -dx : dy;
            return ToRotateMatrix(Look, d * kr);
        }

        private Matrix ToRotateMatrix(Vector3 axis, float angle) {
            var matrixRotate = SDXCommonExtensions.RotateAroundAxis(axis, angle, RotateCenter);
            return matrixRotate;
        }

        public abstract void Zoom(int delta, int x, int y);
        public abstract void Zoom(float delta);
    }
}
