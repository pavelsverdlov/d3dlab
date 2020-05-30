using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Input;

namespace D3DLab.Toolkit.Input.Commands.Camera {
    public class CameraRotateWithCursorReturntingWithoutAccelerationCommand : IInputCommand {
        public InputStateData InputState { get; }

        public CameraRotateWithCursorReturntingWithoutAccelerationCommand(InputStateData state) {
            this.InputState = state;
        }


        public bool Execute(ISceneSnapshot snapshot, IContextState context) {
            var entity = context.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);

            if (!entity.TryGetComponent(out GeneralCameraComponent ccom)) { return false; }

            var p11 = InputState.ButtonsStates[GeneralMouseButtons.Right].PointV2;
            var p2 = InputState.CurrentPosition;
            var data = new MovementData { Begin = p11, End = p2 };

            var state = ccom.GetState();
            entity.UpdateComponent(CameraMovementComponent.CreateRotate(state, data, 0.7f));

            return true;
        }
    }
}
