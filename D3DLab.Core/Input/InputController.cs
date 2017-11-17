using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using D3DLab.Core.Input.EventArgs;
using D3DLab.Core.Input.StateMachine.Core;
using D3DLab.Core.Render.Camera;
using SharpDX;

namespace D3DLab.Core.Input {
    public class InputController : StateControllerBase<InputControllerStateContext, IInputSupport> {
        public InputController(CameraViewController cameraViewController)
            : base(new InputControllerStateContext(), InputControllerState.General) {
            this.CameraViewController = cameraViewController;
            this.RotateMargins = new System.Windows.Thickness(-0.1); // 10% of border
        }

        public CameraViewController CameraViewController { get; private set; }

        public System.Windows.Thickness RotateMargins { get; set; }

        Control control;
        public Control Control {
            get { return control; }
        }

        public void Initialize(Control control) {
            if (this.control != null) {
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
            this.control = control;
            if (this.control != null) {
                this.control.MouseDown += OnMouseDown;
                this.control.MouseUp += OnMouseUp;
                this.control.MouseMove += OnMouseMove;
                this.control.MouseWheel += OnMouseWheel;
                this.control.MouseDoubleClick += OnMouseDoubleClick;
                this.control.KeyDown += OnKeyDown;
                this.control.KeyUp += OnKeyUp;
                this.control.Leave += OnLeave;
                this.control.GotFocus += control_GotFocus;
                this.control.LostFocus += control_LostFocus;
            }
        }

        void control_GotFocus(object sender, System.EventArgs e) {
        }

        void control_LostFocus(object sender, System.EventArgs e) {
            SwitchToState(InputControllerState.General);
        }

        private void OnLeave(object sender, System.EventArgs e) {
            SwitchToState(InputControllerState.General);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetCapture(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetCapture();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool ReleaseCapture();

        public override bool Active {
            get { return base.Active; }
            set {
                if (base.Active == value)
                    return;
                base.Active = value;
                if (!base.Active && Control.IsHandleCreated && GetCapture() == Control.Handle)
                    ReleaseCapture();
            }
        }

        public static System.Windows.Point GetPosition(System.Windows.Media.Visual visual) {//System.Windows.IInputElement relativeTo
                                                                                            //var visual = relativeTo as System.Windows.Media.Visual;
            if (visual == null)
                return new System.Windows.Point(-1, -1);
            var p = Control.MousePosition;
            return visual.PointFromScreen(new System.Windows.Point(p.X, p.Y));
        }

        private MouseEventArgs FixMouse(MouseEventArgs ea) {
            throw new NotImplementedException();
            //			var position = GetPosition(Viewport.RenderHost as DPFCanvas);
            //			return new MouseEventArgs(ea.Button,ea.Clicks,(int) position.X,(int) position.Y,ea.Delta);
        }

        private void OnMouseDown(object sender, MouseEventArgs ea) {
            //todo ea = FixMouse(ea);
            Control.Focus();

            if (Active) {
                if (ea.Button == MouseButtons.Left)
                    Context.DownPointLeft.Point = ea.Location;
                else if (ea.Button == MouseButtons.Right)
                    Context.DownPointRight.Point = ea.Location;
                else if (ea.Button == MouseButtons.Middle)
                    Context.DownPointMiddle.Point = ea.Location;

                Context.PrevMovePoint.Point = ea.Location;

                var curCapturedHandle = GetCapture();
                if (curCapturedHandle != control.Handle)
                    SetCapture(control.Handle);
            }

            DoEvent(sender, i => i.OnMouseDown(ea));
        }

        private void OnMouseUp(object sender, MouseEventArgs ea) {
            //todo ea = FixMouse(ea);
            if (Active && Control.MouseButtons == MouseButtons.None)
                ReleaseCapture();
            DoEvent(sender, i => i.OnMouseUp(ea));
        }

        private void OnMouseWheel(object sender, MouseEventArgs ea) {
            //todo
            //			ea = FixMouse(ea);
            DoEvent(sender, i => i.OnMouseWheel(ea));
        }

        private void OnMouseMove(object sender, MouseEventArgs ea) {
            //ea = FixMouse(ea);
            if (Active) {
                var buttons = System.Windows.Forms.Control.MouseButtons;
                if (object.Equals(CurrentState.Id, InputControllerState.General) && buttons != MouseButtons.None) {
                    EmulateMouseDown(buttons, MouseButtons.Left);
                    EmulateMouseDown(buttons, MouseButtons.Right);
                    EmulateMouseDown(buttons, MouseButtons.Middle);
                    return;
                }
            }

            DoEvent(sender, i => i.OnMouseMove(ea));

            if (Active)
                Context.PrevMovePoint.Point = ea.Location;
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs ea) {
            ea = FixMouse(ea);
            DoEvent(sender, i => i.OnMouseDoubleClick(ea));
        }

        private void EmulateMouseDown(MouseButtons buttons, MouseButtons button) {
            if ((buttons & button) != button)
                return;

            var p = System.Windows.Forms.Control.MousePosition;
            p = Control.PointToClient(p);
            OnMouseDown(Control, new MouseEventArgs(button, 1, p.X, p.Y, 0));
        }

        private void OnKeyDown(object sender, KeyEventArgs ea) {
            DoEvent(sender, i => i.OnKeyDown(ea));
        }

        private void OnKeyUp(object sender, KeyEventArgs ea) {
            DoEvent(sender, i => i.OnKeyUp(ea));
        }

        #region Events

        public bool OnAllowSwitchToState(object fromState, object toState) {
            var ea = new CancelSwitchToStateEventArgs(fromState, toState);
            if (CancelSwitchToState != null)
                CancelSwitchToState(this, ea);
            return !ea.Cancel;
        }
        public event EventHandler<CancelSwitchToStateEventArgs> CancelSwitchToState;

        internal void OnClick(MouseEventArgs ea) {
            if (Click != null)
                Click(this, ea);
        }
        public event MouseEventHandler Click;

        internal void OnContextMenu(MouseEventArgs ea) {
            if (ContextMenu != null)
                ContextMenu(this, ea);
        }
        public event MouseEventHandler ContextMenu;

        internal object OnCustomMoveBegin(AllowMouseEventArgs ea) {
            if (CustomMoveBegin != null) {
                foreach (EventHandler<AllowMouseEventArgs> handler in CustomMoveBegin.GetInvocationList()) {
                    handler(this, ea);
                    if (ea.Allow)
                        return handler.Target;
                }
            }
            return null;
        }
        public event EventHandler<AllowMouseEventArgs> CustomMoveBegin;

        void RiseEventByCustomTarget(MouseEventHandler ev, object customMoveTarget, MouseEventArgs ea) {
            if (ev == null)
                return;

            var handler = customMoveTarget != null
                ? ev.GetInvocationList().Cast<MouseEventHandler>().FirstOrDefault(i => i.Target == customMoveTarget)
                : null;

            (handler ?? ev)(this, ea);
        }

        internal void OnCustomMoveEnd(object customMoveTarget, MouseEventArgs ea) {
            RiseEventByCustomTarget(CustomMoveEnd, customMoveTarget, ea);
        }
        public event MouseEventHandler CustomMoveEnd;

        internal void OnCustomMove(object customMoveTarget, MouseEventArgs ea) {
            RiseEventByCustomTarget(CustomMove, customMoveTarget, ea);
        }
        public event MouseEventHandler CustomMove;

        internal bool OnGetRotateCenter(MouseEventArgs ea, out Vector3? point) {
            var e = new GetRotateCenterEventArgs(ea.X, ea.Y);
            if (GetRotateCenter != null)
                GetRotateCenter(this, e);
            point = e.RotateCenter;
            return e.RotateCenter != null || e.Handled;
        }
        public event EventHandler<GetRotateCenterEventArgs> GetRotateCenter;

        public System.Windows.Thickness OnGetCustomRotateMargins() {
            var ea = new GetCustomRotateMarginsEventArgs(RotateMargins);
            if (GetCustomRotateMargins != null)
                GetCustomRotateMargins(this, ea);
            return ea.RotateMargins;
        }
        public event EventHandler<GetCustomRotateMarginsEventArgs> GetCustomRotateMargins;

        #endregion
    }
}
