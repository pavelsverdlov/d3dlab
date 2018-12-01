using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;

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

            public void Execute(MoveCameraToPositionComponent component) {
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
        }
    }
}
