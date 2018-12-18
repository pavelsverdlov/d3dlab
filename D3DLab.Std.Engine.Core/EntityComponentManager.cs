using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.Std.Engine.Core {
    public sealed class EntityComponentManager : IEntityManager, IComponentManager {

        #region IEntityManager

        readonly Dictionary<ElementTag, GraphicEntity> entities = new Dictionary<ElementTag, GraphicEntity>();
        Func<GraphicEntity, bool> predicate = x => true;
        readonly IManagerChangeNotify notify;

        public GraphicEntity CreateEntity(ElementTag tag) {
            var en = new GraphicEntity(tag, this, this, orderContainer);

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

        public void AddComponents(ElementTag tagEntity, IEnumerable<IGraphicComponent> com) {
            comSynchronizer.AddRange((owner, inp) => {
                inp.EntityTag = tagEntity;
                owner.components[tagEntity].Add(inp);
            }, com);
        }
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
        public void RemoveComponents<T>(ElementTag tagEntity) where T : IGraphicComponent {
            foreach (var com in GetComponents<T>(tagEntity).ToList()) {
                comSynchronizer.Add((owner, c) => {
                    owner._RemoveComponent(tagEntity, c);
                }, com);
            }
        }

        public T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent {
            return components[tagEntity].OfType<T>().Single();
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
        public T GetOrCreateComponent<T>(ElementTag tagEntity, T newone) where T : IGraphicComponent {
            var any = GetComponents<T>(tagEntity);
            if (any.Any()) {
                return any.Single();
            }
            AddComponent(tagEntity, newone);
            return newone;
        }


        private void _RemoveComponent(ElementTag tagEntity, IGraphicComponent com) {
            components[tagEntity].Remove(com);
            com.Dispose();
        }

        #endregion

        public bool HasChanges {
            get {
                return entitySynchronizer.IsChanged || comSynchronizer.IsChanged || frameChanges;
            }
        }

        SynchronizationContext<EntityComponentManager, GraphicEntity> entitySynchronizer;
        SynchronizationContext<EntityComponentManager, IGraphicComponent> comSynchronizer;

        public EntityComponentManager(IManagerChangeNotify notify, EntityOrderContainer orderContainer) {
            this.orderContainer = orderContainer;
            this.notify = notify;
            entitySynchronizer = new SynchronizationContext<EntityComponentManager, GraphicEntity>(this);
            comSynchronizer = new SynchronizationContext<EntityComponentManager, IGraphicComponent>(this);
        }

        public void Synchronize(int theadId) {
            frameChanges = false;
            entitySynchronizer.Synchronize(theadId);
            comSynchronizer.Synchronize(theadId);
        }

        bool frameChanges;
        //not a good decision :(
        public void FrameSynchronize(int theadId) {
            if (!frameChanges) {
                frameChanges = HasChanges;
            }
            entitySynchronizer.Synchronize(theadId);
            comSynchronizer.Synchronize(theadId);            
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
