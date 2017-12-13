using D3DLab.Core.Input;
using D3DLab.Core.Test;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core.Input {

    public class CurrentInputObserver : D3DLab.Core.Input.InputObserver, 
        CurrentInputObserver.ICameraInputHandler, CurrentInputObserver.ITargetingInputHandler {

        public interface ICameraInputHandler : InputObserver .IHandler {
            bool Rotate(InputObserver .InputStateData state);
            void Zoom(InputObserver .InputStateData state);
            void Pan(InputStateData state);
        }

        public interface ITargetingInputHandler : InputObserver .IHandler {
            bool Target(InputObserver .InputStateData state);
            void UnTarget(InputStateData state);
        }
        
        protected sealed class InputIdleState : CurrentStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) { }
            
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
                    Cursor.Show();
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
                Cursor.Position = state.ButtonsStates[GeneralMouseButtons.Right].CursorPointDown;
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Rotate(state));
                return true;
            }
        }
        protected sealed class InputPanState : CurrentStateMachine {
            public InputPanState(StateProcessor processor) : base(processor) {
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    Cursor.Show();
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
            public override bool OnMouseWheel(InputStateData state) {
                Processor.InvokeHandler<ICameraInputHandler>(x => x.Zoom(state));
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
            //states.Add((int)AllInputStates.Pan, s => new InputPanState(s));

            states.Add((int)AllInputStates.Target, s => new InputTargetState(s));

            var router = new StateHandleProcessor<ICameraInputHandler>(states, this);
            router.SwitchTo((int)AllInputStates.Idle, new InputStateData());
            return router;
        }

        protected abstract class CurrentStateMachine : InputStateMachine {
            protected CurrentStateMachine(StateProcessor processor) : base(processor) { }
        }

        readonly IInputContext context;

        public CurrentInputObserver(Control control, IInputContext context) : base(control) {
            this.context = context;
        }
        public CurrentInputObserver(System.Windows.FrameworkElement control, IInputContext context) : base(control) {
            this.context = context;
        }
        public void Zoom(InputStateData state) {
            context.AddEvent(new InputEventState { Data = state, Type = AllInputStates.Zoom });
        }
        public bool Rotate(InputStateData state) {
            context.AddEvent(new InputEventState { Data = state, Type = AllInputStates.Rotate });
            return false;
        }
        public void Pan(InputStateData state) {
            context.AddEvent(new InputEventState { Data = state, Type = AllInputStates.Zoom });
        }

        public bool Target(InputStateData state) {
            context.AddEvent(new InputEventState { Type = AllInputStates.Target, Data = state });
            return false;
        }
        public void UnTarget(InputStateData state) {
            context.AddEvent(new InputEventState { Type = AllInputStates.UnTarget, Data = state });
        }
        

        private InputEventState events;        
    }
    
    public struct InputEventState {//
        public AllInputStates Type { get; set; }
        public InputObserver.InputStateData Data { get; set; }
    }
}
