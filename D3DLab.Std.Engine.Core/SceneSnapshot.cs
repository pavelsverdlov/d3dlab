using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Input;
using System;

namespace D3DLab.Std.Engine.Core {
    public class SceneSnapshot {
        public IContextState State { get; }
        public InputSnapshot Snapshot { get; }
        public TimeSpan FrameRateTime { get; }
        public SceneSnapshot(IContextState state, InputSnapshot isnapshot, TimeSpan time) {
            State = state;
            Snapshot = isnapshot;
            FrameRateTime = time;
        }
    }
}
