using D3DLab.Core.Entities;
using SharpDX;

namespace D3DLab.Core.Render {
    public sealed class World {
        public Matrix WorldMatrix { get; set; }
        public int LightCount { get; set; }
        public CameraData Camera { get; set; }

        // public OrthographicCamera Camera { get; set; }

        public World() {
            WorldMatrix = Matrix.Identity;
        }
    }
}