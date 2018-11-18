using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components.Movements {
    public struct MovementData {
        public Vector2 Begin;
        public Vector2 End;
    }

    public interface IMovementComponentHandler {
        void Execute(RotationComponent component);
    }

    public class RotationComponent : MovementComponent {

        public override void Execute(IMovementComponentHandler handler) {
            handler.Execute(this);
        }
    }

    public abstract class MovementComponent : GraphicComponent {
        public CameraState State { get; set; }
        public MovementData MovementData { get; set; }

        public abstract void Execute(IMovementComponentHandler handler);
    }
}
