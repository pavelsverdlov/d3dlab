using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public sealed class TargetSystem : IComponentSystem {
        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {
                var targeted = entity.GetComponent<TargetedComponent>();
                if(targeted == null) {
                    continue;
                }
                var material = entity.GetComponent<MaterialComponent>();
                material.Setected();                
            }
        }
    }
}
