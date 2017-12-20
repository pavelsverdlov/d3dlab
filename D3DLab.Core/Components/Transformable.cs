using D3DLab.Core.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public class TransformComponent : D3DComponent {
        public Matrix Matrix { get; protected internal set; }

        public TransformComponent() {
            Matrix = Matrix.Identity;
        }

        public override string ToString() {
            return $"Transform[Matrix:{Matrix.ToString()}]";
        }

        public virtual void AddDeltaMatrix(Matrix m) {
            Matrix *= m;
        }

        public virtual Matrix GetMatrix() {
            return Matrix;
        }
    }

    public sealed class TransformComponentTwoWays : TransformComponent {
        private readonly List<TransformComponent> refs;
        private readonly TransformComponent owner;

        public TransformComponentTwoWays(TransformComponent owner) {
            this.owner = owner;
            refs = new List<TransformComponent>();
        }

        public override void AddDeltaMatrix(Matrix m) {
            //add delta to all refs
            refs.ForEach(x => x.AddDeltaMatrix(m));          
        }
        public void AddRefTransform(TransformComponent _ref) {
            refs.Add(_ref);
        }

        public override Matrix GetMatrix() {//return only own transform
            return owner.Matrix;
        }

    }
}
