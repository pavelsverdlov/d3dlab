using D3DLab.Debugger;
using D3DLab.Viewer.Presentation;

using System.Windows;

using WPFLab;
using WPFLab.Messaging;

namespace D3DLab.Viewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class LabApp : LabApplication {
        public LabApp() {

        }

        protected override void ConfigureServices(IDependencyRegisterService registrator) {
            registrator
                .RegisterApplication(this)
                .RegisterAsSingleton<IMessenger, Messenger>()
                .RegisterUnhandledExceptionHandler()
                .RegisterAppLoger()

                .Register<MainWindowViewModel>()
                .RegisterView<MainWindow>()
                //               
                .RegisterMvvm()
                .RegisterDebugger()
                ;
        }

        protected override void AppStartup(StartupEventArgs e, IDependencyResolverService resolver) {
            resolver.UseUnhandledExceptionHandler();
            resolver.ResolveView<MainWindow, MainWindowViewModel>().Show();
        }

        protected override void AppExitp(ExitEventArgs e, IDependencyResolverService resolver) {
            resolver.RemoveUnhandledExceptionHandler();
        }
    }
}
