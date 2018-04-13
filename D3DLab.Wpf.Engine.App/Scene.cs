using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Input;
using D3DLab.Std.Engine.Entities;
using D3DLab.Std.Engine.Systems;
using D3DLab.Wpf.Engine.App.Host;
using D3DLab.Wpf.Engine.App.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Veldrid;

namespace D3DLab.Wpf.Engine.App {
    public sealed class GameWindow : IAppWindow {
        private WinFormsD3DControl win;
        readonly CurrentInputObserver input;

        public GameWindow(WinFormsD3DControl win, CurrentInputObserver input) {
            this.input = input;
            this.win = win;
            Width = (float)Application.Current.MainWindow.ActualWidth;
            Height = (float)Application.Current.MainWindow.ActualHeight;
        }

        public float Width { get; }

        public float Height { get; }

        public bool IsActive => true;

        public IntPtr Handle => win.Handle;

        public InputSnapshot GetShapshot() {
            return input.GetInputSnapshot();
        }
    }

    public class Scene {
        private readonly FormsHost host;
        private readonly FrameworkElement overlay;
        private readonly IEntityRenderNotify notify;
        readonly CurrentInputObserver input;

        private Game game;
        private GameWindow window;
        
        public ContextStateProcessor Context { get; }

        public Scene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, IEntityRenderNotify notify) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            this.Context = context;
            this.notify = notify;
            input = new CurrentInputObserver(Application.Current.MainWindow, new WPFInputPublisher(Application.Current.MainWindow));
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {

        }

        private void OnHandleCreated(WinFormsD3DControl win) {
            window = new GameWindow(win, input);
            game = new Game(window, Context);

            Task.Run(() => {

                Test();

                //systems initialization
                foreach (var sys in Context.GetSystemManager().GetSystems()) {
                    switch (sys) {
                        case IRenderSystemInit rsys:
                            rsys.Init(game.gd, game.factory, window);
                            break;
                    }
                }
                game.Run(notify);
            });
        }

        private void Test() {
            try {
                ShaderInfo[] shaders = {
                    new ShaderInfo{ Path= Path.Combine(AppContext.BaseDirectory, "Shaders", "Cube"),
                        Stage = ShaderStages.Vertex, EntryPoint = "VS" },
                    new ShaderInfo{ Path= Path.Combine(AppContext.BaseDirectory, "Shaders","Cube"),
                        Stage = ShaderStages.Fragment, EntryPoint = "FS"}
                };

                var mb = new Std.Engine.Helpers.MeshBulder();

                EngineInfoBuilder.Build(Context.GetEntityManager());

                var camera = Context.GetEntityManager()
                    .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                    .AddComponent(new CameraBuilder.CameraComponent(window.Width, window.Height));
                // .AddComponent(new CameraBuilder.GraphicsComponent());

                TextureInfo image = new TextureInfo {
                    Path = Path.Combine(AppContext.BaseDirectory, "Textures", "spnza_bricks_a_diff.png"),
                };

                var box = mb.BuildBox(Vector3.Zero, 1, 1, 1);
                var geo = Context.GetEntityManager()
                    .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                    .AddComponent(new TexturedGeometryGraphicsComponent(shaders, box, image) {
                        Matrix = Matrix4x4.CreateTranslation(Vector3.UnitX * 1)
                    });

                var geo1 = Context.GetEntityManager()
                    .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                    .AddComponent(new TexturedGeometryGraphicsComponent(shaders, mb.BuildSphere(Vector3.Zero, 1), image) {
                        Matrix = Matrix4x4.CreateTranslation(Vector3.UnitY * -1)
                    });

                var smanager = Context.GetSystemManager();
                smanager.CreateSystem<VeldridRenderSystem>();
                smanager.CreateSystem<InputSystem>();

                Context.EntityOrder
                    .RegisterOrder<VeldridRenderSystem>(camera.Tag, 0)
                    .RegisterOrder<InputSystem>(camera.Tag, 0)
                    .RegisterOrder<VeldridRenderSystem>(geo.Tag, 1)
                    .RegisterOrder<VeldridRenderSystem>(geo1.Tag, 2);

            } catch (Exception ex) {
                ex.ToString();
            }
        }

    }
}
