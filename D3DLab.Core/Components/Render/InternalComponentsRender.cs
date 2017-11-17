using D3DLab.Core.Render;

namespace D3DLab.Core.Components {
    //    public interface IContainerComponent {
    //        IEnumerable<IRenderable> Items { get; }
    //        void Add(IRenderable item);
    //        void Remove(IRenderable item);
    //    }

    public class InternalComponentsRender : Component, IRenderComponent, 
        IAttachTo<ComponentContainer>, IAttacmenthOf<ComponentContainer> {
        public void Update(Graphics graphics) {
            base.Update();
            foreach (var child in Parent.GetComponents<IRenderComponent>()) {
                if (child == this) { continue; }
                child.Update(graphics);
            }
        }
        public void Render(World world, Graphics graphics) {
            foreach (var child in Parent.GetComponents<IRenderComponent>()) {
                if (child == this) { continue; }
                child.Render(world,graphics);
            }
        }

        public InternalComponentsRender() : base() {}
        public ComponentContainer Parent { get; private set; }

        public void OnAttach(ComponentContainer parent) {
            this.Parent = parent;
        }
    }
}
