using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands {
    class InputIdleCommand : IInputCommand {
        public InputStateData InputState { get; }
        public InputIdleCommand() {
            //InputState = state;
        }
        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);
            entity.RemoveComponent<CameraMovementComponent>();

            return true;
        }
    }
}
