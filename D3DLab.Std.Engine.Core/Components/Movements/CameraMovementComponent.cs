using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components.Movements {
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
}
