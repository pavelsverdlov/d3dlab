using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Techniques.Line;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class PolylineGameObject : SingleGameObject {
        public PolylineGameObject(ElementTag tag1) : base(tag1,"poly") {
        }

        public static PolylineGameObject Create(IEntityManager manager, ElementTag tag,
            IEnumerable<Vector3> points, Vector4 color) {
            var indeces = new List<int>();
            for (var i = 0; i < points.Count(); i++) {
                indeces.AddRange(new[] { i, i });
            }
            var geo = new LineGeometryComponent(points.ToArray(), indeces.ToArray());
            var tag1 = manager
               .CreateEntity(tag)
               .AddComponent(geo)
               .AddComponent(D3DLineVertexRenderComponent.AsLineList())
               .AddComponent(TransformComponent.Identity())
               .AddComponent(MaterialColorComponent.Create(color))
               .Tag;

            return new PolylineGameObject(tag1);
        }

        public override void Cleanup(IEntityManager manager) {
            manager.RemoveEntity(Tag);
        }

        public override void Hide(IEntityManager manager) {

        }

        public override void Show(IEntityManager manager) {

        }
    }
}
