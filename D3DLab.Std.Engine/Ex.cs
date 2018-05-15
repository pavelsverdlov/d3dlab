using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace D3DLab.Std.Engine {
    public static class Ex {
        public static void CreateIfNullBuffer(this DisposeCollectorResourceFactory f, ref DeviceBuffer b, BufferDescription desc) {
            b = b ?? f.CreateBuffer(desc);
        }
        public static void DoFirst<T>(this IEnumerable<T> enu, Action<T> action) where T : ID3DComponent {
            foreach (var i in enu) {
                action(i);
                break;
            }
        }
        public static Vector2 Normalize(this Vector2 v) {
            return new Vector2(v.X, v.Y) / v.Length();
        }

        public static float Angle(this Vector3 u, Vector3 v) {
            return (float)Math.Atan2(Vector3.Cross(u, v).Length(), Vector3.Dot(u, v));
        }
        public static float AngleRad(this Vector2 u, Vector2 v) {
            return (float)(Math.Atan2(v.Y, v.X) - Math.Atan2(u.Y, u.X));
        }
        public static float ToRad(this float degrees) {
            return (float)(degrees * Math.PI / 180f);
        }
        public static float ToDeg(this float radians) {
            return (float)(radians * 180f / Math.PI);
        }


        public static float SizeX(this BoundingBox bounds) {
            return Math.Abs(bounds.Min.X - bounds.Max.X);
        }
        public static float SizeY(this BoundingBox bounds) {
            return Math.Abs(bounds.Min.Y - bounds.Max.Y);
        }
        public static float SizeZ(this BoundingBox bounds) {
            return Math.Abs(bounds.Min.Z - bounds.Max.Z);
        }
    }
}
