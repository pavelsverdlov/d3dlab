using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Components {
    public readonly struct PerfomanceComponent : IGraphicComponent {
        

        public static PerfomanceComponent Create(double millisec, int fps) {
            return new PerfomanceComponent(millisec,fps) {
            };
        }

        public ElementTag Tag { get;  }
        public bool IsModified { get;  }
        public bool IsValid { get; }
        public bool IsDisposed { get;  }

        public double ElapsedMilliseconds { get; }
        public double FPS { get; }
        PerfomanceComponent(double elapsedMilliseconds, double fPS) : this() {
            ElapsedMilliseconds = elapsedMilliseconds;
            FPS = fPS;
        }
        public override string ToString() {
            return $"Perfomance[ElapsedMilliseconds:{ElapsedMilliseconds} FPS:{FPS}]";
        }

        public void Dispose() {
        }
    }
}
