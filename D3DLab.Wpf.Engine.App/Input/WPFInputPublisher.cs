using D3DLab.Std.Engine.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Wpf.Engine.App.Input {
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
            state.CursorCurrentPosition = point.ToWindowPoint();
            state.CurrentPosition = point.ToNumericsV2();
            state.Delta = e.Delta;
            InvokeSubscribers((s, ev) => s.OnMouseWheel(ev));
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var btn = GetMouseButton(e.ChangedButton);
            state.Buttons ^= btn;
            state.ButtonsStates[btn] = new ButtonsState {
                CursorPointDown = new WindowPoint(),
                PointDown = System.Numerics.Vector2.Zero
            };

            //if (state.Buttons == MouseButtons.None) {
            //    ReleaseCapture();
            //}

            InvokeSubscribers((s, ev) => s.OnMouseUp(ev));
        }
        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var btn = GetMouseButton(e.ChangedButton);
            state.Buttons |= btn;
            state.ButtonsStates[btn] = new ButtonsState {
                CursorPointDown = Cursor.Position.ToWindowPoint(),
                PointDown = e.GetPosition(control).ToNumericsV2()
            };
            InvokeSubscribers((s, ev) => s.OnMouseDown(ev));
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            var point = e.GetPosition(control);
            state.CursorCurrentPosition = Cursor.Position.ToWindowPoint();
            state.CurrentPosition = e.GetPosition(control).ToNumericsV2();
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
