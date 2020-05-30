using D3DLab.ECS.Input;
using D3DLab.Toolkit.Input.Commands;
using D3DLab.Toolkit.Input.Commands.Camera;
using System.Windows;

namespace D3DLab.Toolkit.Input {
    public class RotateZoomPanInputObserver : DefaultInputObserver {
        readonly ICameraInputHandler externalInputHandler;

        public RotateZoomPanInputObserver(FrameworkElement control, IInputPublisher publisher, ICameraInputHandler inputHandler) : base(control, publisher) {
            this.externalInputHandler = inputHandler;
        }
        protected override InputState GetIdleState() {//initilization 
            var states = new StateDictionary();
            states.Add((int)AllInputStates.Idle, s => new InputIdleState(s));
            states.Add((int)AllInputStates.Rotate, s => new InputRotateStateWithCursorReturning(s));
            states.Add((int)AllInputStates.Zoom, s => new InputZoomState(s));
            states.Add((int)AllInputStates.Pan, s => new InputPanState(s));
            states.Add((int)AllInputStates.ChangeRotateCenter, s => new InputChangeRotateCenterState(s));
            states.Add((int)AllInputStates.HideOrShowObjectUnderCursor, s => new HideOrShowObjectUnderCursorState(s));
            states.Add((int)AllInputStates.ChangeTransparencyOnObjectUnderCursor, s => new ChangeTransparencyState(s));

            var router = new StateHandleProcessor<ICameraInputHandler>(states, this/*,externalInputHandler*/);
            router.SwitchTo((int)AllInputStates.Idle, InputStateData.Create());
            return router;
        }

        public override bool Rotate(InputStateData state) {
            currentSnapshot.AddEvent(new CameraRotateWithCursorReturntingWithoutAccelerationCommand(state.Clone()));
            return true;
        }
    }
    public class RotateZoomInputObserver : DefaultInputObserver {
        public RotateZoomInputObserver(FrameworkElement control, IInputPublisher publisher) : base(control, publisher) {
        }

        protected override InputState GetIdleState() {//initilization 
            var states = new StateDictionary();

            states.Add((int)AllInputStates.Idle, s => new InputIdleState(s));
            states.Add((int)AllInputStates.Rotate, s => new InputRotateStateWithCursorReturning(s));
            states.Add((int)AllInputStates.Zoom, s => new InputZoomState(s));
          //  states.Add((int)AllInputStates.Pan, s => new InputPanState(s));
            //states.Add((int)AllInputStates.Target, s => new InputTargetState(s));
            //states.Add((int)AllInputStates.KeywordDown, s => new KeywordMovingState(s));
            //states.Add((int)AllInputStates.ChangeFocus, s => new FocusToObjectState(s));

            var router = new StateHandleProcessor<ICameraInputHandler>(states, this);
            router.SwitchTo((int)AllInputStates.Idle, InputStateData.Create());
            return router;
        }
    }

}
