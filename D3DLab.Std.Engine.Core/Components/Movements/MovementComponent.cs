using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components.Movements {
    public class KeywordMovingComponent : CameraMovementComponent {
        public MovingDirection Direction { get; set; }
        public bool IsKeywordDown { get; internal set; }

        public enum MovingDirection {
            Undefined,
            MoveForward,
            MoveBackward,
            TurnLeft,
            TurnRight,
        }

        public KeywordMovingComponent() {
        }

        public override void Execute(ICameraMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }

    public struct MovementData {
        public Vector2 Begin;
        public Vector2 End;
    }
    public interface ICameraMovementComponentHandler {
        void Execute(CameraRotatingComponent component);
        void Execute(CameraZoomingComponent component);
        void Execute(IMoveToPositionComponent component);
        void Execute(KeywordMovingComponent component);
    }

    public class CameraMoveToPositionComponent : CameraMovementComponent, IMoveToPositionComponent {
        public Vector3 TargetPosition { get; set; }

        public override void Execute(ICameraMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }
    public class CameraRotatingComponent : CameraMovementComponent {
        public override void Execute(ICameraMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }
    public class CameraZoomingComponent : CameraMovementComponent {
        public int Delta { get; set; }

        public override void Execute(ICameraMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }
  
    public abstract class CameraMovementComponent : GraphicComponent {
        public CameraState State { get; set; }
        public MovementData MovementData { get; set; }

        public abstract void Execute(ICameraMovementComponentHandler handler);
    }



    public interface IMoveToPositionComponent : IGraphicComponent{
        Vector3 TargetPosition { get; set; }
    }
    public interface IMovementComponentHandler {
        void Execute(MoveCameraToTargetComponent component);
        void Execute(IMoveToPositionComponent component);
        void Execute(HitToTargetComponent component);
    }

    public abstract class MovementComponent : GraphicComponent {
        public abstract void Execute(IMovementComponentHandler handler);
    }

    public class MoveCameraToTargetComponent : MovementComponent {
        public ElementTag Target { get; set; }
        public Vector3 TargetPosition { get; set; }

        public override void Execute(IMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }


    public class HitToTargetComponent : MovementComponent {
        public Vector2 ScreenPosition { get; set; }

        public override void Execute(IMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }


}
