using D3DLab.Core.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public sealed class HierarchicalTransformComponent : TransformComponent {
        private readonly TransformComponent originalParent;
        private readonly List<TransformComponent> children;
        Matrix delta;

        public HierarchicalTransformComponent(TransformComponent parent) {
            this.originalParent = parent;
            children = new List<TransformComponent>();
            delta = Matrix.Identity;
        }
        public override void AddDeltaMatrix(Matrix m) {
            children.ForEach(x=>x.AddDeltaMatrix(m));
            delta *= m;
        }

        public void AddRefTransform(TransformComponent _ref) {
            children.Add(_ref);
        }

        public override Matrix GetMatrix() {
            return originalParent.Matrix * delta;
        }
        public override string ToString() {
            return $"HierarchicalTransform[Matrix:{Matrix.ToString()}; Children:{children.Count}]";
        }
    }

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
}
