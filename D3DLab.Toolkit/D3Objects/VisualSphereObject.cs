using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Techniques.SpherePoint;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class VisualSphereObject : SingleVisualObject {
        public struct Data {
            public Vector3 Center;
            public Vector4 Color;
            public float Radius;
        }

        public VisualSphereObject(ElementTag tag) : base(tag, "SphereByPoint") {
        }

        //public static VisualSphereObject Create(IEntityManager manager) {
        //    var tag = manager
        //       .CreateEntity(new ElementTag("Sphere_" + DateTime.Now.Ticks))
        //       .AddComponent(new SimpleGeometryComponent() {
        //           Indices = new[] { 0 }.ToImmutableArray(),
        //           Positions = new Vector3[] { Vector3.Zero }.ToImmutableArray(),
        //           Color = V4Colors.Red,
        //       })
        //       .AddComponent(new D3DSpherePointRenderComponent())
        //       .AddComponent(new TransformComponent())
        //       .Tag;


        //    return new VisualSphereObject(tag);
        //}
        public static VisualSphereObject Create(ElementTag elet, IEntityManager manager, Data data) {
            var tag = manager
               .CreateEntity(elet)
               .AddComponent(SpherePointComponent.Create(data.Center, data.Radius))
               .AddComponent(MaterialColorComponent.Create(data.Color))
               .AddComponent(RenderableComponent.AsPoints())
               .AddComponent(TransformComponent.Identity())
               .Tag;

            return new VisualSphereObject(tag);
        }
    }

}
