using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Utilities {
    public struct ArrowData {
        public ElementTag tag;
        public Vector3 axis;
        public Vector3 orthogonal;
        public Vector3 center;
        public Vector4 color;
        public float lenght;
        public float radius;
    }

    

    public class GeometryBuilder {

        public static GeometryComponent BuildRotationOrbits(float radius, Vector3 center) {
            var step = 10f;

            var axises = new[] { 
                new { asix = Vector3.UnitX, color =V4Colors.Green}, 
                new { asix = Vector3.UnitY, color = V4Colors.Red },
                new { asix = Vector3.UnitZ, color = V4Colors.Blue }
            };
            var lines = new List<Vector3>();
            var color = new List<Vector4>();
            foreach (var a in axises) {
                var axis = a.asix;
                var start = center + axis.FindAnyPerpendicular() * radius; 
                var rotate = Matrix4x4.CreateFromAxisAngle(axis, step.ToRad());
                lines.Add(start);
                color.Add(a.color);
                for (var angle = step; angle < 360; angle += step) {
                    start = Vector3.Transform(start, rotate);
                    lines.Add(start);
                    color.Add(a.color);
                }
            }
            return new GeometryComponent {
                Colors = color.ToImmutableArray(),
                Positions = lines.ToImmutableArray(),
                Indices = ImmutableArray.Create<int>(),
            };
        }

        public static GeometryComponent BuildArrow(ArrowData data) {
            var axis = data.axis;
            var center = data.center;
            var lenght = data.lenght;
            var radius = data.radius;

            var rotate = Matrix4x4.CreateFromAxisAngle(axis, 10f.ToRad()); // Matrix4x4.CreateFromQuaternion(new Quaternion(axis,10));

            var points = new List<Vector3>();
            var normals = new List<Vector3>();
            var index = new List<int>();

            points.Add(center + axis * lenght);
            normals.Add(axis);

            var orto = data.orthogonal;
            var corner = center + orto * radius;
            normals.Add((corner - center).Normalize());

            points.Add(corner);
            for (var i = 10; i < 360; i += 10) {
                corner = Vector3.Transform(corner, rotate);
                points.Add(corner);
                normals.Add((corner - center).Normalize());
            }
            points.Add(center);
            normals.Add(-axis);

            for (var i = 0; i < points.Count - 2; ++i) {
                index.AddRange(new[] { 0, i, i + 1 });
                index.AddRange(new[] { points.Count - 1, i, i + 1 });
            }
            index.AddRange(new[] { 0, points.Count - 2, 1 });
            index.AddRange(new[] { points.Count - 1, points.Count - 2, 1 });

            return new GeometryComponent {
                Positions = points.ToImmutableArray(),
                Indices = index.ToImmutableArray(),
                Normals = normals.ToImmutableArray(),
                Color = data.color
            };
        }
    }
}
