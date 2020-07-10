using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using System;
using System.Linq;
using System.Numerics;

namespace D3DLab.Toolkit.D3Objects {
    public class SingleVisualObject : GeometryGameObject {
        public ElementTag Tag { get; }
        /// <summary>
        /// this status is only be actual if Show/Hide methods were used
        /// </summary>
        public bool IsVisible { get; set; }

        public SingleVisualObject(ElementTag tag, string desc) : base(desc) {
            Tag = tag;
            IsVisible = true;
        }

        public override void Hide(IEntityManager manager) {
            var en = manager.GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Disable());
            IsVisible = false;
        }

        public override void Show(IEntityManager manager) {
            var en = manager.GetEntity(Tag);
            en.UpdateComponent(en.GetComponent<RenderableComponent>().Enable());
            IsVisible = true;
        }

        public virtual GraphicEntity GetEntity(IEntityManager manager) => manager.GetEntity(Tag);

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

        public override void Cleanup(IContextState context) {
            base.Cleanup(context);
            context.GetEntityManager().RemoveEntity(Tag);
        }
    }
}
