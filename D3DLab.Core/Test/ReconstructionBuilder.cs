using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace D3DLab.Core.Test {
    public static class ReconstructionBuilder {/*
        public class PositionComponent : IPositionComponent {
            public Vector3 Position { get; set; }

            public void Dispose() {
                throw new NotImplementedException();
            }
        }
        public class SubscriberComponent : ISubscriberComponent<IReconstructionMovementMessageComponent> {
            public void Dispose() {}

            readonly IEnumerable<IEntity> entities;

            public SubscriberComponent(IEnumerable<IEntity> entities) {
                this.entities = entities;
            }

            public void Push(IReconstructionMovementMessageComponent message) {
                foreach(var en in entities) {
                    var position = en.GetComponent<IPositionComponent>();
                    
                    position.Position = message.NewPosition;
                    
                }
            }
        }
        public class ReconstructionEntity : IEntity {
            public void AddComponent<T>(T component) where T : IComponent {
                throw new NotImplementedException();
            }

            public T GetComponent<T>() where T : IComponent {
                throw new NotImplementedException();
            }
        }

        public static void Build(IEntityContext context) {
            var rec = context.CreateEntity<ReconstructionEntity>("ReconstructionEntity");
            var sup = context.CreateEntity("SupportEntity");
            var arrow = context.CreateEntity("ArrowEntity");

           // var sc = new SubscriberComponent(new[] { sup, arrow }.SelectMany(x=>x.GetComponent<>());

            //rec.AddComponent(sc);
            rec.AddComponent(new PositionComponent());

            sup.AddComponent(new PositionComponent());
            arrow.AddComponent(new PositionComponent());

            context.AddEntity(rec);
            context.AddEntity(sup);
            context.AddEntity(arrow);
        }
        */
    }
}
