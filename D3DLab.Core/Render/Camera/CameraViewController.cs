using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using D3DLab.Core.Input.StateMachine;
using D3DLab.Core.View;
using SharpDX;
using Point = System.Drawing.Point;

namespace D3DLab.Core.Render.Camera {
    public class CameraViewController {
        public Control Control { get; set; }
        public readonly CameraD3D Camera;
        private readonly OrthographicCameraHandler controllerHandle;

        public CameraViewController(CameraD3D camera, OrthographicCameraHandler handler, System.Windows.Forms.Control control) {
            Control = control;
            this.Camera = camera;
            controllerHandle = handler;
            LeftRightPanSensitivity = 1;
            UpDownPanSensitivity = 1;
            RotateCenter = Vector3.Zero;
        }

        public ProjectionCamera ActualCamera { get { return (ProjectionCamera)Camera; } }

        public Vector3 CameraTarget {
            get { return this.CameraPosition + this.CameraLookDirection; }
            set { this.CameraLookDirection = value - this.CameraPosition; }
        }

        public Vector3 CameraPosition {
            get { return Camera.Position; }
            set { Camera.Position = value; }
        }

        public Vector3 CameraLookDirection {
            get { return Camera.LookDirection; }
            set { Camera.LookDirection = value; }
        }

        public Vector3 CameraUpDirection {
            get { return Camera.UpDirection; }
            set { Camera.UpDirection = value; }
        }

        public float LeftRightPanSensitivity { get; set; }

        public float UpDownPanSensitivity { get; set; }
        
       
        #region Rotate, Pan, Zoom
        
        public Vector3 RotateCenter { get; set; }

        internal bool IsManuallyChanged { get; set; }

        public void Pan(int dx, int dy) {
            if (controllerHandle != null)
                controllerHandle.Pan(dx, dy);
        }

        public void Pan(Vector2 move) {
            if (controllerHandle != null)
                controllerHandle.Pan(move);
        }

        public void Rotate(float dx, float dy, CameraRotateMode rotateMode) {
            if (controllerHandle != null)
                controllerHandle.Rotate(dx, dy, rotateMode);
        }

        public void Rotate(Vector3 axis, float angle) {
            if (controllerHandle != null)
                controllerHandle.Rotate(axis, angle);
        }

        public void Zoom(int delta, int x, int y) {
            if (controllerHandle != null)
                controllerHandle.Zoom(delta, x, y);
        }

        public void Zoom(float delta) {
            if (controllerHandle != null)
                controllerHandle.Zoom(delta);
        }

        public void AddPanForce(int dx, int dy) {
            Pan(dx, dy);
        }

        public void AddRotateForce(int dx, int dy) {
            Rotate(dx, dy, CameraRotateMode.Rotate3D);
        }

        #endregion

      
    }
}
