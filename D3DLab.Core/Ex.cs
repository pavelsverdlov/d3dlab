using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core {
    public static class Ex {
        public static SharpDX.Vector2 ToSharpDX(this System.Numerics.Vector2 v2) {
            return new SharpDX.Vector2(v2.X, v2.Y);
        }
        public static System.Numerics.Vector2 ToNumericsV2(this SharpDX.Vector2 v2) {
            return new System.Numerics.Vector2(v2.X, v2.Y);
        }
        public static System.Numerics.Vector2 ToNumericsV2(this System.Drawing.Point v2) {
            return new System.Numerics.Vector2(v2.X, v2.Y);
        }

        public static System.Drawing.Point ToDrawingPoint(this D3DLab.Std.Engine.Core.Input.WindowPoint p) {
            return new System.Drawing.Point(p.X, p.Y);
        }
        public static System.Numerics.Vector2 ToV2(this System.Windows.Point p) {
            return new System.Numerics.Vector2((float)p.X, (float)p.Y);
        }

        public static D3DLab.Std.Engine.Core.Input.WindowPoint ToWindowPoint(this System.Drawing.Point p) {
            return new D3DLab.Std.Engine.Core.Input.WindowPoint(p.X, p.Y);
        }
    }
}
