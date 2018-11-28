using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Numerics;
using Veldrid.Utilities;

namespace D3DLab.Std.Engine.Core.Utilities {
    public static class ViewportEx {
        public static Ray UnProject(GeneralCameraComponent camera, float w, float h,  Vector2 point2d) {//IAppWindow win,
            var px = (float)point2d.X;
            var py = (float)point2d.Y;

            var viewMatrix = camera.ViewMatrix;
            Vector3 v = new Vector3();

            var matrix = viewMatrix.PsudoInvert();
            //float w = win.Width;
            //float h = win.Height;

            var projMatrix = camera.ProjectionMatrix;
            Vector3 zn;
            v.X = (2 * px / w - 1) / projMatrix.M11;
            v.Y = -(2 * py / h - 1) / projMatrix.M22;
            v.Z = 1 / projMatrix.M33;
            Vector3 zf = Vector3.Transform(v, matrix);

            if (camera is PerspectiveCameraComponent) {
                v.Z = 0;
                zn = Vector3.Transform(v, matrix);
                //zn = camera.Position;
            } else {
                v.Z = 0;
                zn = Vector3.Transform(v, matrix);
            }
            Vector3 r = zf - zn;
            r.Normalize();

            return new Ray(zn + r * camera.NearPlaneDistance, r);
        }
    }
}
