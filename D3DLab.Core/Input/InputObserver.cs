using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Point = System.Drawing.Point;
using D3DLab.Std.Engine.Core.Input;

namespace D3DLab.Core.Input {

    public class WinFormInputPublisher : BaseInputPublisher {
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
            var p = e.Location;
            state.CursorCurrentPosition = p.ToWindowPoint();
            state.CurrentPosition = control.PointToClient(p).ToNumericsV2();
            state.Delta = e.Delta;
            InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
        }

        private void OnMouseUp(object sender, MouseEventArgs e) {
            var btn = GetMouseButton(e.Button);
            state.Buttons ^= btn;
            state.ButtonsStates[btn].CursorPointDown = new WindowPoint();
            state.ButtonsStates[btn].PointDown = System.Numerics.Vector2.Zero;

            if (btn == GeneralMouseButtons.None) {
                ReleaseCapture();
            }

            InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
        }
        private void OnMouseDown(object sender, MouseEventArgs e) {
            var btn = GetMouseButton(e.Button);
            state.Buttons |= btn;
            state.ButtonsStates[btn].CursorPointDown = Cursor.Position.ToWindowPoint();
            state.ButtonsStates[btn].PointDown = control.PointToClient(Cursor.Position).ToNumericsV2();
            InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            state.CurrentPosition = control.PointToClient(Cursor.Position).ToNumericsV2();
            InvokeSubscribers((s, ev) => s.OnMouseMove(ev));
        }

        protected static GeneralMouseButtons GetMouseButton(MouseButtons state) {
            switch (state) {
                case MouseButtons.Left:
                    return GeneralMouseButtons.Left;
                case MouseButtons.Right:
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
            this.control.MouseDoubleClick -= OnMouseDoubleClick;
            this.control.KeyDown -= OnKeyDown;
            this.control.KeyUp -= OnKeyUp;
            this.control.Leave -= OnLeave;
            this.control.GotFocus -= control_GotFocus;
            this.control.LostFocus -= control_LostFocus;
        }
    }

    public class WPFInputPublisher : BaseInputPublisher {
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
            state.CursorCurrentPosition = new WindowPoint((int)point.X, (int)point.Y);
            state.CurrentPosition = point.ToV2();
            state.Delta = e.Delta;
            InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var btn = GetMouseButton(e.ChangedButton);
            state.Buttons ^= btn;
            state.ButtonsStates[btn].CursorPointDown = new WindowPoint();
            state.ButtonsStates[btn].PointDown = System.Numerics.Vector2.Zero;

            //if (state.Buttons == MouseButtons.None) {
            //    ReleaseCapture();
            //}

            InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
        }
        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var btn = GetMouseButton(e.ChangedButton);
            state.Buttons |= btn;
            state.ButtonsStates[btn].CursorPointDown = Cursor.Position.ToWindowPoint();
            state.ButtonsStates[btn].PointDown = e.GetPosition(control).ToV2();
            InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            var point = e.GetPosition(control);
            state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            state.CurrentPosition = e.GetPosition(control).ToV2();
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



}
