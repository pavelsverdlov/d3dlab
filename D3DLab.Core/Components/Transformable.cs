using D3DLab.Core.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public sealed class RefTransformComponent : TransformComponent {
        public override Matrix Matrix {
            get {
                return _ref.Matrix * matrix;
            }
            set {
                matrix = value;
            }
        }

        private Matrix matrix;

        readonly TransformComponent _ref;

        public RefTransformComponent(TransformComponent _ref, Matrix current) {
            this._ref = _ref;
            matrix = current;
        }

    }
    public class TransformComponent : D3DComponent {
        public virtual Matrix Matrix { get; set; }

        public override string ToString() {
            return $"Matrix[{Matrix.ToString()}]";
        }
    }
}
