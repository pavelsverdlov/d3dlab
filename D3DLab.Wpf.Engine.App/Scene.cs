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

            game.Run(notify);

           // Initializing();

            RenderStarted();
        }

        void Initializing() {
            var cameraTag = new ElementTag("CameraEntity");
            {   //default entities
                var em = Context.GetEntityManager();

                //em.CreateEntity(cameraTag)
                //    //.AddComponent(new OrthographicCameraComponent(Window.Width, Window.Height));
                //    .AddComponent(new PerspectiveCameraComponent());

                //em.CreateEntity(new ElementTag("AmbientLight"))
                //    .AddComponents(
                //            new LightComponent {
                //                Index = 0,
                //                Intensity = 0.2f,
                //                //Position = Vector3.Zero + Vector3.UnitZ * 1000,
                //                Type = LightTypes.Ambient
                //            }, 
                //            new ColorComponent { Color = V4Colors.White }
                //        );
                    

                //em.CreateEntity(new ElementTag("PointLight"))
                //    .AddComponents(
                //        new LightComponent {
                //            Index = 1,
                //            Intensity = 0.6f,
                //            //Position = new Vector3(2, 1, 0),
                //            Position = Vector3.Zero + Vector3.UnitZ * 1000,
                //            Type = LightTypes.Point
                //        },
                //        new ColorComponent { Color = new Vector4(1, 1, 1, 1) }
                //    );

                //em.CreateEntity(new ElementTag("DirectionLight"))
                //    .AddComponents(
                //        new LightComponent {
                //            Index = 2,
                //            Intensity = 0.2f,
                //            Direction = new Vector3(1, 4, 4).Normalize(),
                //            Type = LightTypes.Directional
                //        },
                //        new ColorComponent { Color = new Vector4(1, 1, 1, 1) }
                //    );


            }
            {//entities ordering 
                Context.EntityOrder
                       .RegisterOrder<RenderSystem>(cameraTag, 0)
                       .RegisterOrder<InputSystem>(cameraTag, 0);
            }
        }
    }
}
