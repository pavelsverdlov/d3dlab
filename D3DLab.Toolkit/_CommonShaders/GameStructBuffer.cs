using D3DLab.ECS.Camera;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit._CommonShaders {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameStructBuffer {

        public readonly static int Size = Unsafe.SizeOf<GameStructBuffer>();

        public static GameStructBuffer FromCameraState(CameraState state) {
            return new GameStructBuffer(
                      Matrix4x4.Transpose(state.ViewMatrix),
                      Matrix4x4.Transpose(state.ProjectionMatrix),
                      state.LookDirection,
                      state.Position);
        }

        public const int RegisterResourceSlot = 0;

        public readonly Vector4 LookDirection;
        public readonly Vector4 CameraPosition;

        public readonly Matrix4x4 View;
        public readonly Matrix4x4 Projection;

        //  

        public GameStructBuffer(Matrix4x4 view, Matrix4x4 proj, Vector3 lookDirection, Vector3 pos) {
            View = view;
            Projection = proj;
            LookDirection = new Vector4(lookDirection, 0);// Matrix4x4.Identity ;// lookDirection;
            CameraPosition = new Vector4(pos, 1);
        }
    }
}
