using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Host;
using D3DLab.Core.Input;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Point = System.Drawing.Point;

namespace D3DLab.Core.Components {
    public abstract class InputComponent : Component {

        #region input data
        protected sealed class StateDictionary : Dictionary<int, Func<StateProcessor, InputState>> { }
        public sealed class ButtonsState {
            private readonly Control control;

            public ButtonsState(Control control) {
                this.control = control;
            }

            public Vector2 PointDown { get; set; } //=> control.PointToClient(CursorPointDown).ToVector2();
            public Point CursorPointDown { get; set; }
        }
        public sealed class InputStateDate {
            private readonly Control control;
            public MouseButtons Buttons { get; set; }

            public Vector2 CurrentPosition { get; set; }//=> control.PointToClient(CursorCurrentPosition).ToVector2();

            public Point CursorCurrentPosition { get; set; }

            public int Delta { get; set; }

            public float ControlWidth => control.Width;
            public float ControlHeight => control.Height;

            public IReadOnlyDictionary<MouseButtons, ButtonsState> ButtonsStates => buttonsStates;
            private readonly Dictionary<MouseButtons, ButtonsState> buttonsStates;
            public bool IsPressed(MouseButtons button) {
                return (Buttons & button) == button;
            }

            public InputStateDate(Control control) {
                this.control = control;
                buttonsStates = new Dictionary<MouseButtons, ButtonsState>();
                buttonsStates.Add(MouseButtons.Right, new ButtonsState(control));
                buttonsStates.Add(MouseButtons.Left, new ButtonsState(control));
                buttonsStates.Add(MouseButtons.Middle, new ButtonsState(control));
            }
        }

        #endregion
        public interface IHandler {}
        protected sealed class StateHandleProcessor<THandler> : InputComponent.StateProcessor where THandler : IHandler {
            private readonly THandler inputHandler;
            public StateHandleProcessor(StateDictionary states, THandler inputHandler) : base(states) {
                this.inputHandler = inputHandler; 
            }

            public override void InvokeHandler<T>(Action<T> action) {
                //                Dispatcher.CurrentDispatcher.BeginInvoke(action, inputHandler);
                // action.BeginInvoke(inputHandler, null, null);
                //                Task.Run(() => action(inputHandler));
                var handler = (IHandler)inputHandler;
                action((T)handler); 
            }
        }
        protected abstract class StateProcessor : InputState {
            private InputState current;
            private readonly StateDictionary states;
            protected StateProcessor(StateDictionary states) : base() {
                this.states = states;
                /*
                var matrix = new Action<InputStateDate>[3, 3] {
                      //Left    //Right                 //Middle
                    {   null,   InputHandler.Pan,       null    },//Left 
                    {   null,   InputHandler.Rotate,    null    },//Right
                    {   null,   null,                   null    } //Middle
                };
                */
            }

            public abstract void InvokeHandler<T>(Action<T> action) where T : IHandler;

            public override void SwitchTo(int stateTo, InputStateDate state) {
                current?.LeaveState(state);
                current = states[stateTo](this);
                current.EnterState(state);
            }
            public override bool OnMouseMove(InputStateDate state) { return current.OnMouseMove(state); }
            public override bool OnMouseDown(InputStateDate state) { return current.OnMouseDown(state); }
            public override bool OnMouseUp(InputStateDate state) { return current.OnMouseUp(state); }
            public override bool OnMouseWheel(InputStateDate ev) { return current.OnMouseWheel(ev); }
        }

        protected abstract class InputStateMachine : InputState {
            protected readonly StateProcessor Processor;

            protected InputStateMachine(StateProcessor processor) {
                this.Processor = processor;
            }

            public override void SwitchTo(int stateTo, InputStateDate state) {
                Processor.SwitchTo(stateTo, state);
            }
        }

        protected abstract class InputState {
            public virtual void EnterState(InputStateDate inputStateDate) {
                
            }
            public void LeaveState(InputStateDate inputStateDate) {

            }

            public virtual bool OnMouseMove(InputStateDate state) {
                return false;
            }
            public virtual bool OnMouseDown(InputStateDate state) {
                return false;
            }
            public virtual bool OnMouseUp(InputStateDate state) {
                return false;
            }
            public abstract void SwitchTo(int stateTo, InputStateDate state);
            public virtual bool OnMouseWheel(InputStateDate ev) {
                return false;
            }
        }
        
        
        private class InputPublisher {
            private readonly Control control;
            private readonly List<InputComponent> subscribers;
            private readonly InputStateDate state;
            public InputPublisher(Control control) {
                this.control = control;
                subscribers = new List<InputComponent>();
                state = new InputStateDate(control);
                this.control.MouseDown += OnMouseDown;
                this.control.MouseUp += OnMouseUp;
                this.control.MouseMove += OnMouseMove;
                this.control.MouseWheel += OnMouseWheel;
                this.control.MouseDoubleClick += OnMouseDoubleClick;
                this.control.MouseLeave += OnMouseLeave;

                this.control.KeyDown += OnKeyDown;
                this.control.KeyUp += OnKeyUp;
                this.control.Leave += OnLeave;
                this.control.GotFocus += control_GotFocus;
                this.control.LostFocus += control_LostFocus;
                
            }

