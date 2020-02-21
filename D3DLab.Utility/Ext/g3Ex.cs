﻿using g3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Utility.Ext {
    static class g3Ex {
        public static Vector3f ToVector3f(this Vector3 v) {
            return new Vector3f(v.X, v.Y, v.Z);
        }
        public static Vector3 ToVector3(this Vector3f v) {
            return new Vector3(v.x, v.y, v.z);
        }
        public static Vector3 ToVector3(this Vector3d v) {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
    }
}
