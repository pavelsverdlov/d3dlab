using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Std.Engine.Core.Shaders {
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GameStructBuffer {
        public const int RegisterResourceSlot = 0;

        public readonly Vector4 LookDirection;

        public readonly Matrix4x4 View;
        public readonly Matrix4x4 Projection;

      //  

        public GameStructBuffer(Matrix4x4 view, Matrix4x4 proj, Vector3 lookDirection) {
            View = view;
            Projection = proj;
            LookDirection = new Vector4(lookDirection,1);// Matrix4x4.Identity ;// lookDirection;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]//32 + /* Color vector4 16 */ 16 + ???
    //[StructLayout(LayoutKind.Sequential)]
    public struct LightStructBuffer {
        public const int MaxCount = 3;
        public const int RegisterResourceSlot = 1;

        [FieldOffset(0)]
        public readonly uint Type;
        [FieldOffset(4)] // + uint 4
        public readonly float Intensity;
        [FieldOffset(8)] // + float 4
        public readonly Vector3 Position;
        [FieldOffset(20)] // + vector3 12
        public readonly Vector3 Direction;
        [FieldOffset(32)] // + vector3 12
        public readonly Vector4 Color;

        public LightStructBuffer(LightTypes type, Vector3 pos, Vector3 dir, Vector4 color, float intensity) {
            Type = (uint)type;
            Intensity = intensity;
            Position = pos;
            Direction = dir;
            Color = color;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TransforStructBuffer {
        public const int RegisterResourceSlot = 2;
        public readonly Matrix4x4 World;
        public TransforStructBuffer(Matrix4x4 world) {
            World = world;
        }
    }

}
