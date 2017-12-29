using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Common;
using D3DLab.Core.Context;
using D3DLab.Core.Test;

namespace D3DLab.Core.Entities {
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

    public sealed class Entity  {
        public ElementTag Tag { get; }
        readonly IComponentManager manager;

        public Entity(ElementTag tag, IComponentManager manager) {
            this.manager = manager;
            Tag =tag;
        }
        public T GetComponent<T>() where T : ID3DComponent {
            return manager.GetComponent<T>(Tag);
        }

        public void AddComponent<T>(T component) where T : ID3DComponent {
            manager.AddComponent(Tag, component);
        }
        public void RemoveComponent(ID3DComponent component) {
            manager.RemoveComponent(Tag, component);            
        }
        public bool Has<T>() where T : ID3DComponent {
            return manager.Has<T>(Tag);
        }
        public IEnumerable<ID3DComponent> GetComponents() {
            return manager.GetComponents(Tag);
        }
    }
}
