using D3DLab.Core.Common;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using D3DLab.Core.Input;
using D3DLab.Core.Render;
using D3DLab.Core.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Context {

    public sealed class InputManager : IInputManager {
        public List<InputEventState> Events { get; }
        public void AddEvent(InputEventState ev) {
            Events.Add(ev);
        }
        public void RemoveEvent(InputEventState ev) {
            Events.Remove(ev);
        }
        public InputManager() {
            Events = new List<InputEventState>();
        }
    }

    public sealed class SystemManager : ISystemManager {
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();
        public TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem {
            var sys = Activator.CreateInstance<TSystem>();
            systems.Add(sys);
            notify.NotifyChange(sys);
            return sys;
        }
        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }

        readonly IViewportChangeNotify notify;

        public SystemManager(IViewportChangeNotify notify) {
            this.notify = notify;
        }
    }
    public sealed class EntityComponentManager : IEntityManager, IComponentManager {
        
        #region IEntityManager

        readonly Dictionary<ElementTag, Entity> entities = new Dictionary<ElementTag, Entity>();
        Func<Entity, bool> predicate = x => true;
        readonly IViewportChangeNotify notify;

        public Entity CreateEntity(ElementTag tag) {
            var en = new Entity(tag, this);
            entities.Add(tag, en);
            notify.NotifyChange(en);
            components.Add(en.Tag, new List<ID3DComponent>());
            return en;
        }
        public IEnumerable<Entity> GetEntities() {
            return entities.Values.Where(predicate);
        }
        public Entity GetEntity(ElementTag tag) {
            return entities[tag];
        }
        public void SetFilter(Func<Entity, bool> predicate) {
            this.predicate = predicate;
        }
        #endregion

        #region IComponentManager
        readonly Dictionary<ElementTag, List<ID3DComponent>> components = new Dictionary<ElementTag, List<ID3DComponent>>();

        public ID3DComponent AddComponent(ElementTag tagEntity, ID3DComponent com) {
            com.EntityTag = tagEntity;
            components[tagEntity].Add(com);
            return com;
        }
        public void RemoveComponent(ElementTag tagEntity, ID3DComponent com) {
            components[tagEntity].Remove(com);
        }
        public T GetComponent<T>(ElementTag tagEntity) where T : ID3DComponent {
            return components[tagEntity].OfType<T>().FirstOrDefault();
        }
        public IEnumerable<ID3DComponent> GetComponents(ElementTag tagEntity) {
            return components[tagEntity].ToArray();
        }
        public bool Has<T>(ElementTag tag) where T : ID3DComponent {
            return components[tag].Any(x => x is T);
        }

        #endregion

        public EntityComponentManager(IViewportChangeNotify notify) {
            this.notify = notify;
        }
    }


    public class Viewport : IViewportContext {
        public Graphics Graphics { get; set; }
        public World World { get; set; }        
    }

}
