using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using g3;

namespace D3DLab.Std.Engine.Core.Systems {
    public class ManipulatableComponent : GraphicComponent {

    }
    public class CapturedToManipulateComponent : GraphicComponent {
        public Vector3 CapturePoint { get; set; }
    }
    public class MovementSystem : BaseEntitySystem, IGraphicSystem {

        public void Execute(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            foreach (var entity in emanager.GetEntities()) {
                foreach (var com in entity.GetComponents()) {
                    switch (com) {
                        case MovementComponent move:
                            move.Execute(new Handlers(entity, snapshot));
                            break;

                        case CapturedToManipulateComponent manipulate:
                            var movable = entity.GetComponent<ManipulatableComponent>();
                            // move
                            //movable.Move();
                            break;
                    }
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
                //detect that camera should be moved to new position

                //created camera movement component
                var movecom = new CameraMoveToPositionComponent { TargetPosition = component.TargetPosition };

                //just put com to camera entity
                //camera moving is job for Camera system
                emanager
                    .GetEntity(snapshot.CurrentCameraTag)
                    .AddComponent(movecom);

                entity.RemoveComponent(component);
            }

            public void Execute(FollowUpTargetComponent follower) {
                var target = emanager.GetEntity(follower.IsTarget).First();
                follower.Follow(entity, target);
            }

            public void Execute(TranslateMovementComponent translate) {

            }

        }
    }
}
