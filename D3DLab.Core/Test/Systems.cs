using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public interface IComponentSystem {
        void Execute(IContext ctx);
    }


    public class ReconstructionMovementSystem<TReconstruction> : IComponentSystem where TReconstruction : IEntity {

        public interface IPositionComponent : IComponent {
            Vector3 Position { get; set; }
        }
        public interface IReconstructionMovementMessageComponent : IMessageComponent { }

        public void Execute(IContext ctx) {

            foreach (var en in ctx.GetEntities<TReconstruction>()) {
                var position = en.GetComponent<IPositionComponent>();

                //calculate new position

                //...

                //notify all subscribers waiting IMovementMessageComponent
                IReconstructionMovementMessageComponent message = null;
                en.GetComponent<ISubscriberComponent<IReconstructionMovementMessageComponent>>().Push(message);

                //get all subscribers by certain message
                //var subscribers = ctx.GetEntities().Select(x => x.GetComponent<ISubscriberComponent<IReconstructionMovementMessageComponent>>(en.tag));

                //foreach (var s in subscribers) {
                //    s.Push(message);
                //}
            }
        }
    }
}
