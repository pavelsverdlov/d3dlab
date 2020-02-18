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
    /// Interaction logic for SceneD3DImageUCTab.xaml
    /// </summary>
    public partial class SceneD3DImageUCTab : UserControl {
        class TestScene : D3DScene {
            WPFInputPublisher publisher;
            CurrentInputObserver input;
            readonly System.Windows.Controls.Image host;
            private readonly FrameworkElement overlay;

            public TestScene(System.Windows.Controls.Image host, FrameworkElement over, ContextStateProcessor context) : base(context) {

                // host.Unloaded += OnUnloaded;
                this.host = host;
                this.overlay = over;
            }

            private void OnUnloaded(object sender, RoutedEventArgs e) {
                Dispose();
            }

            public void Init() {
                publisher = new WPFInputPublisher(overlay);
                input = new CurrentInputObserver(publisher);
                Window = new WpfD3DImageSurface(host, overlay, input);
                engine = new RenderEngine(Window, Context, notify);
                engine.Run(notify);

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
        public SceneD3DImageUCTab() {
            InitializeComponent();

            this.Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            //var wic = new WicBitmapSource(@"C:\Storage\5lzrcejm7cert584fgerxwtj54o.jpeg");
            //image.Source = wic;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (scene != null) {
                scene.Dispose();
            }
            var notificator = new EngineNotificator();
            var context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));
            context.SwitchTo(0);

            scene = new TestScene(image, this, context);
            scene.Init();
        }
        private void OnUnloaded(object sender, RoutedEventArgs e) {
            //scene.Dispose();
        }

    }
}
