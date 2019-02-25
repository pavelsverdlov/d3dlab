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
    
    public interface IMoveToPositionComponent : IGraphicComponent{
        Vector3 TargetPosition { get; set; }
    }

    public interface IMovementComponentHandler {
        void Execute(MoveCameraToTargetComponent component);
        void Execute(IMoveToPositionComponent component);
        void Execute(HitToTargetComponent component);
        void Execute(FollowUpTargetComponent component);
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
