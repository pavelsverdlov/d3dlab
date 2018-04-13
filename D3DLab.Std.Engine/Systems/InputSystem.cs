using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.Std.Engine.Systems
{
    public sealed class InputSystem : IComponentSystem {
        public void Execute(SceneSnapshot snapshot) {
            var s = snapshot.Snapshot;

            foreach(var en in snapshot.State.GetEntityManager().GetEntities()) {
                foreach (var cmd in s.Events.ToList()) {
                    if (cmd.Execute(en)) {
                        s.RemoveEvent(cmd);
                    }
                }
            }
        }
    }
}
