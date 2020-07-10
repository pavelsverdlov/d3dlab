using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Viewer.Presentation.Componets {
    interface IViewComponent {
        void ApplyMatrix(in Matrix4x4 matrix);
    }
}
