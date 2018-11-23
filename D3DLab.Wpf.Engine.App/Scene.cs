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

        D3DEngine game;
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

    }
}
