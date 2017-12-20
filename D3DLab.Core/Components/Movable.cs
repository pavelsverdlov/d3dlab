using D3DLab.Core.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public sealed class HitableComponent : D3DComponent {

    }
    public sealed class TargetedComponent : D3DComponent {
    }
    public class ManipulationComponent : D3DComponent {
        public struct Input {
            public Ray PrevRay;
            public Ray CurrentRay;
        }

        public virtual Matrix CalculateDelta(Input i) {
            var deltaVector = i.PrevRay.Position - i.CurrentRay.Position;
            return Matrix.Translation(deltaVector);
        }
    }

    public class AxisManipulateComponent : ManipulationComponent {
        readonly Vector3 axis;

        public AxisManipulateComponent(Vector3 axis) {
            this.axis = axis;
        }

        public override Matrix CalculateDelta(Input i) {
            var deltaVector = i.PrevRay.Position - i.CurrentRay.Position;

            var lenght = Vector3.Dot(axis, deltaVector);
            
            return Matrix.Translation(axis * lenght);
        }
    }

    public class Simple3DMovementCaptured : D3DComponent {

    }
}
