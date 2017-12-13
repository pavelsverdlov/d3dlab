using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Common;
using D3DLab.Core.Test;

namespace D3DLab.Core.Entities {
    public sealed class Entity  {
        public string Tag { get; }
        private readonly List<ID3DComponent> components;
        readonly IComponentManager manager;

        public Entity(string tag, IComponentManager manager) {
            this.manager = manager;
            Tag = tag;
            components = new List<ID3DComponent>();
        }
        public T GetComponent<T>() where T : ID3DComponent {
            return components.OfType<T>().FirstOrDefault();
        }

        public void AddComponent<T>(T component) where T : ID3DComponent {
            components.Add(manager.AddComponent(Tag, component));
        }
        public void RemoveComponent(ID3DComponent component) {
            components.Remove(component);
            manager.RemoveComponent(Tag, component);            
        }

        public IEnumerable<ID3DComponent> GetComponents() {
            return components.ToList();
        }
    }
}
