using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Input;
using D3DLab.Std.Engine.Core.Input.Commands;
using System.Windows;
using System.Windows.Forms;

namespace D3DLab.Wpf.Engine.App.Input {
    public enum AllInputStates {
        Idle = 0,
        Rotate = 1,
        Pan = 2,
        Zoom = 3,
        Target = 4,
        UnTarget = 5
    }
    public struct InputEventState {
        public AllInputStates Type { get; set; }
        public InputStateData Data { get; set; }
    }

    public class CurrentInputObserver : InputObserver,
        CurrentInputObserver.ICameraInputHandler, CurrentInputObserver.ITargetingInputHandler {

        public interface ICameraInputHandler : InputObserver.IHandler {
            bool Rotate(InputStateData state);
            void Zoom(InputStateData state);
            void Pan(InputStateData state);
            void Idle();
        }

        public interface ITargetingInputHandler : InputObserver.IHandler {
            bool Target(InputStateData state);
            void UnTarget(InputStateData state);
        }

        protected sealed class InputIdleState : CurrentStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Idle());
            }

            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    //camera
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Pan, state);
                        break;
                    case GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Rotate, state);
                        break;

                    //case GeneralMouseButtons.Middle:
                    //    break;

                    //manipulation
                    case GeneralMouseButtons.Left:
                        SwitchTo((int)AllInputStates.Target, state);
                        break;
                }
                return base.OnMouseDown(state);
            }

            public override bool OnMouseWheel(InputStateData state) {
                SwitchTo((int)AllInputStates.Zoom, state);
                return base.OnMouseWheel(state);
            }
        }

        #region Camera

        protected sealed class InputRotateState : CurrentStateMachine {
            public InputRotateState(StateProcessor processor) : base(processor) {
                // Cursor.Hide();
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    SwitchTo((int)AllInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        SwitchTo((int)AllInputStates.Pan, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseMove(InputStateData state) {
               // Cursor.Position = state.ButtonsStates[GeneralMouseButtons.Right].CursorPointDown.ToDrawingPoint();
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Rotate(state));
                return true;
            }
        }
        protected sealed class InputPanState : CurrentStateMachine {
            public InputPanState(StateProcessor processor) : base(processor) {
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    SwitchTo((int)AllInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }

            public override bool OnMouseMove(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Pan(state));
                return false;
            }
        }
        protected sealed class InputZoomState : CurrentStateMachine {
            public InputZoomState(StateProcessor processor) : base(processor) { }

            public override void EnterState(InputStateData ev) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Zoom(ev));
            }

            public override bool OnMouseWheel(InputStateData ev) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Zoom(ev));
                return true;
            }

            public override bool OnMouseMove(InputStateData state) {
                SwitchTo((int)AllInputStates.Idle, state);
                return false;
            }
        }

        #endregion

        protected sealed class InputTargetState : CurrentStateMachine {
            public InputTargetState(StateProcessor processor) : base(processor) {

            }

            public override void EnterState(InputStateData state) {
                Processor.InvokeHandler<ITargetingInputHandler>(x => x.Target(state));
            }

            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left:
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Left) != GeneralMouseButtons.Left) {
                    Processor.InvokeHandler<ITargetingInputHandler>(x => x.UnTarget(state));
                    SwitchTo((int)AllInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
            public override bool OnMouseMove(InputStateData state) {

                return true;
            }
        }

        protected override InputState GetIdleState() {//initilization 
            var states = new StateDictionary();

            states.Add((int)AllInputStates.Idle, s => new InputIdleState(s));
            states.Add((int)AllInputStates.Rotate, s => new InputRotateState(s));
            states.Add((int)AllInputStates.Zoom, s => new InputZoomState(s));
            states.Add((int)AllInputStates.Pan, s => new InputPanState(s));

            states.Add((int)AllInputStates.Target, s => new InputTargetState(s));

            var router = new StateHandleProcessor<ICameraInputHandler>(states, this);
            router.SwitchTo((int)AllInputStates.Idle, InputStateData.Create());

            return router;
        }

        protected abstract class CurrentStateMachine : InputStateMachine {
            protected CurrentStateMachine(StateProcessor processor) : base(processor) { }
        }


        readonly FrameworkElement control;

        public CurrentInputObserver(FrameworkElement control, IInputPublisher publisher) : base(publisher) {
            this.currentSnapshot = new InputSnapshot();
            this.control = control;
        }
        public void Zoom(InputStateData state) {
            currentSnapshot.AddEvent(new CameraZoomCommand(state.Clone()));
        }
        public bool Rotate(InputStateData state) {
            currentSnapshot.AddEvent(new CameraRotateCommand(state.Clone()));
            return true;
        }
        public void Pan(InputStateData state) {
            currentSnapshot.AddEvent(new CameraPanCommand(state.Clone()));
        }

        public void Idle() {
            currentSnapshot.AddEvent(new CameraIdleCommand());
        }

        public bool Target(InputStateData state) {
            //currentSnapshot.AddEvent(new InputEventState { Type = AllInputStates.Target, Data = state });
            return false;
        }
        public void UnTarget(InputStateData state) {
            //currentSnapshot.AddEvent(new InputEventState { Type = AllInputStates.UnTarget, Data = state });
        }
    }
}
