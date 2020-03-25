using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    public static class V4Colors {
        public static readonly Vector4 White = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Vector4 Green = new Vector4(0, 1, 0, 1);
        public static readonly Vector4 Red = new Vector4(1, 0, 0, 1);
        public static readonly Vector4 Blue = new Vector4(0, 0, 1, 1);
        public static readonly Vector4 Yellow = new Vector4(1, 1, 0, 0);
        public static readonly Vector4 Transparent = Vector4.Zero;


        public static Vector4 NextColor(this Random random) {
            
            return new Vector4(random.NextFloat(0.0f, 1.0f), random.NextFloat(0.0f, 1.0f), random.NextFloat(0.0f, 1.0f), 1.0f);
        }
    }
}
