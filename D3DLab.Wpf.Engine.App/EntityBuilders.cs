using D3DLab.SDX.Engine.Builders;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
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
                .AddComponent(new SDX.Engine.Components.D3DColoredVertexesRenderComponent())
                .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
                .Tag;
        }

        public static ElementTag BuildLineEntity(this IEntityManager manager, Vector3[] points) {
            return manager
               .CreateEntity(new ElementTag("Points" + Guid.NewGuid()))
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                   Positions = points.ToList(),
                   Color = new Vector4(0, 1, 0, 1)
               })
               .AddComponent(new SDX.Engine.Components.D3DLineVertexRenderComponent())
               .Tag;
        }

    }
}
