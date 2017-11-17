using D3DLab.Core.Render;

namespace D3DLab.Core.Components.Render {
    public abstract class RenderComponent : Component, IRenderComponent {
        public void Update(Graphics graphics) {
            OnUpdate(graphics);
            base.Update();
        }

        protected abstract void OnUpdate(Graphics graphics);

        public void Render(World world, Graphics graphics) {
            OnRender(world, graphics);
        }
        protected abstract void OnRender(World world, Graphics graphics);
    }
}