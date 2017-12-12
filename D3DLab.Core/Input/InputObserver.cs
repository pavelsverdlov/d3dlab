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

namespace D3DLab.Core.Input {
    public interface IInputPublisher {
        bool AnySubscrubers();
        void Dispose();
        void Subscrube(InputObserver  s);
        void UnSubscruber(InputObserver  s);
    }
    [Flags]
    public enum GeneralMouseButtons {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        XButton1 = 4,
        XButton2 = 5
    }

    public abstract class InputObserver  : IDisposable{

        #region input data
        protected sealed class StateDictionary : Dictionary<int, Func<StateProcessor, InputState>> { }
        public sealed class ButtonsState {
           // private readonly Control control;

            public ButtonsState() {
                //this.control = control;
            }

            public Vector2 PointDown { get; set; } //=> control.PointToClient(CursorPointDown).ToVector2();
            public Point CursorPointDown { get; set; }
        }
        public sealed class InputStateData {
           // private readonly Control control;
            public GeneralMouseButtons Buttons { get; set; }

            public Vector2 CurrentPosition { get; set; }//=> control.PointToClient(CursorCurrentPosition).ToVector2();

            public Point CursorCurrentPosition { get; set; }

            public int Delta { get; set; }

           // public float ControlWidth => control.Width;
         //   public float ControlHeight => control.Height;

            public IReadOnlyDictionary<GeneralMouseButtons, ButtonsState> ButtonsStates => buttonsStates;
            private readonly Dictionary<GeneralMouseButtons, ButtonsState> buttonsStates;
            public bool IsPressed(GeneralMouseButtons button) {
                return (Buttons & button) == button;
            }

            public InputStateData() {
               // this.control = control;
                buttonsStates = new Dictionary<GeneralMouseButtons, ButtonsState>();
                buttonsStates.Add(GeneralMouseButtons.Right, new ButtonsState());
                buttonsStates.Add(GeneralMouseButtons.Left, new ButtonsState());
                buttonsStates.Add(GeneralMouseButtons.Middle, new ButtonsState());
            }
        }

        #endregion
        public interface IHandler {}
        protected sealed class StateHandleProcessor<THandler> : InputObserver .StateProcessor where THandler : IHandler {
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

            public override void SwitchTo(int stateTo, InputStateData state) {
                current?.LeaveState(state);
                current = states[stateTo](this);
                current.EnterState(state);
            }
            public override bool OnMouseMove(InputStateData state) { return current.OnMouseMove(state); }
            public override bool OnMouseDown(InputStateData state) { return current.OnMouseDown(state); }
            public override bool OnMouseUp(InputStateData state) { return current.OnMouseUp(state); }
            public override bool OnMouseWheel(InputStateData ev) { return current.OnMouseWheel(ev); }
        }

        protected abstract class InputStateMachine : InputState {
            protected readonly StateProcessor Processor;

            protected InputStateMachine(StateProcessor processor) {
                this.Processor = processor;
            }

            public override void SwitchTo(int stateTo, InputStateData state) {
                Processor.SwitchTo(stateTo, state);
            }
        }

        protected abstract class InputState {
            public virtual void EnterState(InputStateData inputStateDate) {
                
            }
            public void LeaveState(InputStateData inputStateDate) {

            }

            public virtual bool OnMouseMove(InputStateData state) {
                return false;
            }
            public virtual bool OnMouseDown(InputStateData state) {
                return false;
            }
            public virtual bool OnMouseUp(InputStateData state) {
                return false;
            }
            public abstract void SwitchTo(int stateTo, InputStateData state);
            public virtual bool OnMouseWheel(InputStateData ev) {
                return false;
            }
        }


        #region publishers

        public abstract class BaseInputPublisher : IInputPublisher {
            private readonly List<InputObserver > subscribers;
            protected readonly InputStateData state;
            public BaseInputPublisher() {
                subscribers = new List<InputObserver >();
                state = new InputStateData();
            }

            public void Subscrube(InputObserver  s) {
                subscribers.Add(s);
            }
            public void UnSubscruber(InputObserver  s) {
                subscribers.Remove(s);
            }
            public virtual void Dispose() {
                subscribers.Clear();
            }
            public bool AnySubscrubers() {
                return subscribers.Count > 0;
            }
            protected void InvokeSubscribers(Func<InputObserver , InputStateData, bool> action) {
                InvokeSubscribers(subscribers, action, state);
            }            

            private static void InvokeSubscribers<T>(IEnumerable<InputObserver > subscribers, Func<InputObserver , T, bool> action, T ev) {
                foreach (var component in subscribers) {
                    if (action(component, ev)) {
                        break;
                    }
                }
            }
        }

