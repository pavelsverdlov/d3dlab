using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
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

        }

        private void Test() {
            try {
                //IShaderInfo[] cubeShaders = {
                //    new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Cube")}-{ShaderStages.Vertex}.hlsl",
                //        Stage = ShaderStages.Vertex.ToString(), EntryPoint = "VS" },
                //    new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Cube")}-{ShaderStages.Fragment}.hlsl",
                //        Stage = ShaderStages.Fragment.ToString(), EntryPoint = "FS"}
                //};
                IShaderInfo[] lineShaders = {
                    new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "Line"),
                        $"line-{ShaderStages.Vertex}",
                        ShaderStages.Vertex.ToString(),
                        "VShaderLines"),
                        //new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line")}-{ShaderStages.Geometry}.hlsl",
                    //    Stage = ShaderStages.Geometry.ToString(), EntryPoint = "GShaderLines"},
                     new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "Line"),
                        $"line-{ShaderStages.Fragment}",
                        ShaderStages.Fragment.ToString(),
                        "PShaderLinesFade")
                };

                //if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line"))) {
                //    Directory.Delete(Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line"), true);
                //}

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
                //var geo = Context.GetEntityManager()
                //    .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                //    .AddComponent(new TexturedGeometryGraphicsComponent(cubeShaders, box, image) {
                //        Matrix = Matrix4x4.Identity//.CreateTranslation(Vector3.UnitX * 1)
                //    });

                var geo = Context.GetEntityManager()
                    .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                    .AddComponent(new LineGeometryRenderComponent(lineShaders, box
                    // new Std.Engine.Helpers.LineBuilder().Build(box.Positions.GetRange(0,3))
                    ));

                var smanager = Context.GetSystemManager();
                smanager.CreateSystem<VeldridRenderSystem>();
                smanager.CreateSystem<InputSystem>();

                Context.EntityOrder
                    .RegisterOrder<VeldridRenderSystem>(camera.Tag, 0)
                    .RegisterOrder<InputSystem>(camera.Tag, 0)
                    //.RegisterOrder<VeldridRenderSystem>(geo.Tag, 1);
                    .RegisterOrder<VeldridRenderSystem>(geo.Tag, 2);

            } catch (Exception ex) {
                ex.ToString();
            }
        }

    }
}
