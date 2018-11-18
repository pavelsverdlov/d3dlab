using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
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
            entity.RemoveComponents<MovementComponent>();

            return true;
        }
    }

    public class CameraIdleCommand : IInputCommand {
        public bool Execute(GraphicEntity entity) {
            var find = entity.GetComponents<CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            entity.RemoveComponents<MovementComponent>();
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

            var p11 = state.ButtonsStates[GeneralMouseButtons.Right].PointDown;
            var p2 = state.CurrentPosition;
            var data = new MovementData { Begin = p11, End = p2 };

            var ccom = find.First();

            entity.GetOrCreateComponent(new RotationComponent { State = ccom.GetState() })
                .MovementData = data;

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
