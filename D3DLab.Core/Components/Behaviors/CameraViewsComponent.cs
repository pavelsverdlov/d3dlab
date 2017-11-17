using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Components.Behaviors {
    public sealed class CameraViewsComponent : Components.Component, IAttachTo<OrthographicCameraEntity>  {
        public enum CameraViews {
            FrontView = 0,
            LeftView,
            BackView,
            RightView,
            TopView,
            BottomView
        }
        private static readonly Dictionary<CameraViews,Tuple<Vector3, Vector3>> viewInfos;

        static CameraViewsComponent() {
            var cameraPosition = new Vector3(0, 0, 300);
            var cameraUpDirection = new Vector3(0, 1, 0);
            var cameraPositionVector = cameraPosition;
            
            cameraUpDirection.Normalize();
            cameraPositionVector.Normalize();
            var cross = Vector3.Cross(cameraPositionVector, cameraUpDirection);
            cross.Normalize();
            viewInfos = new Dictionary<CameraViews, Tuple<Vector3, Vector3>> {
                {CameraViews.BackView, Tuple.Create(new Vector3(-cameraUpDirection.X, -cameraUpDirection.Y, -cameraUpDirection.Z), cameraPositionVector)},
                {CameraViews.FrontView, Tuple.Create(new Vector3(cameraUpDirection.X, cameraUpDirection.Y, cameraUpDirection.Z), cameraPositionVector)},
                {CameraViews.LeftView, Tuple.Create(new Vector3(cross.X, cross.Y, cross.Z), cameraPositionVector)},
                {CameraViews.RightView, Tuple.Create(new Vector3(-cross.X, -cross.Y, -cross.Z), cameraPositionVector)},
                {CameraViews.TopView, Tuple.Create(new Vector3(cameraPositionVector.X, cameraPositionVector.Y, cameraPositionVector.Z), cameraUpDirection)},
                {CameraViews.BottomView, Tuple.Create(new Vector3(-cameraPositionVector.X, -cameraPositionVector.Y, -cameraPositionVector.Z), cameraUpDirection)}
            };
        }

        private OrthographicCameraEntity parent;

        public void SetCamera(CameraViews viewType) {
            Vector3 center = Vector3.Zero;
            float scale = 1;
            var box = new BoundingBox();
            float width = 300;

            if ((box.Maximum - box.Minimum).Length() > 0) {
                float calcwidth = (float)(box.SizeX() * 2.5f);
                width = calcwidth < width ? width : calcwidth;
            }

            var info = viewInfos[viewType];//Vector3D.CrossProduct()
            var data = new CameraData();
            data.Position = Vector3.Multiply(info.Item1, width*2) + center;
            data.LookDirection = Vector3.Multiply(info.Item1, -width).Normalized();
            data.UpDirection = info.Item2;
            data.NearPlaneDistance = 0.001f;
            data.Width = width / (scale == 0.0f || scale == float.NaN ? 1.0f : scale);
        }

        public void OnAttach(OrthographicCameraEntity parent) {
            this.parent = parent;
        }
    }
}
