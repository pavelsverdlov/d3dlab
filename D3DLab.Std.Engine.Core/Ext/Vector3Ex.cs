﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Vector3 ToVector3(this Vector2 v, float z = 0) {
            return new Vector3(v, z);
        }
    }
    public static class Vector3Ex {
        public static Vector3 Normalize(this Vector3 v) {
            return Vector3.Normalize(v);
        }
        public static void Normalized(this Vector3 v) {
            v = Vector3.Normalize(v);
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
        public static Vector3 XYZ(this Vector4 v) {
            return new Vector3(v.X, v.Y, v.Z);
        }
      


        public static Matrix4x4 RotationAround(this Vector3 axis, float angleRadians) {
            return Matrix4x4.CreateFromAxisAngle(axis, angleRadians);
        }
        public static Matrix4x4 RotationAround(this Vector3 axis, float angle, Vector3 center) {
            var m1 = Matrix4x4.CreateTranslation(center * -1f);
            var m2 = axis.RotationAround(angle);
            var m3 = Matrix4x4.CreateTranslation(center);
            var m = m1 * m2 * m3;
            return m;
        }
        public static Vector3 FindAnyPerpendicular(this Vector3 n) {
            n.Normalize();
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3) {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }

            return u;
        }
    }

    public static class Vector3CollectionEx {
        unsafe public static Vector3[] Transform(this Vector3[] positions, ref Matrix4x4 matrix) {
            if (positions == null || positions.Length == 0) {
                return null;
            }

            var result = new Vector3[positions.Length];
            fixed (Vector3* _pSrc = positions) {
                fixed (Vector3* _pDst = result) {
                    Vector3* pSrc = _pSrc, pDst = _pDst;
                    var end = pSrc + positions.Length;
                    for (; pSrc < end; ++pSrc, ++pDst) {
                        *pDst = Vector3.Transform(*pSrc, matrix);
                    }
                }
            }
            return result;
        }

        unsafe public static List<Vector3> CalculateNormals(this List<Vector3> positions, List<int> indices) {

            var aNormals = new Vector3[positions.Count];
            var aPos = positions.ToArray();
            var aInd = indices.ToArray();
            fixed (Vector3* pNormal = aNormals) {
                fixed (Vector3* pPos = aPos) {
                    fixed (int* pInd = aInd) {
                        for (var i = 0; i < indices.Count; i += 3) {
                            var index0 = *(pInd + i);
                            var index1 = *(pInd + i + 1);
                            var index2 = *(pInd + i + 2);
                            Vector3 u = Vector3.Subtract(*(pPos + index1), *(pPos + index0));
                            Vector3 v = Vector3.Subtract(*(pPos + index2), *(pPos + index0));
                            Vector3 w = Vector3.Cross(u, v);
                            w.Normalize();

                            *(pNormal + index0) = Vector3.Add(*(pNormal + index0), w);
                            *(pNormal + index1) = Vector3.Add(*(pNormal + index1), w);
                            *(pNormal + index2) = Vector3.Add(*(pNormal + index2), w);
                        }
                    }
                }
                for (int i = 0; i < aNormals.Length; i++) {
                    *(pNormal + i) = (*(pNormal + i)).Normalize();
                }
            }

            return aNormals.ToList();
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