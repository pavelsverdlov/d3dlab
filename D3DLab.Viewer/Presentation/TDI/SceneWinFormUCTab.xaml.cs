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
    /// Interaction logic for SceneWinFormUCTab.xaml
    /// </summary>
    public partial class SceneWinFormUCTab : System.Windows.Controls.UserControl {
        class TestScene : D3DScene {
            WPFInputPublisher publisher;
            CurrentInputObserver input;
            bool isHandleCreated;
            readonly FormsHost host;
            private readonly FrameworkElement overlay;

            public TestScene(FormsHost host, FrameworkElement over, ContextStateProcessor context) : base(context) {
               
                // host.Unloaded += OnUnloaded;
                this.host = host;
                this.overlay = over;
            }

            private void OnUnloaded(object sender, RoutedEventArgs e) {
                Dispose();
            }

            public void Init(System.Windows.Forms.Control control) {//object? sender, EventArgs e
                if (isHandleCreated) {

                }
                if (!host.IsVisible) {
                    return;
                }
                publisher = new WPFInputPublisher(overlay);
                input = new CurrentInputObserver(overlay, publisher);
                Window = new WFSurface(control, overlay, input);
                engine = new RenderEngine(Window, Context, notify);
                engine.Run(notify);

                isHandleCreated = true;

                InitScene();

            }


            public override void Dispose() {
                //host.Loaded -= OnHandleCreated;
                // host.Unloaded -= OnUnloaded;

                input.Dispose();

                base.Dispose();
            }

        }

        TestScene scene;

        public SceneWinFormUCTab() {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            host.SurfaceCreated += OnHandleCreated;
        }

        void OnHandleCreated(System.Windows.Forms.Control obj) {
            if (scene != null) {
                scene.Dispose();
            }
            var notificator = new EngineNotificator();
            var context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));
            context.SwitchTo(0);

            scene = new TestScene(host, overlay, context);
            scene.Init(obj);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            scene.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
           
        }
    }
}
