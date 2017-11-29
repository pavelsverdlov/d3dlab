using D3DLab.Core.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public abstract class Component : IComponent {
        private readonly string tag;

        public Guid Guid => Guid.NewGuid();

        protected Component() {
            tag = this.GetType().Name;
        }
        protected Component(string tag) {
            this.tag = tag;
        }

        public virtual void Dispose() {}

        public void AttachTo<T>(T parent) where T : Component {
            var attachment = this as IAttachTo<T>;
            attachment?.OnAttach(parent);
        }
        protected void AttachToParent(ICanAttach com, dynamic parent) {
            com.AttachTo(parent);
        }

        /// <summary>
        /// this method should invoke before rendering 
        /// all getting data from scene can be handled in update because it is no effection in scene 
        /// all effect will be after render
        /// (World& world, Graphics& graphics)
        /// </summary>
        public virtual void Update() { }

        public override string ToString() {
            return $"[Component {tag}]";
        }
    }
}
