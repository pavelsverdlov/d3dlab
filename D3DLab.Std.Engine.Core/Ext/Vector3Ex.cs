using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Ext {
    public static class Vector2Ex {
        public static Vector2 Normalize(this Vector2 v) {
            return Vector2.Normalize(v);
        }
        public static float AngleRad(this Vector2 u, Vector2 v) {
            return (float)(Math.Atan2(v.Y, v.X) - Math.Atan2(u.Y, u.X));
        }
    }
    public static class Vector3Ex {
        public static Vector3 Normalize(this Vector3 v) {
            return Vector3.Normalize(v);
        }
        public static Vector3 Cross(this Vector3 v1, Vector3 v2) {
            return Vector3.Cross(v1, v2);
        }
        public static float AngleRad(this Vector3 u, Vector3 v) {
            return (float)Math.Atan2(Vector3.Cross(u, v).Length(), Vector3.Dot(u, v));
        }
        public static Vector4 ToVector4(this Vector3 v) {
            return new Vector4(v.X, v.Y, v.Z, 1);
        }
    }
    public static class ConvertorsEx {
        public static float ToRad(this float degrees) {
            return (float)(degrees * Math.PI / 180f);
        }
        public static float ToDeg(this float radians) {
            return (float)(radians * 180f / Math.PI);
        }
    }
}
