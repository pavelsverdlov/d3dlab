using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    class CameraRotateWithCursorReturntingCommand : IInputCommand {
        public InputStateData InputState { get; }

        public CameraRotateWithCursorReturntingCommand(InputStateData state) {
            this.InputState = state;
        }

        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);
            var state = entity.GetComponent<OrthographicCameraComponent>().GetState();

            var p11 = InputState.ButtonsStates[GeneralMouseButtons.Right].PointV2;
            var p2 = InputState.CurrentPosition;
            var data = new MovementData { Begin = p11, End = p2 };

            //var any = entity.GetComponents<CameraMovementComponent>();
            //if (any.Any()) {//get prev state... means manipulate is continuing 
            //    state = any.Single().State;
            //}

            entity.UpdateComponent(CameraMovementComponent.CreateRotate(state, data, 2.5f));

            return true;
        }
    }
}
