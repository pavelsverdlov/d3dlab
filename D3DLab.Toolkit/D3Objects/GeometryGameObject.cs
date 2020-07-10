using D3DLab.ECS;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public abstract class GeometryGameObject : D3DLab.ECS.GameObject {

        protected GeometryGameObject(string desc) : base(desc) {
        }

        public virtual void ShowDebugVisualization(IEntityManager manager) {
        }
        public virtual void HideDebugVisualization(IEntityManager manager) {
        }
        public override void Cleanup(IContextState context) {
            base.Cleanup(context);

        }
    }
}
