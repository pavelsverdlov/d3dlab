using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Render {
    public class ViewportState {
        /// <summary>
        /// Orthographic /Perspective 
        /// </summary>
        public Matrix4x4 ProjectionMatrix;
        /// <summary>
        /// the same as Camera
        /// </summary>
        public Matrix4x4 ViewMatrix;
    }
}
