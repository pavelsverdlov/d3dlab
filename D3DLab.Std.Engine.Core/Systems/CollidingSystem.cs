using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using g3;

namespace D3DLab.Std.Engine.Core.Systems {

    public interface ICollidableComponent :IGraphicComponent{
        void Execute(ICollidingSystemHandlers handlers);
    }

    public interface ICollidingFromScreenComponent : ICollidableComponent {
        Vector2 ScreenPosition { get; }
    }

    public interface ICollidingSystemHandlers {
        void Handle(ICollidingFromScreenComponent component);
    }

    public class CollidingSystem : BaseEntitySystem, IGraphicSystem {
        public void Execute(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            foreach (var entity in emanager.GetEntities()) {
                foreach (var com in entity.GetComponents<ICollidableComponent>()) {
                    com.Execute(new Handler(snapshot, entity, com));
                }
            }
        }

        class Handler : ICollidingSystemHandlers {
            readonly SceneSnapshot snapshot;
            readonly GraphicEntity entity;
            readonly ICollidableComponent component;

            public Handler(SceneSnapshot snapshot, GraphicEntity entity, ICollidableComponent component) {
                this.snapshot = snapshot;
                this.entity = entity;
                this.component = component;
            }

            public void Handle(ICollidingFromScreenComponent component) {
                var ray = snapshot.Viewport.UnProject(component.ScreenPosition, snapshot.Camera, snapshot.Window);
                if (TryToColliding(ray)) {
                    
                }
            }

            bool TryToColliding(Ray ray) {
                var geo = entity.GetComponent<HittableGeometryComponent>();
                if (geo.IsNull() || !geo.IsBuilt) {
                    return false;
                }

                IntrRay3Triangle3 hitted = null;
                var minDistance = double.MaxValue;

                //find object
                var res = snapshot.Octree.GetColliding(ray, tag => {
                    var entity = snapshot.ContextState.GetEntityManager().GetEntity(tag);

                    var renderable = entity.GetComponents<IRenderableComponent>().Any(x => x.CanRender);
                    if (!renderable) {
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
                        hitted = intr;
                        return true;
                    }
                    return false;
                });
                if (!res.Any() || hitted.IsNull()) {
                    return false;
                }


                return false;
            }
        }

       
    }
}
