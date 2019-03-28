using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Systems {
    public class MovingOnHeightMapSystem : BaseEntitySystem, IGraphicSystem {


        protected override void Executing(SceneSnapshot snapshot) {
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();

            foreach (var entity in emanager.GetEntities()) {
                foreach (var com in entity.GetComponents<PerspectiveCameraComponent>()) {
                    entity.GetComponents<KeywordMovingComponent>().DoFirst(movment => {
                        
                    });
                }
            }
        }
    }
}
