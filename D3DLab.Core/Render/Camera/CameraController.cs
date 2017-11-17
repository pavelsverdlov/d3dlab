using System;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Render.Camera {
    public enum CameraPositionTypes {
        FrontView = 0,
        LeftView,
        BackView,
        RightView,
        TopView,
        BottomView
    }

    internal static class Ex {
        public static Point3D Move(this Point3D point, Vector3D vector) {
            point.Offset(vector.X, vector.Y, vector.Z);
            return point;
        }

        public static Vector3D ToVector3D(this Point3D n) {
            return new Vector3D(n.X, n.Y, n.Z);
        }
    }

    public sealed class CameraController {
        public readonly Render.Camera.OrthographicCamera cameraController;

        public CameraController(Render.Camera.OrthographicCamera cameraController) {
            this.cameraController = cameraController;
            InitializeDefaultView(new Point3D(0, 0, 300), new Vector3D(0, 1, 0));
        }

        private Tuple<CameraPositionTypes, Point3D, Vector3D>[] viewInfos;

        public void InitializeDefaultView(Point3D cameraPosition, Vector3D cameraUpDirection) {
            var cameraPositionVector = new Vector3D(cameraPosition.X, cameraPosition.Y, cameraPosition.Z);
            cameraUpDirection.Normalize();
            cameraPositionVector.Normalize();
            var cross = Vector3D.CrossProduct(cameraPositionVector, cameraUpDirection);
            cross.Normalize();
            viewInfos = new[] {
                Tuple.Create(CameraPositionTypes.BackView, new Point3D(-cameraUpDirection.X, -cameraUpDirection.Y, -cameraUpDirection.Z), cameraPositionVector),
                Tuple.Create(CameraPositionTypes.FrontView, new Point3D(cameraUpDirection.X, cameraUpDirection.Y, cameraUpDirection.Z), cameraPositionVector),
                Tuple.Create(CameraPositionTypes.LeftView, new Point3D(cross.X, cross.Y, cross.Z), cameraPositionVector),
                Tuple.Create(CameraPositionTypes.RightView, new Point3D(-cross.X, -cross.Y, -cross.Z), cameraPositionVector),
                Tuple.Create(CameraPositionTypes.TopView, new Point3D(cameraPositionVector.X, cameraPositionVector.Y, cameraPositionVector.Z), cameraUpDirection),
                Tuple.Create(CameraPositionTypes.BottomView, new Point3D(-cameraPositionVector.X, -cameraPositionVector.Y, -cameraPositionVector.Z), cameraUpDirection)
            };
        }

        //		public void SetCamera(CameraPositionTypes viewType, BoundingBox box, double width = 300) {
        //			SetCamera(viewType, default(Point3D), 1, box, width);
        //        }

        public void SetCamera(CameraPositionTypes viewType, BoundingBox box, Point3D center = default(Point3D)) {
            SetCamera(viewType, center, 1, box);
        }

        public void SetCamera(CameraPositionTypes viewType, Point3D center = default(Point3D), 
            float scale = 1, BoundingBox box = new BoundingBox(), float width = 300) {
            if ((box.Maximum - box.Minimum).Length() > 0) {
                float calcwidth = (float) (box.SizeX() * 2.5f);
                width = calcwidth < width ? width : calcwidth;
            }

            var info = viewInfos.First(i => i.Item1 == viewType);//Vector3D.CrossProduct()
            cameraController.Position = info.Item2
                .Multiply(width * 2)
                .Move(center.ToVector3D()).ToVector3();
            cameraController.LookDirection = info.Item2
                .Multiply(-width)
                .ToVector3D().ToVector3();

            cameraController.UpDirection = info.Item3.ToVector3();
         //   Viewport.InputController.CameraViewController.RotateCenter = center;

            var orthographic = cameraController as OrthographicCamera;
            if (orthographic != null) {
                orthographic.NearPlaneDistance = 0.001f;
                orthographic.Width = width / (scale == 0.0f || scale == float.NaN ? 1.0f: scale);
            }
        }

//        public void SetCamera(Vector3D lookDirection, Vector3D upDirection, Point3D position, CustomView.CameraSettings settings, Point3D center = default(Point3D), double scale = 1, Point3D cameraTarget = default(Point3D)) {
//            cameraController.CameraLookDirection = lookDirection;
//            cameraController.CameraPosition = position;
//
//            var upPerpendicular = Vector3D.CrossProduct(Vector3D.CrossProduct(lookDirection, upDirection), lookDirection);
//
//            cameraController.CameraUpDirection = upPerpendicular;
//
////            Viewport.InputController.CameraViewController.RotateCenter = center;
//            if (!cameraTarget.Equals(default(Point3D))) {
////                Viewport.InputController.CameraViewController.CameraTarget = cameraTarget;
//            }
//            var orthographic = cameraController.Camera as OrthographicCamera;
//            if (orthographic != null) {
//                orthographic.NearPlaneDistance = 0.001;
//                orthographic.Width = settings != null ? settings.Width : 250 / (scale == 0.0 || scale == double.NaN ? 1.0 : scale);
//            }
//        }

//        public Vector3D GetViewVector(CameraPositionTypes viewType) {
//            return viewInfos.First(i => i.Item1 == viewType).Item2.ToVector3D();
//        }

//        public void ZoomToVisual(Zirkonzahn.Visualization.SharpDX.Common.VisualGeometryModel visual, double width = Double.NaN) {
//            var camera = (OrthographicCamera)Viewport.CameraController.ActualCamera;
//
//            var vUp = camera.UpDirection;
//            vUp.Normalize();
//
//            var center = visual.GetVisualCenter();
//            var lookDirection = Viewport.CameraController.CameraLookDirection;
//            lookDirection.Normalize();
//            var newPosition = center - lookDirection * 300.0;
//
//            var k = Viewport.ActualWidth / Viewport.ActualHeight;
//            var oldCameraWidth = camera.Width;
//            var oldCameraSize = k > 1 ? oldCameraWidth / k : oldCameraWidth * k;
//
//            var newCameraSize = oldCameraSize;
//            if (visual.Geometry.Return(i => i.Positions).Return(i => i.Count) > 0)
//                newCameraSize = global::SharpDX.BoundingSphere.FromPoints(visual.Geometry.Positions.ToArrayFast()).Radius * 2;
//
//            var delta = (newCameraSize - oldCameraSize) / newCameraSize;
//            var newCameraWidth = newCameraSize * k * 1.15;
//
//            vUp *= 50 * newCameraSize / Viewport.ActualHeight;
//
//            //Viewport.CameraController.CameraPosition = newPosition + vUp;
//            //Viewport.CameraController.CameraTarget = center + vUp;
//            Viewport.InputController.CameraViewController.RotateCenter = center;
//            camera.AnimateTo(newPosition + vUp, camera.LookDirection, camera.UpDirection, 500);
//            camera.AnimateWidth(Double.IsNaN(width) ? newCameraWidth : width, 500);
//            //camera.Width = newCameraWidth;
//            //Viewport.CameraController.AddZoomForce(delta);
//        }
//
//        public void ZoomToVisual(Zirkonzahn.Visualization.SharpDX.Common.VisualGeometryModel visual, Vector3D direction, CameraPositionTypes? type, double width = Double.NaN) {
//            var camera = (OrthographicCamera)Viewport.CameraController.ActualCamera;
//
//            if (type.HasValue) {
//                SetCamera(type.GetValueOrDefault(), visual.Bounds);
//            } else if (!direction.IsNaN() && direction.LengthSquared > 0.1)
//                Viewport.CameraController.CameraLookDirection = direction;
//
//            var vUp = camera.UpDirection;
//            vUp.Normalize();
//
//            var center = visual.GetVisualCenter();
//            var lookDirection = Viewport.CameraController.CameraLookDirection;
//            lookDirection.Normalize();
//            var newPosition = center - lookDirection * 300.0;
//
//            var k = Viewport.ActualWidth / Viewport.ActualHeight;
//            var oldCameraWidth = camera.Width;
//            var oldCameraSize = k > 1 ? oldCameraWidth / k : oldCameraWidth * k;
//
//            var newCameraSize = oldCameraSize;
//            if (visual.Geometry.Return(i => i.Positions).Return(i => i.Count) > 0)
//                newCameraSize = global::SharpDX.BoundingSphere.FromPoints(visual.Geometry.Positions.ToArrayFast()).Radius * 2;
//
//            var delta = (newCameraSize - oldCameraSize) / newCameraSize;
//            var newCameraWidth = newCameraSize * k * 1.15;
//
//            vUp *= 50 * newCameraSize / Viewport.ActualHeight;
//
//            Viewport.InputController.CameraViewController.RotateCenter = center;
//            newPosition = newPosition + vUp;
//
//            camera.AnimateTo(newPosition, camera.LookDirection, camera.UpDirection, 500);//3000
//            camera.AnimateWidth(Double.IsNaN(width) ? newCameraWidth : width, 500);//3000
//        }
//
//        public void PreviewZoomToVisual(Zirkonzahn.Visualization.SharpDX.Common.VisualGeometryModel visual) {
//            var camera = (OrthographicCamera)Viewport.CameraController.ActualCamera;
//
//            var vUp = camera.UpDirection;
//            vUp.Normalize();
//
//            var center = visual.GetVisualCenter();
//            var lookDirection = Viewport.CameraController.CameraLookDirection;
//            lookDirection.Normalize();
//            var newPosition = center - lookDirection * 300.0;
//
//            var k = Viewport.ActualWidth / Viewport.ActualHeight;
//            var oldCameraWidth = camera.Width;
//            //var oldCameraSize = k > 1 ? oldCameraWidth / k : oldCameraWidth * k;
//
//            //var newCameraSize = oldCameraSize;
//            //if (visual.Geometry.Return(i => i.Positions).Return(i => i.Count) > 0)
//            //    newCameraSize = global::SharpDX.BoundingSphere.FromPoints(visual.Geometry.Positions.ToArrayFast()).Radius * 2;
//
//            //var delta = (newCameraSize - oldCameraSize) / newCameraSize;
//            //  var newCameraWidth = newCameraSize * k * 1.15;
//            var newCameraWidth = oldCameraWidth / 10;
//
//            //vUp *= 50 * newCameraSize / Viewport.ActualHeight;
//
//            ////Viewport.CameraController.CameraPosition = newPosition + vUp;
//            ////Viewport.CameraController.CameraTarget = center + vUp;
//            //Viewport.CameraController.RotationPoint3D = center;
//            camera.AnimateTo(newPosition + vUp, camera.LookDirection, camera.UpDirection, 500);//3000
//            camera.AnimateWidth(newCameraWidth, 500);//3000
//            camera.Width = newCameraWidth;
//            //Viewport.CameraController.AddZoomForce(delta);
//        }
    }
}
