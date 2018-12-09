using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Common {
    public class Frustum {
        readonly Plane[] planes = new Plane[6];

        public Frustum(float screenDepth, Matrix4x4 projection, Matrix4x4 view) {
            // Calculate the minimum Z distance in the frustum.
            float zMinimum = -projection.M43 / projection.M33;
            float r = screenDepth / (screenDepth - zMinimum);
            projection.M33 = r;
            projection.M43 = -r * zMinimum;

            // Create the frustum matrix from the view matrix and updated projection matrix.
            Matrix4x4 matrix = view * projection;

            // Calculate near plane of frustum.
            planes[0] = new Plane(matrix.M14 + matrix.M13, matrix.M24 + matrix.M23, matrix.M34 + matrix.M33, matrix.M44 + matrix.M43);
            planes[0].Normalize();

            // Calculate far plane of frustum.
            planes[1] = new Plane(matrix.M14 - matrix.M13, matrix.M24 - matrix.M23, matrix.M34 - matrix.M33, matrix.M44 - matrix.M43);
            planes[1].Normalize();

            // Calculate left plane of frustum.
            planes[2] = new Plane(matrix.M14 + matrix.M11, matrix.M24 + matrix.M21, matrix.M34 + matrix.M31, matrix.M44 + matrix.M41);
            planes[2].Normalize();

            // Calculate right plane of frustum.
            planes[3] = new Plane(matrix.M14 - matrix.M11, matrix.M24 - matrix.M21, matrix.M34 - matrix.M31, matrix.M44 - matrix.M41);
            planes[3].Normalize();

            // Calculate top plane of frustum.
            planes[4] = new Plane(matrix.M14 - matrix.M12, matrix.M24 - matrix.M22, matrix.M34 - matrix.M32, matrix.M44 - matrix.M42);
            planes[4].Normalize();

            // Calculate bottom plane of frustum.
            planes[5] = new Plane(matrix.M14 + matrix.M12, matrix.M24 + matrix.M22, matrix.M34 + matrix.M32, matrix.M44 + matrix.M42);
            planes[5].Normalize();
        }

        bool CheckRectangle(float xCenter, float yCenter, float zCenter, float xSize, float ySize, float zSize) {
            // Check if any of the 6 planes of the rectangle are inside the view frustum.
            for (var i = 0; i < 6; i++) {
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - xSize, yCenter - ySize, zCenter - zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + xSize, yCenter - ySize, zCenter - zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - xSize, yCenter + ySize, zCenter - zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + xSize, yCenter + ySize, zCenter - zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - xSize, yCenter - ySize, zCenter + zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + xSize, yCenter - ySize, zCenter + zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - xSize, yCenter + ySize, zCenter + zSize)) >= 0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + xSize, yCenter + ySize, zCenter + zSize)) >= 0f)
                    continue;

                return false;
            }

            return true;
        }
        bool CheckSphere(Vector3 center, float radius) {
            // Check if the radius of the sphere is inside the view frustum.
            for (int i = 0; i < 6; i++) {
                if (Plane.DotCoordinate(planes[i], center) < -radius)
                    return false;
            }
            return true;
        }
        bool CheckCube(float xCenter, float yCenter, float zCenter, float radius) {
            // Check if any one point of the cube is in the view frustum.
            for (var i = 0; i < 6; i++) {
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - radius, yCenter - radius, zCenter - radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + radius, yCenter - radius, zCenter - radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - radius, yCenter + radius, zCenter - radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + radius, yCenter + radius, zCenter - radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - radius, yCenter - radius, zCenter + radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + radius, yCenter - radius, zCenter + radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter - radius, yCenter + radius, zCenter + radius)) >= 0.0f)
                    continue;
                if (Plane.DotCoordinate(planes[i], new Vector3(xCenter + radius, yCenter + radius, zCenter + radius)) >= 0.0f)
                    continue;

                return false;
            }
            return true;
        }
        bool CheckPoint(Vector3 point) {
            // Check if the point is inside all six planes of the view frustum.
            for (var i = 0; i < 6; i++)
                if (Plane.DotCoordinate(planes[i], point) < 0f)
                    return false;

            return true;
        }
    }
}
