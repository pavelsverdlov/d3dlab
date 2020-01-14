using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Systems;
using D3DLab.Render;
using D3DLab.Render.GameObjects;
using D3DLab.Render.TriangleColored;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Utilities;
using D3DLab.Viewer.D3D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DLab.Viewer.Presentation.TDI {
    class UC  : System.Windows.Forms.UserControl { }
    /// <summary>
    /// Interaction logic for SceneUCTab.xaml
    /// </summary>
    public partial class SceneUCTab : UserControl {
        public SceneUCTab() {
            InitializeComponent();
            //host.MouseMove += Host_MouseMove;
            //host.PreviewMouseMove += Host_PreviewMouseMove;
            //host.Loaded += Host_Loaded;
            Loaded += SceneUCTab_Loaded;
            
        }

        private void SceneUCTab_Loaded(object sender, RoutedEventArgs e) {
            Test();
        }

        private void Host_PreviewMouseMove(object sender, MouseEventArgs e) {
            
        }

        private void Host_Loaded(object sender, RoutedEventArgs e) {
            //host.InternallWin.Loaded += InternallWin_Loaded;
            //host.InternallWin.PreviewMouseMove += InternallWin_PreviewMouseMove;
            //host.InternallWin.MouseMove += InternallWin_PreviewMouseMove;
        }

        private void InternallWin_Loaded(object sender, RoutedEventArgs e) {
            
        }

        private void InternallWin_PreviewMouseMove(object sender, MouseEventArgs e) {
            
        }

        private void Host_MouseMove(object sender, MouseEventArgs e) {
            
        }

        D3DScene scene;
        void Test() {
            var notificator = new EngineNotificator();
            var context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));

            context.SwitchTo(0);
           // scene = new TestScene(host, this, context);

        }
    }
    /*
    class TestScene : D3DScene {
        WPFInputPublisher publisher;
        CurrentInputObserver input;
        bool isHandleCreated;
        readonly IRenderSurface host;
        private readonly FrameworkElement over;

        public TestScene(IRenderSurface host, FrameworkElement over, ContextStateProcessor context) : base(context) {
            host.SurfaceCreated += OnHandleCreated;
           // host.Unloaded += OnUnloaded;
            this.host = host;
            this.over = over;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            Dispose();
        }

        private void OnHandleCreated() {//object? sender, EventArgs e
            if (isHandleCreated) {

            }
            if (!host.IsVisible) {
                return;
            }
            var overlay = over;// host.InternallWin;

            //publisher = new WinFormInputPublisher(win);
            publisher = new WPFInputPublisher(host.Window);
            input = new CurrentInputObserver(host.Window, publisher);
            //
            //publisher = new WPFInputPublisher(overlay);
            //return;
            //Window = new WFSurface(host.Child, overlay, input);
            Window = new WpfSurface(host.Host, host.Window, input);
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

    }*/

}
