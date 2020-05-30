using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.Toolkit.D3Objects {
    public class PolylineGameObject : SingleGameObject {
        public PolylineGameObject(ElementTag tag1) : base(tag1,"poly") {
        }

        public static PolylineGameObject Create(IContextState context, ElementTag tag,
            Vector3[] points, Vector4 color) {
            var manager = context.GetEntityManager();

            var indeces = new List<int>();
            var pos = new List<Vector3>();
            var prev = points[0];
            for (var i = 0; i < points.Length; i++) {
                pos.Add(prev);
                pos.Add(points[i]);
                prev = points[i];

            }
            for (var i = 0; i < pos.Count; i++) {
                indeces.AddRange(new[] { i, i });
            }
            var geo = context.GetGeometryPool()
                .AddGeometry(new ImmutableGeometryData(pos.AsReadOnly(), indeces.AsReadOnly()));

            manager
               .CreateEntity(tag)
               .AddComponents(
                    geo,
                    TransformComponent.Identity(),
                    ColorComponent.CreateDiffuse(color),
                    RenderableComponent.AsLineList());


            return new PolylineGameObject(tag);
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
