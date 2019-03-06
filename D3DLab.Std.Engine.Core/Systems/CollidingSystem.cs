using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Input.Commands;
using D3DLab.Std.Engine.Core.Utilities;
using g3;

namespace D3DLab.Std.Engine.Core.Systems {

    public interface ICollidableComponent : IGraphicComponent {
    }

    public class CollidingWithScreenRayComponent : GraphicComponent, ICollidableComponent {
        public Vector2 ScreenPosition { get; set; }
    }

    public class RayCollidedWithEntityComponent : GraphicComponent {
        public ElementTag With { get; set; }
        public Vector3 IntersectionPosition { get; set; }
    }

    public class CollidingSystem : BaseEntitySystem, IGraphicSystem {
        public void Execute(SceneSnapshot snapshot) {
            var colliding = new Colliding(snapshot);
            var emanager = snapshot.ContextState.GetEntityManager();

            foreach (var ev in snapshot.Snapshot.Events) {
                switch (ev) {
                    case CaptureTargetUnderMouseCameraCommand capture:
                        if (colliding.TryToColliding(capture.ScreenPosition, out var collidedWith)) {
                            var entity = emanager.GetEntity(collidedWith.EntityTag);
                            var has = entity.GetComponents<ManipulatableComponent>();
                            if (has.Any()) {
                                entity.AddComponent(new CapturedToManipulateComponent() {
                                    CapturePoint = collidedWith.IntersectionPosition,
                                });
                                snapshot.Snapshot.RemoveEvent(ev);
                            }
                        }
                        break;
                }
            }

            foreach (var entity in emanager.GetEntities()) {
                foreach (var com in entity.GetComponents<ICollidableComponent>()) {
                    switch (com) {
                        case CollidingWithScreenRayComponent byRay:
                            try {
                                if (colliding.TryToColliding(byRay.ScreenPosition, out var collidedWith)) {
                                    entity.AddComponent(collidedWith);
                                }
                            } finally {
                                entity.RemoveComponent(byRay);
                            }
                            break;
                    }
                }
            }
        }

        class Colliding {
            readonly SceneSnapshot snapshot;

            public Colliding(SceneSnapshot snapshot) {
                this.snapshot = snapshot;
            }

            public bool TryToColliding(Vector2 pos, out RayCollidedWithEntityComponent collided) {
                var ray = snapshot.Viewport.UnProject(pos, snapshot.Camera, snapshot.Window);
                collided = new RayCollidedWithEntityComponent();

                var minDistance = double.MaxValue;

                IntrRay3Triangle3 local = null;
                var tag = ElementTag.Empty;
                //find object
                var res = snapshot.Octree.GetColliding(ray, tag => {
                    var entity = snapshot.ContextState.GetEntityManager().GetEntity(tag);

                    var renderable = entity.GetComponents<IRenderableComponent>().Any(x => x.CanRender);
                    if (!renderable) {
                        return false;
                    }

                    var geo = entity.GetComponent<HittableGeometryComponent>();
                    if (geo.IsNull() || !geo.IsBuilt) {
                        return false;
                    }

                    int hit_tid = geo.Tree.FindNearestHitTriangle(ray.g3Rayf);
                    if (hit_tid == DMesh3.InvalidID) {
                        return false;
                    }
                    var intr = MeshQueries.TriangleIntersection(geo.DMesh, hit_tid, ray.g3Rayf);
                    double hit_dist = ray.g3Rayd.Origin.Distance(ray.g3Rayd.PointAt(intr.RayParameter));

                    if (minDistance > hit_dist) {
                        minDistance = hit_dist;
                        local = intr;
                        tag = entity.Tag;
                        return true;
                    }
                    return false;
                });
                if (!res.Any() || local.IsNull()) {
                    return false;
                }

                collided.IntersectionPosition = local.Triangle.V1.ToVector3();
                collided.With = tag;

                return true;
            }
        }


    }
}
