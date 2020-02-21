using D3DLab.Viewer.Debugger;
using D3DLab.Viewer.Presentation;
using D3DLab.Viewer.Presentation.TDI.ComponentList;
using D3DLab.Viewer.Presentation.TDI.Editer;
using D3DLab.Viewer.Presentation.TDI.Scene;
using D3DLab.Viewer.Presentation.TDI.SystemList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using WPFLab;
using WPFLab.Messaging;

namespace D3DLab.Viewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class LabApp : LabApplication {
        public LabApp() {
            Syncfusion.Licensing.SyncfusionLicenseProvider
                .RegisterLicense(Viewer.Properties.Resources.ResourceManager.GetString("SyncfusionLicense"));

            //Syncfusion.SfSkinManager.SfSkinManager.ApplyStylesOnApplication = true;
        }

        protected override void ConfigureServices(IDependencyRegistrator registrator) {
            registrator
                .RegisterApplication(this)

                .RegisterAsSingleton<IMessenger, Messenger>()
                //.RegisterView<MainWindow_TEST>()


                .Register<IDockingManager, TabDockingManager>()
                .Register<SystemsViewModel>()
                .Register<SceneViewModel>()
                .Register<ComponetsViewModel>()
                .Register<MainWindowViewModel>()

                .RegisterView<MainWindow>()
                //


                .RegisterMvvm()
                ;
        }

        protected override void AppStartup(StartupEventArgs e, IDependencyResolver resolver) {
             resolver.ResolveView<MainWindow, MainWindowViewModel>().Show();
            //resolver.ResolveView<MainWindow_TEST, MainWindowViewModel>().Show();
        }

        
    }
}
