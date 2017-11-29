using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core;
using D3DLab.Core.Components;
using D3DLab.Core.Host;
using D3DLab.Core.Viewport;
using D3DLab.Core.Visual3D;
using D3DLab.Debugger.Windows;
using D3DLab.Core.Test;
using System.Windows.Input;
using D3DLab.Properties;

namespace D3DLab {
    public sealed class MainWindowViewModel {
        private D3DEngine engine;
        private readonly ViewportSubscriber subscriber;

        public VisualTreeviewerPopup VisualTreeviewer { get; set; }
        public ICommand LoadDuck { get; set; }
        public I3DobjectLoader Loader { get { return engine; } }


        public MainWindowViewModel() {
            LoadDuck = new Command(this);
            VisualTreeviewer = new VisualTreeviewerPopup();
            subscriber = new ViewportSubscriber(this);
        }

        public void Init(FormsHost host) {
            engine = new D3DEngine(host);
            engine.Notificator.Subscribe(subscriber);

            VisualTreeviewer.Show();
        }


        private class Command : ICommand {
            private MainWindowViewModel main;

            public Command(MainWindowViewModel mainWindowViewModel) {
                this.main = mainWindowViewModel;
            }

            public event EventHandler CanExecuteChanged = (s, r) => { };


            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                main.Loader.LoadObj(this.GetType().Assembly.GetManifestResourceStream("D3DLab.Resources.ducky.obj"));
            }
        }
    }

    public sealed class ViewportSubscriber : IViewportChangeSubscriber<Entity>, IViewportRenderSubscriber {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Add(Entity entity) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.ViewModel.Add(entity);
            }));
        }

        public void Render() {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.ViewModel.Refresh();
            }));
        }
    }
}