        private class WinFormInputPublisher : BaseInputPublisher {
            private readonly Control control;
            
            
            public WinFormInputPublisher(Control control) {
                this.control = control;
               
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


            private void OnMouseLeave(object sender, System.EventArgs e) {
                var btn = Control.MouseButtons;
               // state.Buttons = state.Buttons | btn;

            }

            private void control_LostFocus(object sender, System.EventArgs e) {

            }

            private void control_GotFocus(object sender, System.EventArgs e) {

            }

            private void OnLeave(object sender, System.EventArgs e) {

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
                InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
            }

            private void OnMouseUp(object sender, MouseEventArgs e) {
                var btn = GetMouseButton(e.Button);
                state.Buttons ^= btn;
                state.ButtonsStates[btn].CursorPointDown = Point.Empty;
                state.ButtonsStates[btn].PointDown = Vector2.Zero;

                if (btn == GeneralMouseButtons.None) {
                    ReleaseCapture();
                }

                InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
            }
            private void OnMouseDown(object sender, MouseEventArgs e) {
                var btn = GetMouseButton(e.Button);
                state.Buttons |= btn;
                state.ButtonsStates[btn].CursorPointDown = Cursor.Position;
                state.ButtonsStates[btn].PointDown = control.PointToClient(state.ButtonsStates[btn].CursorPointDown).ToVector2();
                InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
            }

            private void OnMouseMove(object sender, MouseEventArgs e) {
                state.CursorCurrentPosition = Cursor.Position;
                state.CurrentPosition = control.PointToClient(state.CursorCurrentPosition).ToVector2();
                InvokeSubscribers((s, ev) => s.OnMouseMove(ev));
            }

            protected static GeneralMouseButtons GetMouseButton(MouseButtons state) {
                switch (state) {
                    case MouseButtons.Left: return GeneralMouseButtons.Left;
                    case MouseButtons.Right: return GeneralMouseButtons.Right;
                }
                return GeneralMouseButtons.None;
            }

            #endregion

            public override void Dispose() {
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
        }

        private class WPFInputPublisher : BaseInputPublisher {
            private readonly System.Windows.FrameworkElement control;

            public WPFInputPublisher(System.Windows.FrameworkElement control) {
                this.control = control;

                this.control.MouseDown += OnMouseDown;
                this.control.MouseUp += OnMouseUp;
                this.control.MouseMove += OnMouseMove;
                this.control.MouseWheel += OnMouseWheel;
                //this.control.MouseDoubleClick += OnMouseDoubleClick;
                this.control.MouseLeave += OnMouseLeave;

                this.control.KeyDown += OnKeyDown;
                this.control.KeyUp += OnKeyUp;
            //    this.control.Leave += OnLeave;
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
            
            #region event handlers


            private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
                var btn = Control.MouseButtons;
               //state.Buttons = state.Buttons | btn;
            }

            private void control_LostFocus(object sender, System.Windows.RoutedEventArgs e) {

            }

            private void control_GotFocus(object sender, System.Windows.RoutedEventArgs e) {

            }
            
            private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            }

            private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {

            }

            private void OnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            }

            private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
                var point = e.GetPosition(control);
                var cp = Cursor.Position;
                state.CursorCurrentPosition = new Point((int)point.X, (int)point.Y);
                state.CurrentPosition = point.ToVector2();
                state.Delta = e.Delta;
                InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
            }

            private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
                var btn = GetMouseButton(e.ChangedButton);
                state.Buttons ^= btn;
                state.ButtonsStates[btn].CursorPointDown = Point.Empty;
                state.ButtonsStates[btn].PointDown = Vector2.Zero;

                //if (state.Buttons == MouseButtons.None) {
                //    ReleaseCapture();
                //}

                InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
            }
            private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
                var btn = GetMouseButton(e.ChangedButton);
                state.Buttons |= btn;
                state.ButtonsStates[btn].CursorPointDown = Cursor.Position;
                state.ButtonsStates[btn].PointDown = e.GetPosition(control).ToVector2();
                InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
            }

            private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
                var point = e.GetPosition(control);
                state.CursorCurrentPosition = Cursor.Position;
                state.CurrentPosition = e.GetPosition(control).ToVector2();
                InvokeSubscribers((s, ev) => s.OnMouseMove(ev));
            }

            protected static GeneralMouseButtons GetMouseButton(System.Windows.Input.MouseButton state) {
                switch (state) {
                    case System.Windows.Input.MouseButton.Left:
                        return GeneralMouseButtons.Left;
                    case System.Windows.Input.MouseButton.Right:
                        return GeneralMouseButtons.Right;
                }
                return GeneralMouseButtons.None;
            }

            #endregion

            public override void Dispose() {
                this.control.MouseDown -= OnMouseDown;
                this.control.MouseUp -= OnMouseUp;
                this.control.MouseMove -= OnMouseMove;
                this.control.MouseWheel -= OnMouseWheel;
                
                this.control.KeyDown -= OnKeyDown;
                this.control.KeyUp -= OnKeyUp;
                
                this.control.GotFocus -= control_GotFocus;
                this.control.LostFocus -= control_LostFocus;
            }
        }

        #endregion

        private static IInputPublisher publisher;
        private static readonly object loker;

        static InputObserver () {
            loker = new object();
        }

        private InputState stateMachine;
        private InputState StateMachine {
            get { return stateMachine ?? (stateMachine = GetIdleState()); }
        }

        protected readonly Control control;
        protected InputObserver (Control control) {
            this.control = control;
            lock (loker) {
                if (publisher == null) {
                    publisher = new WinFormInputPublisher(control);
                }
                publisher.Subscrube(this);
            }
        }
        protected InputObserver (System.Windows.FrameworkElement control) {
            //this.control = control;
            lock (loker) {
                if (publisher == null) {
                    publisher = new WPFInputPublisher(control);
                }
                publisher.Subscrube(this);
            }
        }
        protected abstract InputState GetIdleState();

        private bool OnMouseMove(InputStateData state) { return StateMachine.OnMouseMove(state); }
        private bool OnMouseDown(InputStateData state) { return StateMachine.OnMouseDown(state); }
        private bool OnMouseUp(InputStateData state) { return StateMachine.OnMouseUp(state); }
        private bool OnMouseWheel(InputStateData ev) { return StateMachine.OnMouseWheel(ev); }



        public void Dispose() {
            lock (loker) {
                publisher.UnSubscruber(this);
                if (!publisher.AnySubscrubers()) {
                    publisher.Dispose();
                    publisher = null;
                }
            }
        }

        
    }
}
