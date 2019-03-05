using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Systems;
using D3DLab.Wpf.Engine.App.Host;
using D3DLab.Wpf.Engine.App.Input;
using System;
using System.Numerics;
using System.Windows;
using D3DLab.Wpf.Engine.App.D3D.Techniques;

namespace D3DLab.Wpf.Engine.App {
    public class Scene {
        private readonly FormsHost host;
        private readonly FrameworkElement overlay;
        private readonly EngineNotificator notify;
        readonly CurrentInputObserver input;

        D3DEngine game;
        public GameWindow Window { get; private set; }

        public IContextState Context { get; }
        public event Action RenderStarted = () => { };

        public Scene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, EngineNotificator notify) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            this.Context = context;
            this.notify = notify;
            input = new CurrentInputObserver(Application.Current.MainWindow, new WPFInputPublisher(Application.Current.MainWindow));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            game.Dispose();
        }

        private void OnHandleCreated(WinFormsD3DControl win) {
            Window = new GameWindow(win, input);
            game = new D3DEngine(Window, Context, notify);
            game.Initialize += Initializing;
            game.Run(notify);

            // Initializing();

            RenderStarted();
        }

        void Initializing(SynchronizedGraphics device) {
            var cameraTag = new ElementTag("CameraEntity");
            {   //systems creating
                var smanager = Context.GetSystemManager();

                smanager.CreateSystem<InputSystem>();
                smanager.CreateSystem<D3DCameraSystem>();
                smanager.CreateSystem<LightsSystem>();
                smanager.CreateSystem<MovementSystem>();
                //smanager.CreateSystem<MovingOnHeightMapSystem>();
                smanager.CreateSystem<AnimationSystem>();
                smanager.CreateSystem<TerrainGeneratorSystem>();
                smanager
                    .CreateSystem<RenderSystem>()
                    .Init(device)
                    .CreateNested<SkyGradientColoringRenderTechnique>()
                    .CreateNested<SkyPlaneWithParallaxRenderTechnique>()
                    .CreateNested<TerrainRenderTechnique>()
                    .CreateNested<TriangleColoredVertexRenderTechnique>()
                    .CreateNested<LineVertexRenderTechnique>()
                    .CreateNested<SpherePointRenderStrategy>();

            }
            {//entities ordering 
                Context.EntityOrder
                       .RegisterOrder<RenderSystem>(cameraTag, 0)
                       .RegisterOrder<InputSystem>(cameraTag, 0);
            }
        }
    }
}
