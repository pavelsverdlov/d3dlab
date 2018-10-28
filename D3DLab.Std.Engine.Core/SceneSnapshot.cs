using D3DLab.Std.Engine.Core.Input;
using D3DLab.Std.Engine.Core.Systems;
using System;
using System.Numerics;

namespace D3DLab.Std.Engine.Core {
    public class SceneSnapshot {
        public IContextState ContextState { get; }
        public IAppWindow Window { get; }

        public InputSnapshot Snapshot { get; }
        public TimeSpan FrameRateTime { get; }

        public CameraState Camera { get; private set; }
        public LightState[] Lights { get; private set; }

        public SceneSnapshot(IAppWindow win, IContextState state, InputSnapshot isnapshot, TimeSpan time) {
            ContextState = state;
            Snapshot = isnapshot;
            FrameRateTime = time;
            Window = win;
            Lights = new LightState[LightStructLayout.MaxCount];
        }

        public void UpdateCamera(CameraState state) {
            Camera = state;
        }
        public void UpdateLight(int index, LightState state) {
            Lights[index] = state;
        }
    }

    public interface IRenderState : IDisposable {
        IContextState ContextState { get; }
        float Ticks { get; }
        IAppWindow Window { get; }
        CameraState Camera { get; }
    }

    public enum LightTypes : uint {
        Undefined = 0,
        Ambient = 1,
        Point = 2,        
        Directional = 3
    }

   

    public class Lights {
        public void Add() {

        }
    }
}
