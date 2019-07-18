using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Utilities;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Physics.Engine;

namespace D3DLab.Wpf.Engine.App.GameObjects {
    public class PhysicsObjectTest : SingleGameObject {
        public PhysicsObjectTest(ElementTag tag) : base(tag, "PhysicsObjectTest") {
        }

        public static SingleGameObject Create(IEntityManager manager, BoundingBox box, Vector4 color) {
            var tag = new ElementTag("Physics Object" + Guid.NewGuid());

            //var box = new BoundingBox(new Vector3(-5,10,-5), new Vector3(5,20,5));

            var geobox = GeometryBuilder.BuildGeoBox(box);

            manager.CreateEntity(tag)
              .AddComponents(new IGraphicComponent[] {
                    new SimpleGeometryComponent{
                        Positions = geobox.Positions.ToImmutableArray(),
                        Indices = geobox.Indices.ToImmutableArray(),
                        Normals = geobox.Positions.CalculateNormals(geobox.Indices).ToImmutableArray(),
                        Color = color
                    },
                    new D3DTriangleColoredVertexRenderComponent(),
                    new TransformComponent(){ MatrixWorld = Matrix4x4.Identity },
                    PhysicalComponentFactory.Create(box),
              });

            return new PhysicsObjectTest(tag);
        }

        public static SingleGameObject CreateStatic(IEntityManager manager, BoundingBox box) {
            var tag = new ElementTag("Physics Static " + Guid.NewGuid());

            var geobox = GeometryBuilder.BuildGeoBox(box);

            manager.CreateEntity(tag)
              .AddComponents(new IGraphicComponent[] {
                    new SimpleGeometryComponent{
                        Positions = geobox.Positions.ToImmutableArray(),
                        Indices = geobox.Indices.ToImmutableArray(),
                        Normals = geobox.Positions.CalculateNormals(geobox.Indices).ToImmutableArray(),
                        Color = new Vector4(1, 0, 0, 1)
                    },
                    new D3DTriangleColoredVertexRenderComponent(),
                    new TransformComponent(){ MatrixWorld = Matrix4x4.Identity },
                    PhysicalComponentFactory.CreateStatic(box),
              });

            return new PhysicsObjectTest(tag);
        }
    }
}
