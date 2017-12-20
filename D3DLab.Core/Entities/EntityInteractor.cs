using D3DLab.Core.Components;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.Core.Entities {
    public class EntityInteractor {
        public void ManipulateInteractingOneWay(Entity parent, Entity child) {
            var ptc = parent.GetComponent<TransformComponent>();
            var ctc = child.GetComponent<TransformComponent>();

            var refCom = new HierarchicalTransformComponent(ctc);

            refCom.AddRefTransform(ptc);

            child.RemoveComponent(ctc);
            child.AddComponent(refCom);
        }

        public void ManipulateInteractingTwoWays(Entity owner, IEnumerable<Entity> children) {
            var ownerTC = owner.GetComponent<TransformComponent>();
            var ownerRTC = new HierarchicalTransformComponent(ownerTC);

            foreach (var child in children) {
                var ctc = child.GetComponent<TransformComponent>();
                
                child.RemoveComponent(ctc);
                //each child has ref to owner transform component 
                //thant means child can't change own transform directly only throught the parent transform component
                child.AddComponent(ownerRTC);

                ownerRTC.AddRefTransform(ctc);
            }

            owner.RemoveComponent(ownerTC);
            owner.AddComponent(ownerRTC);
        }
    }
}
