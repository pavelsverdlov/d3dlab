using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {

    public interface IEntityContext {
        IEntity CreateEntity(string tag);
        T CreateEntity<T>(string tag) where T : IEntity;
        IEnumerable<IEntity> GetEntities();
        IEnumerable<T> GetEntities<T>() where T : IEntity;
        void AddEntity(IEntity entity);
    }

    public interface IContext : IEntityContext {
        InputStates InputState { get; }
    }

    public class Context : IContext {
        public IEntity CreateEntity(string tag) {
            return new Entity();
        }

        public T CreateEntity<T>(string tag) where T : IEntity{
            var entity = Activator.CreateInstance<T>();

            //set importand dependency to entity

            return entity;
        }

        public InputStates InputState => state;
        public IEnumerable<IEntity> GetEntities() {
            return entities;
        }
        public void AddEntity(IEntity entity) {
            entities.Add(entity);
        }
        public IEnumerable<T> GetEntities<T>() where T : IEntity {
            return entities.OfType<T>();
        }

        List<IEntity> entities = new List<IEntity>();
        List<IComponentSystem> systems = new List<IComponentSystem>();
        InputStates state = new InputStates();



        public void Test() {
            foreach (var sys in systems) {
                sys.Execute(this);
            }
        }

      
    }

}
