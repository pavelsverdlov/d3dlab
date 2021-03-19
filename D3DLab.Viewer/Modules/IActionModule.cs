using D3DLab.Viewer.D3D;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Viewer.Modules {
    interface ISelectedObjectTransformation {
        public Vector3 GetCenter();
        void Transform(Matrix4x4 matrix);
        void ShowTransformationAxis(Vector3 axis);
        void HideTransformationAxis(Vector3 axis);
        void HideAllTransformationAxis();
        void Finish();
    }
    interface IActionModule {
    }
}
