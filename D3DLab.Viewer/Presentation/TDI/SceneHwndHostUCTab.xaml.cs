using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.Render;
using D3DLab.Viewer.D3D;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DLab.Viewer.Presentation.TDI {
    /// <summary>
    /// Interaction logic for SceneHwndHostUCTab.xaml
    /// </summary>
    public partial class SceneHwndHostUCTab : UserControl {
        class TestScene : D3DScene {
            WPFInputPublisher publisher;
            CurrentInputObserver input;
            bool isHandleCreated;
            readonly Host host;
            private readonly FrameworkElement overlay;

            public TestScene(Host host, FrameworkElement over, ContextStateProcessor context) : base(context) {
                this.host = host;
                this.overlay = over;
            }

            private void OnUnloaded(object sender, RoutedEventArgs e) {
                Dispose();
            }

            public void Init() {//object? sender, EventArgs e
                if (isHandleCreated) {

                }
                if (!host.IsVisible) {
                    return;
                }
                publisher = new WPFInputPublisher(overlay);
                input = new CurrentInputObserver(publisher);
                Window = new WpfSurface(host, overlay, input);
                engine = new RenderEngine(Window, Context, notify);
                engine.Run(notify);

                isHandleCreated = true;

                InitScene();

            }


            public override void Dispose() {
                input.Dispose();
                base.Dispose();
            }

        }

        TestScene scene;

        public SceneHwndHostUCTab() {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            hwndHost.SurfaceCreated += HwndHost_SurfaceCreated;
        }       

        private void HwndHost_SurfaceCreated() {
            if (scene != null) {
                scene.Dispose();
            }
            var notificator = new EngineNotificator();
            var context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));
            context.SwitchTo(0);

            scene = new TestScene(hwndHost, overlay, context);
            scene.Init();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            scene.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
           
        }
    }
}
