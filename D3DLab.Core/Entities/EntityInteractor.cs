using D3DLab.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.Core.Entities {
    public class EntityInteractor {
        public void ManipulateInteractingOneWay(Entity parent, Entity child) {
            throw new NotImplementedException();
        }

        public void ManipulateInteractingTwoWays(IEnumerable<Entity> refs) {
            var tr = new List<TransformComponent>();
            var ttr = new List<TransformComponentTwoWays>();

            foreach (var _ref in refs) {
                var ownTCom = _ref.GetComponent<TransformComponent>();
                var ownTTCom = new TransformComponentTwoWays(ownTCom);

                tr.Add(ownTCom);
                ttr.Add(ownTTCom);

                _ref.RemoveComponent(ownTCom);
                _ref.AddComponent(ownTTCom);
            }
            //each child has ref to owner transform component 
            //thant means child change all refs transform as own 
            foreach (var t in ttr) {
                foreach (var tt in tr) {
                    t.AddRefTransform(tt);
                }
            }

        }
    }
}
