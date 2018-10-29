using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Rendering {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameResourceBuffer {
        public const int RegisterResourceSlot = 0;

        public readonly Matrix4x4 World;
        public readonly Matrix4x4 View;
        public readonly Matrix4x4 Projection;
        //  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        // public Vector3[] vec;
        // public readonly LightBuffer Lights;
        public GameResourceBuffer(Matrix4x4 world, Matrix4x4 view, Matrix4x4 proj) {
            World = world;
            View = view;
            Projection = proj;
            //  Lights = new LightBuffer[1];
            //   Lights = new LightBuffer(1);
            //  vec = new[] { Vector3.Zero };
        }
    }
}
