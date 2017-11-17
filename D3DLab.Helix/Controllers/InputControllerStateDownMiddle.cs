using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Controllers {
    public class InputControllerStateDownMiddle : InputControllerStateBase {
        public InputControllerStateDownMiddle(InputController manager) : base(manager) { }

        protected override void OnEnterState(StateMachine.Core.StateBase<InputControllerStateContext> fromState) {
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
            if(ea.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None) {
                Point3D? point;
                if(!Controller.OnGetRotateCenter(ea, out point)) {
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

            if(ea.Button == MouseButtons.Middle) {
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
}