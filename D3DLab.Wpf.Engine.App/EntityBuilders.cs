using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public static class EntityBuilders {
        public static ElementTag BuildMeshElement(this IEntityManager manager, List<Vector3> pos, List<int> indexes, Vector4 color) {
            return Build(manager, pos, indexes, pos.Select(x => color).ToList());
        }

        public static ElementTag BuildGroupMeshElement(this IEntityManager manager, IEnumerable<AbstractGeometry3D> objs) {
            var group = new Std.Engine.Core.Components.GroupGeometryComponent();
            objs.ForEach(x => group.Add(new Std.Engine.Core.Components.GeometryComponent {
                Positions = x.Positions.ToImmutableArray(),
                Indices = x.Indices.ToImmutableArray(),
                Normals = x.Normals.ToImmutableArray(),
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
                   Positions = points.ToImmutableArray(),
                   Indices = ImmutableArray.Create<int>(),
                   Color = new Vector4(0, 1, 0, 1)
               })
               .AddComponent(new SDX.Engine.Components.D3DLineVertexRenderComponent())
               .Tag;
        }
       


        #region components 

        public static GraphicEntity AddRenderAsTriangleColored(this GraphicEntity en) {
            return en.AddComponent(new SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent());
        }
        public static GraphicEntity AddTransformation(this GraphicEntity en) {
            return en.AddComponent(new SDX.Engine.Components.D3DTransformComponent());
        }

        #endregion

        public static ElementTag Build(IEntityManager manager, List<Vector3> pos, List<int> indexes, List<Vector4> colors) {
            return manager
                .CreateEntity(new ElementTag("Geometry" + Guid.NewGuid()))
                .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                    Positions = pos.ToImmutableArray(),
                    Indices = indexes.ToImmutableArray(),
                    Colors = colors.ToImmutableArray()
                })
                .AddComponent(new SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent())
                .Tag;
        }
    }

}
