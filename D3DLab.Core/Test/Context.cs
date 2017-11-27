using D3DLab.Core.Entities;
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
        IComponent AddComponent(string tagEntity, IComponent com);
    }
    public interface ISystemManager {
        TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem;
        IEnumerable<IComponentSystem> GetSystems();
        void AddSystem(IComponentSystem system);
    }

    public interface IContext {
        Graphics Graphics { get; }
        World World { get; }
    }


    public class Context : IContext, ISystemManager, IEntityManager, IComponentManager {
        readonly List<Entity> entities = new List<Entity>();
        readonly Dictionary<string, List<IComponent>> components = new Dictionary<string, List<IComponent>>();
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();
                
        public Entity CreateEntity(string tag) {
            var en = new Entity(tag, this);
            entities.Add(en);
            d3DEngine.Notificator.NotifyAdd(en);
            components.Add(en.Tag, new List<IComponent>());
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
            d3DEngine.Notificator.NotifyAdd(sys);
            return sys;
        }
        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }
        public void AddSystem(IComponentSystem system) {
            systems.Add(system);
            d3DEngine.Notificator.NotifyAdd(system);
        }

        public IComponent AddComponent(string tagEntity, IComponent com) {
            components[tagEntity].Add(com);
            d3DEngine.Notificator.NotifyAdd(entities.Single(x=>x.Tag == tagEntity));
            return com;
        }

        readonly D3DEngine d3DEngine;

        public Context(D3DEngine d3DEngine) {
            this.d3DEngine = d3DEngine;
        }
    }

}
