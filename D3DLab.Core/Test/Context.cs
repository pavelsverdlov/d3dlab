using D3DLab.Core.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {

    public interface IEntityContext {
        Entity CreateEntity(string tag);
        IEnumerable<Entity> GetEntities();
    }
    public interface ISystemContext {
        TSystem CreateSystemy<TSystem>() where TSystem : IComponentSystem;
        IEnumerable<IComponentSystem> GetSystems();
    }

    public interface IContext : IEntityContext {
        InputStates InputState { get; }
        Graphics Graphics { get; }
        World World { get; }
    }


    public class Context : IContext, ISystemContext {
        readonly List<Entity> entities = new List<Entity>();
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();

        InputStates state = new InputStates();


        public Entity CreateEntity(string tag) {
            var en = new Entity(tag);
            entities.Add(en);
            return en;
        }

        public InputStates InputState => state;

        public Graphics Graphics { get; set; }
        public World World { get; set; }


        public IEnumerable<Entity> GetEntities() {
            return entities;
        }

        public TSystem CreateSystemy<TSystem>() where TSystem : IComponentSystem {
            var sys = Activator.CreateInstance<TSystem>();
            systems.Add(sys);
            return sys;
        }

        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }

        readonly D3DEngine d3DEngine;

        public Context(D3DEngine d3DEngine) {
            this.d3DEngine = d3DEngine;
        }
    }

}
