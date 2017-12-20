using D3DLab.Core.Common;
using D3DLab.Core.Entities;
using D3DLab.Core.Input;
using D3DLab.Core.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {

    public interface IEntityManager {
        Entity CreateEntity(ElementTag tag);
        IEnumerable<Entity> GetEntities();
        Entity GetEntity(ElementTag tag);
    }
    public interface IComponentManager {
        ID3DComponent AddComponent(ElementTag tagEntity, ID3DComponent com);
        void RemoveComponent(ElementTag tagEntity, ID3DComponent com);
        T GetComponent<T>(ElementTag tagEntity) where T : ID3DComponent;
        IEnumerable<ID3DComponent> GetComponents(ElementTag tagEntity);
    }
    public interface ISystemManager {
        TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem;
        IEnumerable<IComponentSystem> GetSystems();
        //void AddSystem(IComponentSystem system);
    }

    public interface IInputContext {
        List<InputEventState> Events { get; }
        void AddEvent(InputEventState inputEventState);
        void RemoveEvent(InputEventState inputEventState);
    }

    public interface IContext : IInputContext{
        Graphics Graphics { get; }
        World World { get; }
    }

    public enum AllInputStates {
        Idle = 0,
        Rotate = 1,
        Pan = 2,
        Zoom = 3,
        Target = 4,
        UnTarget = 5
    }

    public class Context : IContext, ISystemManager, IEntityManager, IComponentManager, IInputContext {

        #region IInputContext
        
        public List<InputEventState> Events { get; }
        public void AddEvent(InputEventState ev) {
            Events.Add(ev);
        }
        public void RemoveEvent(InputEventState ev) {
            Events.Remove(ev);
        }

        #endregion

        #region IEntityManager

        readonly Dictionary<ElementTag, Entity> entities = new Dictionary<ElementTag, Entity>();
        readonly Dictionary<ElementTag, List<ID3DComponent>> components = new Dictionary<ElementTag, List<ID3DComponent>>();
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();

        public Entity CreateEntity(ElementTag tag) {
            var en = new Entity(tag, this);
            entities.Add(tag, en);
            d3DEngine.Notificator.NotifyChange(en);
            components.Add(en.Tag, new List<ID3DComponent>());
            return en;
        }
        public IEnumerable<Entity> GetEntities() {
            return entities.Values;
        }
        public Entity GetEntity(ElementTag tag) {
            return entities[tag];
        }

        #endregion

        #region IComponentManager

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
            return components[tagEntity];
        }

        #endregion



        public Graphics Graphics { get; set; }
        public World World { get; set; }
      


        public TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem {
            var sys = Activator.CreateInstance<TSystem>();
            systems.Add(sys);
            d3DEngine.Notificator.NotifyChange(sys);
            return sys;
        }
        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }

       

        //public void AddSystem(IComponentSystem system) {
        //    systems.Add(system);
        //    d3DEngine.Notificator.NotifyChange(system);
        //}





        readonly D3DEngine d3DEngine;

        public Context(D3DEngine d3DEngine) {
            this.d3DEngine = d3DEngine;
            Events = new List<InputEventState>();
        }
    }

}
