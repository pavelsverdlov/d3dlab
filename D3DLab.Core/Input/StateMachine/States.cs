using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using D3DLab.Core.Input.StateMachine.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Cursor = System.Windows.Forms.Cursor;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace D3DLab.Core.Input.StateMachine {

    public abstract class InputControllerStateBase : StateBase<InputControllerStateContext>, IInputSupport {
        public InputControllerStateBase(InputController controller)
            : base(controller) {
        }

        public new InputController Controller { get { return (InputController)base.Controller; } }

        public virtual bool OnMouseDown(MouseEventArgs ea) {
            return false;
        }

        public virtual bool OnMouseUp(MouseEventArgs ea) {
            return false;
        }

        public virtual bool OnMouseWheel(MouseEventArgs ea) {
            //var test = Cursor.Position;
            Controller.CameraViewController.Zoom(ea.Delta, ea.X, ea.Y);
            return false;
        }

        public virtual bool OnMouseMove(MouseEventArgs ea) {
            return false;
        }

        public virtual bool OnMouseDoubleClick(MouseEventArgs ea) {
            return false;
        }

        public bool OnKeyDown(KeyEventArgs e) {
            return false;
        }
        public bool OnKeyUp(KeyEventArgs e) {
            return false;
        }

        protected static double Distance(System.Drawing.Point p1, System.Drawing.Point p2) {
            var x = p2.X - p1.X;
            var y = p2.Y - p1.Y;
            return Math.Sqrt(x * x + y * y);
        }

        protected static bool IsPressed(MouseButtons button) {
            return (Control.MouseButtons & button) == button;
        }
    }
    public class InputControllerStateGeneral : InputControllerStateBase {
        public InputControllerStateGeneral(InputController manager) : base(manager) { }

        public override bool OnMouseDown(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Left)
                SwitchToState(InputControllerState.DownLeft);

            if (ea.Button == MouseButtons.Right)
                SwitchToState(InputControllerState.DownRight);

            if (ea.Button == MouseButtons.Middle)
                SwitchToState(InputControllerState.DownMiddle);

            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            return base.OnMouseUp(ea);
        }

        public override bool OnMouseMove(MouseEventArgs ea) {
            return base.OnMouseMove(ea);
        }
    }

    public class InputControllerStateRotate : InputControllerStateBase {
        public InputControllerStateRotate(InputController manager) : base(manager) { }

        public override bool OnMouseDown(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Left)
                return SwitchToState(InputControllerState.Pan);

            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Right)
                return Controller.SwitchToState(InputControllerState.General);

            return base.OnMouseUp(ea);
        }

        public override bool OnMouseMove(MouseEventArgs ea) {
            if (!IsPressed(MouseButtons.Right)) {
                Controller.SwitchToState(InputControllerState.General);
                return false;
            }

            //System.Drawing.Point p = ea.Location;
            return MouseMoveCore(Controller.Control.PointToClient(Cursor.Position));
        }

        private bool MouseMoveCore(System.Drawing.Point p) {
            var p1 = Controller.Control.PointToClient(rotateAtPoint).ToVector2();
            var p2 = (Control.ModifierKeys & Keys.Shift) == Keys.Shift
                ? p.ToVector2()
                : GetSmoothedPoint(p);

            var moveV = p2 - p1;
            if (moveV.Length() < 0.5f)
                return false;

            Cursor.Position = rotateAtPoint;

            Controller.CameraViewController.Rotate(moveV.X, moveV.Y, rotateMode);

            return true;
        }

        List<Vector2> prevMovePoints = new List<Vector2>();
        private Vector2 GetSmoothedPoint(System.Drawing.Point p) {
            prevMovePoints.Add(p.ToVector2());
            while (prevMovePoints.Count > 2)
                prevMovePoints.RemoveAt(0);

            var p2 = prevMovePoints.First();
            for (int i = 1; i < prevMovePoints.Count; i++)
                p2 += prevMovePoints[i];
            p2 /= (float)prevMovePoints.Count;
            return p2;
        }

        CameraRotateMode rotateMode;
        System.Drawing.Point rotateAtPoint;

        protected override void OnEnterState(StateBase<InputControllerStateContext> fromState) {
            base.OnEnterState(fromState);

            rotateMode = CameraRotateMode.RotateAroundZ;
            rotateAtPoint = Cursor.Position;
            Cursor.Hide();

            prevMovePoints.Clear();

            var rotateMargins = Controller.OnGetCustomRotateMargins();

            if (rotateMargins.Left.IsNaN() || rotateMargins.Top.IsNaN() || rotateMargins.Bottom.IsNaN() ||
                rotateMargins.Right.IsNaN()) {
                rotateMode = CameraRotateMode.Rotate3D;
                return;
            }

            var w = (int)Controller.Control.Width;
            var h = (int)Controller.Control.Height;
            var rect = new global::SharpDX.Rectangle();
            rect.Left = ConvertValue(rotateMargins.Left, w);
            rect.Top = ConvertValue(rotateMargins.Top, h);
            rect.Right = ConvertValue(rotateMargins.Right, w, true);
            rect.Bottom = ConvertValue(rotateMargins.Bottom, h, true);

            var p = Context.DownPointRight.Point;

            if (rect.Contains(p.X, p.Y)) {
                rotateMode = CameraRotateMode.Rotate3D;
                return;
            }

            if (p.X < rect.Left || p.Y < rect.Top) {
                rotateMode = CameraRotateMode.RotateAroundZInverted;
            } else if (p.Y > rect.Bottom) {
                rotateMode = CameraRotateMode.RotateAroundY;
            }
            //var dx = Math.Min(Context.DownPointRight.Point.X, w - Context.DownPointRight.Point.X);
            //var dy = Math.Min(Context.DownPointRight.Point.Y, h - Context.DownPointRight.Point.Y);

            //rotateMode = dx < dy ? CameraRotateMode.RotateAroundX : CameraRotateMode.RotateAroundY;
        }

        protected override void OnLeaveState(StateBase<InputControllerStateContext> toState) {
            Cursor.Show();
            base.OnLeaveState(toState);
        }

        private static int ConvertValue(double value, double fullSize, bool isRightBottom = false) {
            if (value < 0) {
                var result = Math.Abs(fullSize * value);
                if (isRightBottom)
                    result = fullSize - result;
                return (int)result;
            }
            return (int)value;
        }
    }
    public enum CameraRotateMode {
        Rotate3D,
        RotateAroundX,
        RotateAroundY,
        RotateAroundZ,
        RotateAroundZInverted,
    }

    public class InputControllerStatePan : InputControllerStateBase {
        public InputControllerStatePan(InputController manager) : base(manager) { }

        public override bool OnMouseDown(MouseEventArgs ea) {
            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Left) {
                SwitchToState(InputControllerState.Rotate);
                return false;
            }

            if (ea.Button == MouseButtons.Right) {
                SwitchToState(InputControllerState.DownLeft);
                return false;
            }
            return base.OnMouseUp(ea);
        }

        public override bool OnMouseMove(MouseEventArgs ea) {
            bool notLeft = !IsPressed(MouseButtons.Left);
            bool notRight = !IsPressed(MouseButtons.Right);
            if (notLeft && notRight) {
                Controller.SwitchToState(InputControllerState.General);
                return false;
            }
            if (notLeft) {
                Controller.SwitchToState(InputControllerState.DownRight);
                return false;
            }
            if (notRight) {
                Controller.SwitchToState(InputControllerState.DownLeft);
                return false;
            }

            var p1 = Context.PrevMovePoint.Point;
            var p2 = ea.Location;
            Controller.CameraViewController.Pan(p2.X - p1.X, p2.Y - p1.Y);
            return true;
        }
    }

    public class InputControllerStateDownLeft : InputControllerStateBase {
        public InputControllerStateDownLeft(InputController manager) : base(manager) { }

        protected override void OnEnterState(StateMachine.Core.StateBase<InputControllerStateContext> fromState) {
            base.OnEnterState(fromState);
            canSwitchToCustomMove = !object.Equals(fromState.Id, InputControllerState.CustomMove);
            hasMove = false;
        }

        public override bool OnMouseDown(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Right)
                return SwitchToState(InputControllerState.Pan);

            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Left) {
                if (!hasMove)
                    Controller.OnClick(ea);
                Controller.SwitchToState(InputControllerState.General);
            }
            return base.OnMouseUp(ea);
        }

        bool hasMove;
        bool canSwitchToCustomMove;
        public override bool OnMouseMove(MouseEventArgs ea) {
            if (!hasMove)
                hasMove = Distance(Context.DownPointLeft.Point, ea.Location) > 1;
            if (canSwitchToCustomMove) {
                if (hasMove)
                    return Controller.SwitchToState(InputControllerState.CustomMove);
            }

            return base.OnMouseMove(ea);
        }
    }

    public class InputControllerStateDownRight : InputControllerStateBase {
        public InputControllerStateDownRight(InputController manager) : base(manager) { }

        public override bool OnMouseDown(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Left)
                return SwitchToState(InputControllerState.Pan);

            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Right && Keyboard.Modifiers == ModifierKeys.None) {
                Controller.OnContextMenu(ea);
                return Controller.SwitchToState(InputControllerState.General);
            }
            return base.OnMouseUp(ea);
        }

        public override bool OnMouseMove(MouseEventArgs ea) {
            if (Distance(Context.DownPointRight.Point, ea.Location) > 2 && Controller.OnAllowSwitchToState(InputControllerState.DownRight,InputControllerState.Rotate))
                return Controller.SwitchToState(InputControllerState.Rotate);

            return base.OnMouseMove(ea);
        }
    }

    public class InputControllerStateDownMiddle : InputControllerStateBase {
        public InputControllerStateDownMiddle(InputController manager) : base(manager) { }

        protected override void OnEnterState(StateBase<InputControllerStateContext> fromState) {
            base.OnEnterState(fromState);
            canSwitchToCustomMove = !object.Equals(fromState.Id, InputControllerState.CustomMove);
        }

        public override bool OnMouseDown(MouseEventArgs ea) {
            //            if(ea.Button == MouseButtons.Right)
            //                return SwitchToState(InputControllerState.Pan);

            return base.OnMouseDown(ea);
            //            return SwitchToState(InputControllerState.DownMiddle);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None) {
                Vector3? point;
                if (!Controller.OnGetRotateCenter(ea, out point)) {
                    //TODO: FindHits
                    //                    var hits = Controller.Viewport.FindHits(new System.Windows.Point(ea.X, ea.Y));
                    //                    if(hits != null) {
                    //                        var hit = hits.Where(i => i.IsValid && i.ModelHit.Visible).FirstOrDefault();
                    //                        if(hit.IsValid)
                    //                            point = hit.PointHit;
                    //                    }
                }
                if (point != null) {
                    Controller.CameraViewController.IsManuallyChanged = true;
                    try {
                        Controller.CameraViewController.RotateCenter = point.Value;
                        var p = new Vector2(ea.X, ea.Y);
                        var p0 = new Vector2((float)Controller.Control.Width * 0.5f, (float)Controller.Control.Height * 0.5f);
                        var v = p0 - p;
                        Controller.CameraViewController.Pan((int)v.X, (int)v.Y);
                    } finally {
                        Controller.CameraViewController.IsManuallyChanged = false;
                    }
                }
            }

            if (ea.Button == MouseButtons.Middle) {
                Controller.OnClick(ea);
                Controller.SwitchToState(InputControllerState.General);
            }

            return base.OnMouseUp(ea);
        }


        bool canSwitchToCustomMove;
        public override bool OnMouseMove(MouseEventArgs ea) {
            //            if(canSwitchToCustomMove) {
            //                if(Distance(Context.DownPointLeft.Point, ea.Location) > 2)
            //                    return Controller.SwitchToState(InputControllerState.CustomMove);
            //            }

            return base.OnMouseMove(ea);
        }
    }

    public class InputControllerStateCustomMove : InputControllerStateBase {
        public InputControllerStateCustomMove(InputController manager) : base(manager) { }

        public override bool OnMouseDown(MouseEventArgs ea) {
            if (ea.Button == MouseButtons.Right)
                return SwitchToState(InputControllerState.Pan);

            return base.OnMouseDown(ea);
        }

        public override bool OnMouseUp(MouseEventArgs ea) {
            return SwitchToState(InputControllerState.General);
        }

        public override bool OnMouseMove(MouseEventArgs ea) {
            Controller.OnCustomMove(customMoveTarget, ea);
            return true;
        }

        object customMoveTarget;
        protected override void OnEnterState(StateBase<InputControllerStateContext> fromState) {
            base.OnEnterState(fromState);
            var ea = Context.DownPointLeft.ToMouseArgsAllow();
            customMoveTarget = Controller.OnCustomMoveBegin(ea);
            if (!ea.Allow)
                SwitchToState(InputControllerState.DownLeft);
        }

        protected override void OnLeaveState(StateBase<InputControllerStateContext> toState) {
            Controller.OnCustomMoveEnd(customMoveTarget, Context.PrevMovePoint.ToMouseArgs(MouseButtons.Left));
            base.OnLeaveState(toState);
        }
    }
}
