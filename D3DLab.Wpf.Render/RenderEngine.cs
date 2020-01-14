using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace D3DLab.Render {
    
    using SDX.Engine;
    using SDX.Engine.Shader;
    using ECS;
    using ECS.Render;
    using ECS.Input;
    using ECS.Camera;

    public class SceneSnapshot : ISceneSnapshot {
        public IManagerChangeNotify Notifier { get; set; }
        public IAppWindow Window { get; set; }
        public InputSnapshot InputSnapshot { get; set; }
        public TimeSpan FrameRateTime { get; set; }

        //next data updated in certatin systems
        public ElementTag CurrentCameraTag { get; private set; }
        public CameraState Camera { get; private set; }
        public LightState[] Lights { get; set; }

        public void UpdateCamera(ElementTag tag, CameraState state) {
            CurrentCameraTag = tag;
            Camera = state;
        }

        public void UpdateLight(int index, LightState state) {
            Lights[index] = state;
        }
    }

    public class RenderEngine : DefaultEngine {
        //class IncludeResourse : IIncludeResourse {
        //    readonly string path;
        //    public string Key { get; }

        //    public IncludeResourse(string key, string path) {
        //        Key = key;
        //        this.path = path;
        //    }

        //    public Stream GetResourceStream() {
        //        return this.GetType().Assembly.GetManifestResourceStream(path);
        //    }
        //}
        class IncludeResourse : IIncludeResourse {
            readonly Assembly assembly;
            readonly string path;
            public string Key { get; }

            public IncludeResourse(Assembly assembly, string key, string path) {
                this.assembly = assembly;
                Key = key;
                this.path = path;
            }

            public Stream GetResourceStream() {
                return assembly.GetManifestResourceStream(path);
            }
        }

        /// <summary>
        /// DO NOT COPY OR PASS THIS OBJECT
        /// </summary>
        public readonly SynchronizedGraphics Graphics;

        public RenderEngine(ISDXSurface window, IContextState context, EngineNotificator notificator) : base(window, context, notificator) {
            Graphics = new SynchronizedGraphics(window);

            var includes = new System.Collections.Generic.Dictionary<string, IIncludeResourse>();

            //includes.Add("Common", new IncludeResourse("Common", "Zirkonzahn.Visualization.D3D.ComonShaders.Common.hlsl"));

            var sdx = typeof(GraphicsDevice).Assembly;
            //var app = typeof(Scene).Assembly;

            includes.Add("Game", new IncludeResourse(sdx, "Game", "D3DLab.SDX.Engine.Rendering.Shaders.Game.hlsl"));
            includes.Add("Light", new IncludeResourse(sdx, "Light", "D3DLab.SDX.Engine.Rendering.Shaders.Light.hlsl"));
            includes.Add("Math", new IncludeResourse(sdx, "Math", "D3DLab.SDX.Engine.Rendering.Shaders.Math.hlsl"));
           // includes.Add("Common", new IncludeResourse(app, "Common", "D3DLab.Wpf.Engine.App.D3D.Animation.Shaders.Common.hlsl"));

            Graphics.Device.Compilator.AddInclude(new D3DLab.SDX.Engine.Shader.D3DIncludeAdapter(includes));
        }

        protected override ISceneSnapshot CreateSceneSnapshot(InputSnapshot isnap, TimeSpan frameRateTime) => new SceneSnapshot {
            Window = Window,
            Notifier = Notificator,
            InputSnapshot = isnap,
            FrameRateTime = frameRateTime,
            Lights = new LightState[Std.Engine.Core.Shaders.LightStructBuffer.MaxCount],

        };

        protected override void Initializing() {
            var em = Context.GetEntityManager();
            var smanager = Context.GetSystemManager();


        }

        public override void Dispose() {
            base.Dispose();
            Graphics.Dispose();
        }

        protected override bool Synchronize() {
            var changed = Graphics.IsChanged;
            Graphics.Synchronize(System.Threading.Thread.CurrentThread.ManagedThreadId);
            return changed || base.Synchronize();
        }
    }
}
