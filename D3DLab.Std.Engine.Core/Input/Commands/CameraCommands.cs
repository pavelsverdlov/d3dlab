using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Linq;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Input.Commands {
    public class ForceRenderCommand : IInputCommand {
        public bool Execute(GraphicEntity entity) {
            return true;
        }
    }

    public class ToCenterWorldCommand : IInputCommand {
        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            var ccom = find.First();

            ccom.ResetToDefault();

            return true;
        }
    }

    public class CameraZoomCommand : IInputCommand {
        const float scrollSpeed = 0.5f;
        readonly InputStateData state;

        public CameraZoomCommand(InputStateData state) {
            this.state = state;
        }

        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            var ccom = find.First();
            var delta = state.Delta;

            ccom.ZoomTo(delta, state.CurrentPosition);

            return true;
        }
    }

    public class CameraRotateCommand : IInputCommand {
        readonly InputStateData state;

        public CameraRotateCommand(InputStateData state) {
            this.state = state;
        }

        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            var ccom = find.First();

            var p11 = state.ButtonsStates[GeneralMouseButtons.Right].PointDown;
            var p2 = state.CurrentPosition;

            var moveV = p2 - p11;
            if (moveV == Vector2.Zero) {
                return false;
            }
            var v2Up = new Vector2(0, -1);
            var mouseMove = moveV;
            var angleLook = v2Up.AngleRad(mouseMove.Normalize());

            //Console.WriteLine($"Angle 2D: {v2Up.Angle(mouseMove.Normalize())}");

            var look = ccom.LookDirection.Normalize();
            var up = ccom.UpDirection.Normalize();

            var rotatedUp = Vector3.TransformNormal(up, Matrix4x4.CreateFromAxisAngle(look, angleLook));
            var cross = Vector3.Cross(look, rotatedUp);

            var angle = mouseMove.Length();

            var movetozero = Matrix4x4.CreateTranslation(ccom.RotatePoint * -1f);
            var rotate = Matrix4x4.CreateFromAxisAngle(cross, angle.ToRad());
            var returntocenter = Matrix4x4.CreateTranslation(ccom.RotatePoint);
            var matrixRotate = movetozero * rotate * returntocenter;

            if (matrixRotate.IsIdentity) {
                return false;
            }

            ccom.UpDirection = Vector3.TransformNormal(ccom.UpDirection.Normalize(), matrixRotate).Normalize();
            ccom.LookDirection = Vector3.TransformNormal(ccom.LookDirection.Normalize(), matrixRotate).Normalize();
            ccom.Position = Vector3.Transform(ccom.Position, matrixRotate);

            return true;
        }
    }

    public class ChangeCameraRotationCenter : IInputCommand {
        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }



            return true;
        }
    }

    public class CameraPanCommand : CameraCommand {
        public CameraPanCommand(InputStateData state) : base(state) { }
        protected override bool Executing(CameraComponent comp) {
            var p1 = state.PrevPosition;
            var p2 = state.CurrentPosition;
            var move = new Vector2(p2.X - p1.X, p2.Y - p1.Y);

            comp.Pan(move);

            return true;
        }
    }

    public abstract class CameraCommand : IInputCommand {
        protected readonly InputStateData state;

        public CameraCommand(InputStateData state) {
            this.state = state;
        }
        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            var ccom = find.First();
            return Executing(ccom);

        }
        protected abstract bool Executing(CameraComponent comp);
    }

    /*
    public class ZoomToObjectCommand : IInputCommand {
        readonly BoundingBox box;

        public ZoomToObjectCommand(BoundingBox box) {
            this.box = box;
        }

        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            var com = find.First();

            var x = box.SizeX();
            var y = box.SizeY();
            var ratio = x / y;
            var max = Math.Max(box.SizeX(), box.SizeY()) * 1.2f;

            com.Position = box.GetCenter() + Vector3.UnitZ * max;
            com.RotatePoint = box.GetCenter();
            com.Width = max;
            com.LookDirection = new Vector3(0, 0, -3);
            com.UpDirection = new Vector3(0, 1, 0);
            com.Scale = 1;
            // com.LookDirection = -Vector3.UnitZ * max;

            //ccom.Width *= box.SizeX() / viewport.ActualWidth;
            //var oldTarget = pcam.Position + pcam.LookDirection;
            //var distance = pcam.LookDirection.Length;
            //var newTarget = centerRay.PlaneIntersection(oldTarget, w);
            //if (newTarget != null) {
            //    orthographicCamera.LookDirection = w * distance;
            //    orthographicCamera.Position = newTarget.Value - orthographicCamera.LookDirection;
            //}

            return true;
        }
    }*/
}
