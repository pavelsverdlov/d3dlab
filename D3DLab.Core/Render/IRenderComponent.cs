namespace D3DLab.Core.Render {
    public interface IRenderComponent {
        void Update(Graphics graphics);
        void Render(World world, Graphics graphics);
    }
}