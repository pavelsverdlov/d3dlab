namespace D3DLab.SDX.Engine {
    internal static class SDXMathematicsEx {
        internal static SharpDX.Vector3 ToSDXVector3(this System.Numerics.Vector3 v) {
            return new SharpDX.Vector3(v.X, v.Y, v.Z);
        }
    }

    public static class NumericsEx {
        public static System.Numerics.Vector4 ToNumericV4(this SharpDX.Vector4 v4) {
            return new System.Numerics.Vector4(v4.X, v4.Y, v4.Z, v4.W);
        }
        public static System.Numerics.Vector4 ToNumericV4(this SharpDX.Color color) {
            return new System.Numerics.Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            //return new System.Numerics.Vector4(color.R, color.G, color.B, color.A);
        }

        internal static System.Numerics.Vector3 ToNVector3(this SharpDX.Vector3 v) {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        class Color {
            private static byte ToByte(float component) {
                var value = (int)(component * 255.0f);
                return ToByte(value);
            }

            public static byte ToByte(int value) {
                return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
            }
        }
    }
}


