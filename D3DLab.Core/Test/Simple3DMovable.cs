using SharpDX;

namespace D3DLab.Core.Test
{
    public class Simple3DMovable : Component
    {
    }
    public class MovementDataComponent : Component
    {
        public Vector2 MouseFrom { get; set; }
        public Vector2 MouseTo { get; set; }
    }
}



