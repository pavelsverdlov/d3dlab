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

        public Matrix GetViewportMatrix() {
            return new Matrix(
                (float)(actualWidth / 2),
                0,
                0,
                0,
                0,
                (float)(-actualHeight / 2),
                0,
                0,
                0,
                0,
                1,
                0,
                (float)((actualWidth - 1) / 2),
                (float)((actualHeight - 1) / 2),
                0,
                1);
        }

        readonly double actualWidth;
        readonly double actualHeight;

        // public OrthographicCamera Camera { get; set; }

        public World(double actualWidth, double actualHeight) {
            this.actualHeight = actualHeight;
            this.actualWidth = actualWidth;
            WorldMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
            ViewMatrix = Matrix.Identity;
        }
    }
}