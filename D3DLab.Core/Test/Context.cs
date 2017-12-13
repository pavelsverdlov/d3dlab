using D3DLab.Core.Common;
using D3DLab.Core.Input;
using D3DLab.Core.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {

    public interface IEntityManager {
        Entity CreateEntity(string tag);
        IEnumerable<Entity> GetEntities();
    }
    public interface IComponentManager {
        ID3DComponent AddComponent(string tagEntity, ID3DComponent com);
        void RemoveComponent(string tagEntity, ID3DComponent com);
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

        readonly List<Entity> entities = new List<Entity>();
        readonly Dictionary<string, List<ID3DComponent>> components = new Dictionary<string, List<ID3DComponent>>();
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();
                
        public Entity CreateEntity(string tag) {
            var en = new Entity(tag, this);
            entities.Add(en);
            d3DEngine.Notificator.NotifyChange(en);
            components.Add(en.Tag, new List<ID3DComponent>());
            return en;
        }
        
        public Graphics Graphics { get; set; }
        public World World { get; set; }
        public IEnumerable<Entity> GetEntities() {
            return entities;
        }


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

        public ID3DComponent AddComponent(string tagEntity, ID3DComponent com) {
            components[tagEntity].Add(com);
            //3DEngine.Notificator.NotifyChange(entities.Single(x=>x.Tag == tagEntity));
            return com;
        }

        public void RemoveComponent(string tagEntity, ID3DComponent com) {
            components[tagEntity].Remove(com);
           // d3DEngine.Notificator.NotifyChange(entities.Single(x => x.Tag == tagEntity));
        }

      

        readonly D3DEngine d3DEngine;

        public Context(D3DEngine d3DEngine) {
            this.d3DEngine = d3DEngine;
            Events = new List<InputEventState>();
        }
    }

}
