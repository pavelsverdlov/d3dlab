using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public sealed class Entity  {
        public string Tag { get; }
        private readonly List<IComponent> components;
        readonly IComponentManager manager;

        public Entity(string tag, IComponentManager manager) {
            this.manager = manager;
            Tag = tag;
            components = new List<IComponent>();
        }
        public T GetComponent<T>() where T : IComponent {
            return components.OfType<T>().FirstOrDefault();
        }

        public void AddComponent<T>(T component) where T : IComponent {
            components.Add(manager.AddComponent(Tag, component));
        }
        public void RemoveComponent(IComponent component) {
            components.Remove(component);
            manager.RemoveComponent(Tag, component);            
        }

        public IEnumerable<IComponent> GetComponents() {
            return components.ToList();
        }
    }
}