            /*
            Mouse events occur in the following order:
                MouseEnter
                MouseMove
                MouseHover / MouseDown / MouseWheel
                MouseUp
                MouseLeave
            */

            #region com

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
            public static extern bool ReleaseCapture();

            #endregion

            #region event handlers


            private void OnMouseLeave(object sender, EventArgs e) {
                var btn = Control.MouseButtons;
                state.Buttons = state.Buttons | btn;
              
            }

            private void control_LostFocus(object sender, EventArgs e) {
                
            }

            private void control_GotFocus(object sender, EventArgs e) {
                
            }

            private void OnLeave(object sender, EventArgs e) {
                
            }

            private void OnKeyUp(object sender, KeyEventArgs e) {
            }

            private void OnKeyDown(object sender, KeyEventArgs e) {
                
            }

            private void OnMouseDoubleClick(object sender, MouseEventArgs e) {
                
            }

            private void OnMouseWheel(object sender, MouseEventArgs e) {
                state.CursorCurrentPosition = e.Location;
                state.CurrentPosition = control.PointToClient(state.CursorCurrentPosition).ToVector2();
                state.Delta = e.Delta;
                InvokeSubscribers(subscribers, (s, ev) => s.OnMouseWheel(ev), state);
            }

            private void OnMouseUp(object sender, MouseEventArgs e) {
                state.Buttons ^= e.Button;
                state.ButtonsStates[e.Button].CursorPointDown = Point.Empty;
                state.ButtonsStates[e.Button].PointDown = Vector2.Zero;

                if (state.Buttons == MouseButtons.None) {
                    ReleaseCapture();
                }

                InvokeSubscribers(subscribers, (s, ev) => s.OnMouseUp(ev), state);
            }
            private void OnMouseDown(object sender, MouseEventArgs e) {
                state.Buttons |= e.Button;
                state.ButtonsStates[e.Button].CursorPointDown = Cursor.Position;
                state.ButtonsStates[e.Button].PointDown = control.PointToClient(state.ButtonsStates[e.Button].CursorPointDown).ToVector2();
                InvokeSubscribers(subscribers, (s, ev) => s.OnMouseDown(ev), state);
            }

            private void OnMouseMove(object sender, MouseEventArgs e) {
                state.CursorCurrentPosition = Cursor.Position;
                state.CurrentPosition = control.PointToClient(state.CursorCurrentPosition).ToVector2();
                InvokeSubscribers(subscribers, (s, ev) => s.OnMouseMove(ev), state);
            }

            private static void InvokeSubscribers<T>(IEnumerable<InputComponent> subscribers, Func<InputComponent,T,bool> action, T ev) {
                foreach (var component in subscribers) {
                    if (action(component, ev)) {
                        break;
                    }
                }
            }
            
            #endregion

            public void Subscrube(InputComponent s) {
                subscribers.Add(s);
            }
            public void UnSubscruber(InputComponent s) {
                subscribers.Remove(s);
            }
            public void Dispose() {
                subscribers.Clear();
                this.control.MouseDown -= OnMouseDown;
                this.control.MouseUp -= OnMouseUp;
                this.control.MouseMove -= OnMouseMove;
                this.control.MouseWheel -= OnMouseWheel;
                this.control.MouseDoubleClick -= OnMouseDoubleClick;
                this.control.KeyDown -= OnKeyDown;
                this.control.KeyUp -= OnKeyUp;
                this.control.Leave -= OnLeave;
                this.control.GotFocus -= control_GotFocus;
                this.control.LostFocus -= control_LostFocus;
            }
            public bool AnySubscrubers() {
                return subscribers.Count > 0;
            }
        }
        
        private static InputPublisher publisher;
        private static readonly object loker;

        static InputComponent() {
            loker = new object();
        }

        private InputState stateMachine;
        private InputState StateMachine {
            get { return stateMachine ?? (stateMachine = GetIdleState()); }
        }

        protected readonly Control control;
        protected InputComponent(Control control) {
            this.control = control;
            lock (loker) {
                if (publisher == null) {
                    publisher = new InputPublisher(control);
                }
                publisher.Subscrube(this);
            }
        }
        protected abstract InputState GetIdleState();

        private bool OnMouseMove(InputStateDate state) { return StateMachine.OnMouseMove(state); }
        private bool OnMouseDown(InputStateDate state) { return StateMachine.OnMouseDown(state); }
        private bool OnMouseUp(InputStateDate state) { return StateMachine.OnMouseUp(state); }
        private bool OnMouseWheel(InputStateDate ev) { return StateMachine.OnMouseWheel(ev); }



        public override void Dispose() {
            lock (loker) {
                publisher.UnSubscruber(this);
                if (!publisher.AnySubscrubers()) {
                    publisher.Dispose();
                    publisher = null;
                }
            }
            base.Dispose();
        }

        
    }
}
