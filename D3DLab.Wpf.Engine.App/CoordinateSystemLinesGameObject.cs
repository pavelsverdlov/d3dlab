using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Utilities;
using System.Collections.Immutable;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public class CoordinateSystemLinesGameObject {
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
            obj.Lines = manager.BuildCoordinateSystemLinesEntity(new ElementTag("Coordinate System"),
                points, color);
            var lenght = 20.0f;
            var radius = 5.0f;
            
            obj.Arrows = new ElementTag[3];
            obj.Arrows[0] = BuildArrow(manager,new ArrowData {
                axis = Vector3.UnitZ,
                orthogonal = Vector3.UnitX,
                center = Vector3.Zero + Vector3.UnitZ * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue,
                tag = new ElementTag("Arrow_Z")
            });
            obj.Arrows[1] = BuildArrow(manager,new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = Vector3.Zero + Vector3.UnitX * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
                tag = new ElementTag("Arrow_X")
            });
            obj.Arrows[2] = BuildArrow(manager,new ArrowData {
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
        static ElementTag BuildArrow(IEntityManager manager, ArrowData data) {
            return manager
               .CreateEntity(data.tag)
               .AddComponent(GeometryBuilder.BuildArrow(data))
               .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;
        }

        public CoordinateSystemLinesGameObject() {

        }
    }

    public class OrbitsRotationGameObject {
        public static OrbitsRotationGameObject Build(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("OrbitsRotationGameObject"))
               .AddComponent(GeometryBuilder.BuildRotationOrbits(100,Vector3.Zero))
               .AddComponent(D3DLineVertexRenderComponent.AsLineStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;


            return new OrbitsRotationGameObject();
        }
    }

    public class SphereGameObject {
        private ElementTag tag;

        public SphereGameObject(ElementTag tag) {
            this.tag = tag;
        }

        public static SphereGameObject Create(IEntityManager manager) {
            var tag = manager
               .CreateEntity(new ElementTag("Sphere"))
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
}
