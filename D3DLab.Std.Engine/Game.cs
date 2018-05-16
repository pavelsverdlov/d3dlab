using D3DLab.Std.Engine.Systems;
using D3DLab.Std.Engine.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using D3DLab.Std.Engine.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace D3DLab.Std.Engine {

    public class ViewportState {
        public Matrix4x4 WorldMatrix;
        /// <summary>
        /// Orthographic /Perspective 
        /// </summary>
        public Matrix4x4 ProjectionMatrix;
        /// <summary>
        /// the same as Camera
        /// </summary>
        public Matrix4x4 ViewMatrix;
    }

    public class RenderState {
        public ViewportState Viewport = new ViewportState();
        public float Ticks;
        public GraphicsDevice GrDevice;
        public DisposeCollectorResourceFactory Factory;
        public IAppWindow Window;
        public CommandList Commands;
    }
  
    class GD {
        public static GraphicsDevice Create(IAppWindow window) {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, true);
            return GraphicsDevice.CreateD3D11(options, window.Handle, (uint)window.Width, (uint)window.Height);
        }
    }

    public class Game {
        static readonly double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static readonly double oneFrameMilliseconds = (total / 60);
        //static double _desiredFrameLengthSeconds = 1.0 / 60.0;

        readonly IAppWindow window;
        public readonly GraphicsDevice gd;
        public readonly DisposeCollectorResourceFactory factory;
        IEntityRenderNotify notify;

        Task loopTask;
        readonly CancellationTokenSource tokensource;
        readonly CancellationToken token;

        public ContextStateProcessor Context { get; }

        public Game(IAppWindow window, ContextStateProcessor context) {
            Context = context;
            this.gd = GD.Create(window);//for test
            this.window = window;
            this.factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);

            tokensource = new CancellationTokenSource();
            token = tokensource.Token;
        }

        public void Run(IEntityRenderNotify notify) {
            this.notify = notify;
            loopTask = Task.Run((Action)Loop);
        }

        void Loop() {
            //synchronization
            Context.GetEntityManager().Synchronize();
            window.InputManager.Synchronize();
            //

            Stopwatch speed = new Stopwatch();
            var engineInfoTag = Context.GetEntityManager().GetEntities()
                    .Single(x => x.GetComponent<EngineInfoBuilder.PerfomanceComponent>() != null).Tag;

            double millisec = oneFrameMilliseconds;
            while (window.IsActive && !token.IsCancellationRequested) {
                speed.Restart();

                var eman = Context.GetEntityManager();
                //synchronization
                eman.Synchronize();
                window.InputManager.Synchronize();

                try {
                    var ishapshot = window.InputManager.GetInputSnapshot();
                    var snapshot = new SceneSnapshot(Context, ishapshot, TimeSpan.FromMilliseconds(millisec));
                    foreach (var sys in Context.GetSystemManager().GetSystems()) {
                        sys.Execute(snapshot);
                    }
                } catch (Exception ex) {
                    ex.ToString();
                    throw ex;
                }

                millisec = speed.ElapsedMilliseconds;

                if (millisec < oneFrameMilliseconds) {
                    Thread.Sleep((int)(oneFrameMilliseconds - millisec));
                }
                speed.Stop();

                millisec = speed.ElapsedMilliseconds;

                var perfomance = eman.GetEntity(engineInfoTag)
                    .GetComponent<EngineInfoBuilder.PerfomanceComponent>();

                perfomance.ElapsedMilliseconds = millisec;
                perfomance.FPS = (int)(total / millisec);

                //Debug.WriteLine($"FPS {(int)(total / speed.ElapsedMilliseconds)} / {speed.ElapsedMilliseconds} ms");

                notify.NotifyRender(Context.GetEntityManager().GetEntities().ToArray());
            }

            gd.WaitForIdle();
            factory.DisposeCollector.DisposeAll();
            gd.Dispose();

            window.InputManager.Dispose();
            Context.Dispose();
        }
    }
}
