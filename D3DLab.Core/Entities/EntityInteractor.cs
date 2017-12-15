using D3DLab.Core.Components;

namespace D3DLab.Core.Entities {
    public class EntityInteractor {
        public void ManipulateInteractingOneWay(Entity parent, Entity child) {
            var ptc = parent.GetComponent<TransformComponent>();
            var ctc = child.GetComponent<TransformComponent>();

            var refCom = new RefTransformComponent(ctc);

            refCom.AddRefTransform(ptc);

            child.RemoveComponent(ctc);
            child.AddComponent(refCom);
        }

        public void ManipulateInteractingTwoWays(Entity one, Entity two) {
            var ptc = one.GetComponent<TransformComponent>();
            var ctc = two.GetComponent<TransformComponent>();

            var refCom1 = new RefTransformComponent(ptc);
            var refCom2 = new RefTransformComponent(ctc);

            refCom1.AddRefTransform(refCom2);
            refCom2.AddRefTransform(refCom1);
            
            two.RemoveComponent(ctc);
            two.AddComponent(refCom1);

            one.RemoveComponent(ptc);
            one.AddComponent(refCom2);
        }
    }
}
