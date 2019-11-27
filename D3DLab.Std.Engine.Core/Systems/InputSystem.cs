using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Systems {
    public sealed class InputSystem : BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { get; set; }

        protected override void Executing(ISceneSnapshot ss) {
            var snapshot = (SceneSnapshot)ss;
            var s = snapshot.Snapshot;

            foreach (var en in ContextState.GetEntityManager().GetEntities()) {
                foreach (var cmd in s.Events) {
                    if (cmd.Execute(en)) {
                        s.RemoveEvent(cmd);
                    }
                }
            }
        }
    }
}
