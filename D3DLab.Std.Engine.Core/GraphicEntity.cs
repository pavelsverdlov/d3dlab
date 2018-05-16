using System;
using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {
    public struct ElementTag : IEquatable<ElementTag> {
        readonly string tag;      
        public ElementTag(string tag) {
            this.tag = tag;
        }

        public override bool Equals(object obj) {
            return obj is ElementTag && Equals((ElementTag)obj);
        }
        public bool Equals(ElementTag other) {
            return tag == other.tag;
        }
        public override int GetHashCode() {
            var hashCode = -1778964077;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(tag);
            return hashCode;
        }
        public override string ToString() {
            return tag;
        }
        public static bool operator ==(ElementTag x, ElementTag y) {
            return x.Equals(y);
        }
        public static bool operator !=(ElementTag x, ElementTag y) {
            return !x.Equals(y);
        }
    }

    public sealed class GraphicEntity  {
        public ElementTag Tag { get; }
        readonly IComponentManager manager;
        readonly EntityOrderContainer order;

        public GraphicEntity(ElementTag tag, IComponentManager manager, EntityOrderContainer order) {
            this.order = order;
            this.manager = manager;
            Tag =tag;
           
        }

        public T GetComponent<T>() where T : IGraphicComponent {
            return manager.GetComponent<T>(Tag);
        }
        public IEnumerable<T> GetComponents<T>() where T : IGraphicComponent {
            return manager.GetComponents<T>(Tag);
        }

        public GraphicEntity AddComponent<T>(T component) where T : IGraphicComponent {
            manager.AddComponent(Tag, component);
            return this;
        }
        public void RemoveComponent(IGraphicComponent component) {
            manager.RemoveComponent(Tag, component);            
        }
        public bool Has<T>() where T : IGraphicComponent {
            return manager.Has<T>(Tag);
        }
        public IEnumerable<IGraphicComponent> GetComponents() {
            return manager.GetComponents(Tag);
        }

        public int GetOrderIndex<TSys>()
            where TSys : IComponentSystem  {
            return order.Get<TSys>(Tag);
        }
    }
    public class OrderSystemContainer : Dictionary<Type, int> {

    }
    public class EntityOrderContainer {
        readonly Dictionary<ElementTag, OrderSystemContainer> componentOrderIndex;
        readonly Dictionary<Type, int> systemsOrder;

        public EntityOrderContainer() {
            componentOrderIndex = new Dictionary<ElementTag, OrderSystemContainer>();
            systemsOrder = new Dictionary<Type, int>();
        }
        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag,int index) 
            where TSys : IComponentSystem{
            OrderSystemContainer ordering;
            if (!componentOrderIndex.TryGetValue(tag, out ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, index);
            IncrementSystemOrderIndex(t);

            return this;
        }

        public EntityOrderContainer RegisterOrder<TSys>(ElementTag tag)
           where TSys : IComponentSystem {
            OrderSystemContainer ordering;
            if (!componentOrderIndex.TryGetValue(tag, out ordering)) {
                ordering = new OrderSystemContainer();
                componentOrderIndex.Add(tag, ordering);
            }
            var t = typeof(TSys);

            ordering.Add(t, IncrementSystemOrderIndex(t));

            return this;
        }

        public int Get<TSys>(ElementTag tag)
            where TSys : IComponentSystem {
            if (!componentOrderIndex.ContainsKey(tag)) {
                return int.MaxValue;
            }
            return componentOrderIndex[tag][typeof(TSys)];
        }

        int IncrementSystemOrderIndex(Type t) {
            if (!systemsOrder.ContainsKey(t)) {
                systemsOrder.Add(t, 0);
            } else {
                systemsOrder[t] = systemsOrder[t] + 1;
            }
            return systemsOrder[t];
        }
    }
}
