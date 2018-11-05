using D3DLab.SDX.Engine.Builders;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public struct ArrowData {
        public ElementTag tag;
        public Vector3 axis;
        public Vector3 orthogonal;
        public Vector3 center;
        public Vector4 color;
        public float lenght;
        public float radius;
    }

    public static class EntityBuilders {
        public static ElementTag BuildMeshElement(this IEntityManager manager, List<Vector3> pos, List<int> indexes, Vector4 color) {
            return MeshEntityBuilder.Build(manager, pos, indexes, pos.Select(x => color).ToList());
        }

        public static ElementTag BuildGroupMeshElement(this IEntityManager manager, IEnumerable<AbstractGeometry3D> objs) {
            var group = new Std.Engine.Core.Components.GroupGeometryComponent();
            objs.ForEach(x => group.Add(new Std.Engine.Core.Components.GeometryComponent {
                Positions = x.Positions,
                Indices = x.Indices,
                Normals = x.Normals,
                Color = x.Color
            }));
            group.Combine();
            return manager
                .CreateEntity(new ElementTag("GroupGeometry" + Guid.NewGuid()))
                .AddComponent(group)
                .AddComponent(new SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent())
                .AddComponent(new SDX.Engine.Components.D3DTransformComponent() {
                    MatrixWorld = Matrix4x4.CreateTranslation(Vector3.UnitY * 30)
                })
                .Tag;
        }

        public static ElementTag BuildLineEntity(this IEntityManager manager, Vector3[] points) {
            return manager
               .CreateEntity(new ElementTag("Points" + Guid.NewGuid()))
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                   Positions = points.ToList(),
                   Indices = new List<int>(),
                   Color = new Vector4(0, 1, 0, 1)
               })
               .AddComponent(new SDX.Engine.Components.D3DLineVertexRenderComponent())
               .Tag;
        }
        public static ElementTag BuildCoordinateSystemLinesEntity(this IEntityManager manager,
            ElementTag tag, Vector3[] points, Vector4[] color) {

            return manager
               .CreateEntity(tag)
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                   Positions = points.ToList(),
                   Indices = new List<int>(),
                   Colors = new List<Vector4>(color),
               })
               .AddComponent(SDX.Engine.Components.D3DLineVertexRenderComponent.AsLineList())
               .Tag;
        }


        public static ElementTag BuildArrow(this IEntityManager manager, ArrowData data) {

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

            return manager
               .CreateEntity(data.tag)
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent {
                   Positions = points,
                   Indices = index,
                   Normals = normals,
                   Color = data.color
               })
               .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;
        }
    }
}
