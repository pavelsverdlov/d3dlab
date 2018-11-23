using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public class SingleGameObject : GameObject {
        public ElementTag Tag { get; }

        public SingleGameObject(ElementTag tag) {
            Tag = tag;
        }

        public override void Hide(IEntityManager manager) {
            manager.GetEntity(Tag)
                  .GetComponent<IRenderableComponent>()
                  .CanRender = false;
        }

        public override void Show(IEntityManager manager) {
            manager.GetEntity(Tag)
                  .GetComponent<IRenderableComponent>()
                  .CanRender = true;
        }
    }
    public class CoordinateSystemLinesGameObject : GameObject {
        public ElementTag Lines { get; private set; }
        public ElementTag[] Arrows { get; private set; }

        public static CoordinateSystemLinesGameObject Build(IEntityManager manager) {
            var llength = 100;
            var obj = new CoordinateSystemLinesGameObject();
            var points = new[] {
                Vector3.Zero - Vector3.UnitX * llength, Vector3.Zero + Vector3.UnitX * llength,
                Vector3.Zero- Vector3.UnitY  * llength, Vector3.Zero + Vector3.UnitY * llength,
                Vector3.Zero- Vector3.UnitZ  * llength, Vector3.Zero + Vector3.UnitZ * llength,
            };
            var color = new[] {
                V4Colors.Green, V4Colors.Green,
                V4Colors.Red, V4Colors.Red,
                V4Colors.Blue, V4Colors.Blue,
            };
            obj.Lines = PolylineGameObject.Create(manager, new ElementTag("Coordinate System"),
                points, color).Tag;
            var lenght = 20.0f;
            var radius = 5.0f;

            obj.Arrows = new ElementTag[3];
            obj.Arrows[0] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitZ,
                orthogonal = Vector3.UnitX,
                center = Vector3.Zero + Vector3.UnitZ * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue,
                tag = new ElementTag("Arrow_Z")
            });
            obj.Arrows[1] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = Vector3.Zero + Vector3.UnitX * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
                tag = new ElementTag("Arrow_X")
            });
            obj.Arrows[2] = ArrowGameObject.BuildArrow(manager, new ArrowData {
                axis = Vector3.UnitY,
                orthogonal = Vector3.UnitZ,
                center = Vector3.Zero + Vector3.UnitY * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Red,
                tag = new ElementTag("Arrow_Y")
            });

            return obj;
        }

        public override void Show(IEntityManager manager) {
            foreach (var tag in Arrows) {
                manager.GetEntity(tag)
                    .GetComponent<IRenderableComponent>()
                    .CanRender = true;
            }
            manager.GetEntity(Lines)
                .GetComponent<IRenderableComponent>()
                .CanRender = true;
        }

        public override void Hide(IEntityManager manager) {
            foreach (var tag in Arrows) {
                manager.GetEntity(tag)
                    .GetComponent<IRenderableComponent>()
                    .CanRender = false;
            }
            manager.GetEntity(Lines)
                .GetComponent<IRenderableComponent>()
                .CanRender = false;
        }

        public CoordinateSystemLinesGameObject() {

        }
    }

    public class ArrowGameObject {
        public ElementTag Tag { get; }

        public ArrowGameObject(ElementTag tag) {
            Tag = tag;
        }

        public static ArrowGameObject Build(IEntityManager manager, ArrowData data) {
            var en = manager
              .CreateEntity(data.tag)
              .AddComponent(GeometryBuilder.BuildArrow(data))
              .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
              .AddComponent(new SDX.Engine.Components.D3DTransformComponent());

            return new ArrowGameObject(en.Tag);
        }

        [Obsolete("Remove")]
        public static ElementTag BuildArrow(IEntityManager manager, ArrowData data) {
            return manager
               .CreateEntity(data.tag)
               .AddComponent(GeometryBuilder.BuildArrow(data))
               .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;
        }
    }

    public class OrbitsRotationGameObject {
        public static OrbitsRotationGameObject Create(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("OrbitsRotationGameObject"))
               .AddComponent(GeometryBuilder.BuildRotationOrbits(100, Vector3.Zero))
               .AddComponent(D3DLineVertexRenderComponent.AsLineStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;

            return new OrbitsRotationGameObject();
        }
    }

    public class SphereGameObject {
        public struct Data {
            public Vector3 Center;
            public float Radius;
        }

        private readonly ElementTag tag;

        public SphereGameObject(ElementTag tag) {
            this.tag = tag;
        }

        public static SphereGameObject Create(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("Sphere_" + DateTime.Now.Ticks))
               .AddComponent(new GeometryComponent() {
                   Indices = new[] { 0 }.ToImmutableArray(),
                   Positions = new Vector3[] { Vector3.Zero }.ToImmutableArray(),
                   Color = V4Colors.Red,
               })
               .AddComponent(new D3DSphereRenderComponent())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;


            return new SphereGameObject(tag);
        }
    }

    public class PolylineGameObject {
        public ElementTag Tag { get; }

        public PolylineGameObject(ElementTag tag1) {
            this.Tag = tag1;
        }

        public static PolylineGameObject Create(IEntityManager manager, ElementTag tag,
            IEnumerable<Vector3> points, IEnumerable<Vector4> color) {
            var indeces = new List<int>();
            for (var i = 0; i < points.Count(); i++) {
                indeces.AddRange(new[] { i, i });
            }

            var tag1 = manager
               .CreateEntity(tag)
               .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                   Positions = points.ToImmutableArray(),
                   Indices = indeces.ToImmutableArray(),
                   Colors = color.ToImmutableArray(),
               })
               .AddComponent(SDX.Engine.Components.D3DLineVertexRenderComponent.AsLineList())
               .Tag;

            return new PolylineGameObject(tag1);
        }
    }
}
