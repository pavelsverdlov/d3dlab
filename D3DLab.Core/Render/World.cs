using D3DLab.Core.Entities;
using SharpDX;

namespace D3DLab.Core.Render {
    public sealed class World {
        public Matrix WorldMatrix { get; set; }
        public int LightCount { get; set; }

        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LookDirection { get; set; }



        // public OrthographicCamera Camera { get; set; }

        public World() {
            WorldMatrix = Matrix.Identity;
        }
    }
}