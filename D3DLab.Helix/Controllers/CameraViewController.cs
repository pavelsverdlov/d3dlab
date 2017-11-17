using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using Point3D = System.Windows.Media.Media3D.Point3D;
using System.Reflection;
using System.Windows.Forms;

namespace HelixToolkit.Wpf.SharpDX.Controllers {
    public class CameraViewController {
        public Control Control { get; set; }
        // public readonly System.Windows.Forms.Control Child;
        public readonly Camera Camera;

        public CameraViewController(Camera camera, ICameraViewControllerHandler handler, System.Windows.Forms.Control control) {
            Control = control;
//            this.Child = renderHost.Child;
           // Viewport = viewport;
            this.Camera = camera;
            //viewport.CameraChanged += viewport_CameraChanged;
            controllerHandle = handler;//new OrthographicCameraViewControllerHandler(this)
            LeftRightPanSensitivity = 1;
            UpDownPanSensitivity = 1;
            rotateCenter = new Point3D(0,0,0);
        }

        public ProjectionCamera ActualCamera { get { return (ProjectionCamera)Camera; } }

        public System.Windows.Media.Media3D.Point3D CameraTarget {
            get { return this.CameraPosition + this.CameraLookDirection; }
            set { this.CameraLookDirection = value - this.CameraPosition; }
        }

        public System.Windows.Media.Media3D.Point3D CameraPosition {
            get { return Camera.Position; }
            set { Camera.Position = value; }
        }

        public System.Windows.Media.Media3D.Vector3D CameraLookDirection {
            get { return Camera.LookDirection; }
            set { Camera.LookDirection = value; }
        }

        public System.Windows.Media.Media3D.Vector3D CameraUpDirection {
            get { return Camera.UpDirection; }
            set { Camera.UpDirection = value; }
        }
        
        public double LeftRightPanSensitivity { get; set; }

        public double UpDownPanSensitivity { get; set; }

      //  public Viewport3DX Viewport { get; private set; }

//        void viewport_CameraChanged(object sender, System.Windows.RoutedEventArgs e) {
//            UpdateCameraController();
//        }

        private readonly ICameraViewControllerHandler controllerHandle;
//        private void UpdateCameraController() {
//            //TODO: move this to ower this class
//            if (Camera == null)
//                controllerHandle = null;
//            else if (Camera is OrthographicCamera)
//                controllerHandle = new OrthographicCameraViewControllerHandler(this);
//            else if (Camera is PerspectiveCamera)
//                controllerHandle = new PerspectiveCameraViewControllerHandler(this);
//            else
//                throw new NotImplementedException();
//        }

        #region Rotate, Pan, Zoom

        private Point3D rotateCenter;
        public Point3D RotateCenter {
            get { return rotateCenter; }
            set {
                if (rotateCenter == value)
                    return;
                rotateCenter = value;
                OnRotateCenterChanged();
            }
        }

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

        #region History

        private readonly List<CameraSetting> cameraHistory = new List<CameraSetting>();
        public void PushCameraSetting() {
            this.cameraHistory.Add(new CameraSetting(this.ActualCamera));
            if (this.cameraHistory.Count > 100) {
                this.cameraHistory.RemoveAt(0);
            }
        }

        public bool RestoreCameraSetting() {
            if (this.cameraHistory.Count > 0) {
                var cs = this.cameraHistory[this.cameraHistory.Count - 1];
                this.cameraHistory.RemoveAt(this.cameraHistory.Count - 1);
                cs.UpdateCamera(this.ActualCamera);
                return true;
            }

            return false;
        }

        #endregion

        #region Events

        private void OnRotateCenterChanged() {
            if (RotateCenterChanged != null)
                RotateCenterChanged(this, new RotateCenterChangedEventArgs(IsManuallyChanged));
        }
        public event EventHandler<RotateCenterChangedEventArgs> RotateCenterChanged;

        #endregion
    }

    public class RotateCenterChangedEventArgs : EventArgs {
        public RotateCenterChangedEventArgs(bool manual) {
            IsManuallyChanged = manual;
        }
        public bool IsManuallyChanged { get; private set; }
    }
}
