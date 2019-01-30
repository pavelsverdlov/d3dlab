using System.Collections.Generic;
using System.Linq;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using g3;

namespace D3DLab.Std.Engine.Core.Systems {
    public class MovementSystem : BaseComponentSystem, IComponentSystem {

        public void Execute(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            foreach (var entity in emanager.GetEntities()) {
                foreach (var com in entity.GetComponents<MovementComponent>()) {
                    com.Execute(new Handlers(entity, snapshot));
                }
            }
        }

        class Handlers : IMovementComponentHandler {
            readonly IEntityManager emanager;
            readonly GraphicEntity entity;
            readonly SceneSnapshot snapshot;

            public Handlers(GraphicEntity entity, SceneSnapshot snapshot) {
                this.emanager = snapshot.ContextState.GetEntityManager();
                this.entity = entity;
                this.snapshot = snapshot;
            }

            public void Execute(MoveCameraToTargetComponent component) {
                //var geo = emanager.CreateEntity(component.Target).GetComponent<IGeometryComponent>();
                //var center = geo.Box.GetCenter();

                var movecom = new CameraMoveToPositionComponent { TargetPosition = component.TargetPosition };

                emanager
                    .GetEntity(snapshot.CurrentCameraTag)
                    .AddComponent(movecom);

                entity.RemoveComponent(component);
            }

            public void Execute(IMoveToPositionComponent component) {
                
            }

            public void Execute(HitToTargetComponent component) {
                try {
                    var ray = snapshot.Viewport.UnProject(component.ScreenPosition, snapshot.Camera, snapshot.Window);
                    //var ray = snapshot.Camera.GetRay(snapshot.Window, component.ScreenPosition);

                    System.Diagnostics.Trace.WriteLine($"{ray.Origin} {ray.Direction}");

                    //return;
                    IntrRay3Triangle3 hitted = null;
                    var minDistance = double.MaxValue;

                    //find object
                    var manager = snapshot.ContextState.GetEntityManager();
                    var res = snapshot.Octree.GetColliding(ray, tag => {
                        var entity = manager.GetEntity(tag);
                        var geo = entity.GetComponent<HittableGeometryComponent>();
                        if (!geo.IsBuilt) {
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
                        return;
                    }
                    //push command to camera for new focus position
                    var center = hitted.Triangle.V1;
                    emanager
                        .GetEntity(snapshot.CurrentCameraTag)
                        .AddComponent(new CameraMoveToPositionComponent { TargetPosition = center.ToVector3() });

                    //
                } finally {
                    entity.RemoveComponent(component);
                }
            }
        }
    }
}
