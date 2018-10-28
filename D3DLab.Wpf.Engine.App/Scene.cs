using D3DLab.SDX.Engine;
using D3DLab.Std.Engine.Core;
using D3DLab.Wpf.Engine.App.Host;
using D3DLab.Wpf.Engine.App.Input;
using System;
using System.Numerics;
using System.Windows;

namespace D3DLab.Wpf.Engine.App {
    public class Scene {
        private readonly FormsHost host;
        private readonly FrameworkElement overlay;
        private readonly IEntityRenderNotify notify;
        readonly CurrentInputObserver input;

        private D3DEngine game;
        public GameWindow Window { get; private set; }

        public IContextState Context { get; }
        public event Action RenderStarted = () => { };

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
            game.Dispose();
        }
        
        private void OnHandleCreated(WinFormsD3DControl win) {
            Window = new GameWindow(win, input);
            game = new D3DEngine(Window, Context);

            game.Run(notify);
            RenderStarted();
        }

        private void Test() {
            /*
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
            */
        }

    }
}
