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
    public interface ISystemManager {
        TSystem CreateSystem<TSystem>() where TSystem : IComponentSystem;
        IEnumerable<IComponentSystem> GetSystems();
        void AddSystem(IComponentSystem system);
    }

    public interface IContext {
        Graphics Graphics { get; }
        World World { get; }
    }


    public class Context : IContext, ISystemManager, IEntityManager {
        readonly List<Entity> entities = new List<Entity>();
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();
                
        public Entity CreateEntity(string tag) {
            var en = new Entity(tag);
            entities.Add(en);
            return en;
        }
        
        public Graphics Graphics { get; set; }
        public World World { get; set; }

        public IEnumerable<Entity> GetEntities() {
            return entities;
        }

        public TSystem CreateSystem<TSystem>() where TSystem : IComponentSystem {
            var sys = Activator.CreateInstance<TSystem>();
            systems.Add(sys);
            return sys;
        }

        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }
        public void AddSystem(IComponentSystem system) {
            systems.Add(system);
        }

        readonly D3DEngine d3DEngine;

        public Context(D3DEngine d3DEngine) {
            this.d3DEngine = d3DEngine;
        }
    }

}
