using D3DLab.Core.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public sealed class RefTransformComponent : TransformComponent {
        private readonly TransformComponent current;
        TransformComponent _ref;

        public RefTransformComponent(TransformComponent current) {
            this.current = current;
        }
        public override void AddDeltaMatrix(Matrix m) {
            _ref.Matrix *= m;
            Matrix = current.Matrix * _ref.Matrix;
        }

        public void AddRefTransform(TransformComponent _ref) {
            this._ref = _ref;
        }
    }

    public class TransformComponent : D3DComponent {
        public Matrix Matrix { get; protected internal set; }

        public TransformComponent() {
            Matrix = Matrix.Identity;
        }

        public override string ToString() {
            return $"Matrix[{Matrix.ToString()}]";
        }

        public virtual void AddDeltaMatrix(Matrix m) {
            Matrix *= m;
        }

    }
}
