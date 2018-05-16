using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.Std.Engine.Core {
    public sealed class EntityComponentManager : IEntityManager, IComponentManager {

        #region IEntityManager

        readonly Dictionary<ElementTag, GraphicEntity> entities = new Dictionary<ElementTag, GraphicEntity>();
        Func<GraphicEntity, bool> predicate = x => true;
        readonly IManagerChangeNotify notify;

        public GraphicEntity CreateEntity(ElementTag tag) {
            var en = new GraphicEntity(tag, this, orderContainer);

            entitySynchronizer.Add((owner, input) => {
                owner.entities.Add(tag, input);
                owner.notify.NotifyChange(input);
                owner.components.Add(input.Tag, new List<IGraphicComponent>());
            }, en);

            return en;
        }

        public void RemoveEntity(ElementTag elementTag) {
            entitySynchronizer.Add((owner, input) => {
                if (owner.entities.TryGetValue(elementTag, out GraphicEntity entity)) {
                    foreach (var component in owner.GetComponents(entity.Tag)) {
                        owner._RemoveComponent(entity.Tag, component);
                    }
                    owner.entities.Remove(entity.Tag);
                    owner.components.Remove(entity.Tag);
                }
            }, null);
        }

        public IEnumerable<GraphicEntity> GetEntities() {
            return entities.Values.Where(predicate);
        }
        public GraphicEntity GetEntity(ElementTag tag) {
            return entities[tag];
        }
        public void SetFilter(Func<GraphicEntity, bool> predicate) {
            this.predicate = predicate;
        }
        #endregion

        #region IComponentManager

        readonly Dictionary<ElementTag, List<IGraphicComponent>> components = new Dictionary<ElementTag, List<IGraphicComponent>>();
        readonly EntityOrderContainer orderContainer;

        public IGraphicComponent AddComponent(ElementTag tagEntity, IGraphicComponent com) {
            comSynchronizer.Add((owner, inp) => {
                inp.EntityTag = tagEntity;
                owner.components[tagEntity].Add(inp);
            }, com);
            return com;
        }
        public void RemoveComponent(ElementTag tagEntity, IGraphicComponent com) {
            comSynchronizer.Add((owner, inp) => {
                owner._RemoveComponent(tagEntity, inp);
            }, com);
        }
        public T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent {
            return components[tagEntity].OfType<T>().FirstOrDefault();
        }
        public IEnumerable<T> GetComponents<T>(ElementTag tagEntity) where T : IGraphicComponent {
            return components[tagEntity].OfType<T>();
        }
        public IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity) {
            return components[tagEntity].ToArray();
        }
        public bool Has<T>(ElementTag tag) where T : IGraphicComponent {
            return components[tag].Any(x => x is T);
        }


        private void _RemoveComponent(ElementTag tagEntity, IGraphicComponent com) {
            components[tagEntity].Remove(com);
            com.Dispose();
        }

        #endregion

        SynchronizationContext<EntityComponentManager, GraphicEntity> entitySynchronizer;
        SynchronizationContext<EntityComponentManager, IGraphicComponent> comSynchronizer;

        public EntityComponentManager(IManagerChangeNotify notify, EntityOrderContainer orderContainer) {
            this.orderContainer = orderContainer;
            this.notify = notify;
            entitySynchronizer = new SynchronizationContext<EntityComponentManager, GraphicEntity>(this);
            comSynchronizer = new SynchronizationContext<EntityComponentManager, IGraphicComponent>(this);
        }

        public void Synchronize() {
            entitySynchronizer.Synchronize();
            comSynchronizer.Synchronize();
        }
        public void Dispose() {
            foreach (var coms in components) {
                foreach (var com in coms.Value) {
                    com.Dispose();
                }
            }
            entitySynchronizer.Dispose();
            comSynchronizer.Dispose();
        }
    }
}
