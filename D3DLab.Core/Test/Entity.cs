using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public interface IEntity {
        void AddComponent<T>(T component) where T : IComponent;
        T GetComponent<T>() where T : IComponent;
    }

    public class Entity : IEntity {
        private readonly List<IComponent> components;

        public Entity() {
            components = new List<IComponent>();
        }
        public T GetComponent<T>() where T : IComponent {
            return components.OfType<T>().First();
        }

        public void AddComponent<T>(T component) where T : IComponent {
            components.Add(component);
        }
    }
}
