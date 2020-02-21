using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class SingleGameObject : Std.Engine.Core.GeometryGameObject {
        public ElementTag Tag { get; }

        public SingleGameObject(ElementTag tag, string desc) : base(desc) {
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

        public override void LookAtSelf(IEntityManager manager) {
            var entity = manager.GetEntity(Tag);
            var geos = entity.GetComponents<IGeometryComponent>();
            var hasTransformation = entity.GetComponents<TransformComponent>();
            if (geos.Any()) {
                var geo = geos.First();
                var local = geo.Box.GetCenter();

                if (hasTransformation.Any()) {
                    local = Vector3.TransformNormal(local, hasTransformation.First().MatrixWorld);
                }

                var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition = local };
                entity.AddComponent(com);
            }
        }

        public override void Cleanup(IEntityManager manager) {
            base.Cleanup(manager);
            manager.RemoveEntity(Tag);
        }
    }
}
