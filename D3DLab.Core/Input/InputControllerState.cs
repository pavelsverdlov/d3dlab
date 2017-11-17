using D3DLab.Core.Input.StateMachine;
using D3DLab.Core.Input.StateMachine.Core;

namespace D3DLab.Core.Input {
    public enum InputControllerState {
        [StateType(typeof(InputControllerStateGeneral))]
        General,
        [StateType(typeof(InputControllerStateRotate))]
        Rotate,
        [StateType(typeof(InputControllerStatePan))]
        Pan,
        [StateType(typeof(InputControllerStateDownLeft))]
        DownLeft,
        [StateType(typeof(InputControllerStateDownRight))]
        DownRight,
        [StateType(typeof(InputControllerStateDownMiddle))]
        DownMiddle,
        [StateType(typeof(InputControllerStateCustomMove))]
        CustomMove,

        Zoom
    }
}
