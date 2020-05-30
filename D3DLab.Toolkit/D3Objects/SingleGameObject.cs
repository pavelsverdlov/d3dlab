using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using System;
using System.Linq;
using System.Numerics;

namespace D3DLab.Toolkit.D3Objects {
    public class SingleGameObject : GeometryGameObject {
        public ElementTag Tag { get; }

        public SingleGameObject(ElementTag tag, string desc) : base(desc) {
            Tag = tag;
        }

        public override void Hide(IEntityManager manager) {
            var en = manager.GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Disable());
        }

        public override void Show(IEntityManager manager) {
            var en = manager.GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Enable());
        }

        public void LookAtSelf(IEntityManager manager) {
            //var entity = manager.GetEntity(Tag);
            //var geos = entity.GetComponents<IGeometryComponent>();
            //var hasTransformation = entity.GetComponents<TransformComponent>();
            //if (geos.Any()) {
            //    var geo = geos.First();
            //    var local = geo.Box.GetCenter();

            //    if (hasTransformation.Any()) {
            //        local = Vector3.TransformNormal(local, hasTransformation.First().MatrixWorld);
            //    }

            //    var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition = local };
            //    entity.AddComponent(com);
            //}
            throw new NotImplementedException();
        }

        public override void Cleanup(IEntityManager manager) {
            base.Cleanup(manager);
            manager.RemoveEntity(Tag);
        }
    }
}
