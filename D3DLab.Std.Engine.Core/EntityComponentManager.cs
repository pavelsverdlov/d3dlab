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
            entities.Add(tag, en);
            notify.NotifyChange(en);
            components.Add(en.Tag, new List<IGraphicComponent>());
            return en;
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

        public IGraphicComponent AddComponent(ElementTag tagEntity, IGraphicComponent com) {
            com.EntityTag = tagEntity;
            components[tagEntity].Add(com);
            return com;
        }
        public void RemoveComponent(ElementTag tagEntity, IGraphicComponent com) {
            components[tagEntity].Remove(com);
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

        readonly EntityOrderContainer orderContainer;

        #endregion

        public EntityComponentManager(IManagerChangeNotify notify, EntityOrderContainer orderContainer) {
            this.orderContainer = orderContainer;
            this.notify = notify;
        }
    }
}
