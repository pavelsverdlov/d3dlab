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
        public GraphicsDevice gd;
        public DisposeCollectorResourceFactory factory;
        public IAppWindow window;
        public CommandList Commands;
    }
  
    class GD {
        public static GraphicsDevice Create(IAppWindow window) {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(false, PixelFormat.R16_UNorm, true);
            return GraphicsDevice.CreateD3D11(options, window.Handle, (uint)window.Width, (uint)window.Height);
        }
    }

    public class Game {
        static double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static double oneFrameMilliseconds = (total / 60);
        private static double _desiredFrameLengthSeconds = 1.0 / 60.0;

        readonly IAppWindow window;
        public readonly GraphicsDevice gd;
        public readonly DisposeCollectorResourceFactory factory;

        public ContextStateProcessor Context { get; }

        public Game(IAppWindow window, ContextStateProcessor context) {
            Context = context;
            this.gd = GD.Create(window);//for test
            this.window = window;
            this.factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
        }

        public void Run(IEntityRenderNotify notify) {
            Task.Run(() => {
                Stopwatch speed = new Stopwatch();
                var engineInfoTag = Context.GetEntityManager().GetEntities()
                        .Single(x => x.GetComponent<EngineInfoBuilder.PerfomanceComponent>() != null).Tag;

                double millisec = oneFrameMilliseconds;
                while (window.IsActive) {
                    speed.Restart();

                    try {
                        var ishapshot = window.GetShapshot();
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
                        System.Threading.Thread.Sleep((int)(oneFrameMilliseconds - millisec));
                    }
                    speed.Stop();

                    var perfomance = Context.GetEntityManager().GetEntity(engineInfoTag)
                        .GetComponent<EngineInfoBuilder.PerfomanceComponent>();

                    perfomance.ElapsedMilliseconds = millisec;
                    perfomance.FPS = (int)(total / speed.ElapsedMilliseconds);

                    //Debug.WriteLine($"FPS {(int)(total / speed.ElapsedMilliseconds)} / {speed.ElapsedMilliseconds} ms");

                    notify.NotifyRender(Context.GetEntityManager().GetEntities().ToArray());
                }

                gd.WaitForIdle();
                factory.DisposeCollector.DisposeAll();
                gd.Dispose();
            });
        }
    }
}
