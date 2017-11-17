using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public abstract class ComponentContainer : Component {
        private readonly List<Component> components;

        protected ComponentContainer(string tag) : base(tag) {
            components = new List<Component>();
        }

        protected ComponentContainer() {
            components = new List<Component>();
        }

        public Action<Action<T>> GetComponent<T>() where T : Component {
            var com = (T)components.FirstOrDefault(x => x is T);
            return com == null ? action => { } : (Action<Action<T>>)(action => action(com));
        }

        public IEnumerable<T> GetComponents<T>() {
            return components.OfType<T>();
        }

        public void AddComponent<T>(T component) where T : Component {
            var attachment = component as ICanAttach;
            if (attachment != null) {
                AttachToParent(attachment, this);
            }
            var dependent = component as IHaveDependency;
            if (dependent != null) {
                foreach (var com in components) {
                    DynamicDependentBy(dependent, com);
                }
            }
            components.Add(component);
            OnComponentAdded(component);
        }
        public static void DependentBy<T>(IHaveDependency com, T parent) where T : Component {
            var attachment = com as IDependentBy<T>;
            attachment?.OnAttach(parent);
        }
        protected static void DynamicDependentBy(IHaveDependency com, dynamic dependence) {
            DependentBy(com, dependence);
        }

        protected virtual void OnComponentAdded<T>(T component) { }
    }

    public abstract class ComponentInteractor {
        //https://habrahabr.ru/post/270825/
        public void Interact(ComponentContainer a, ComponentContainer b) {
        }
    }

}
