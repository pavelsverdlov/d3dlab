using D3DLab.Debugger.Presentation;
using D3DLab.Debugger.Presentation.TDI.ComponentList;
using D3DLab.Debugger.Presentation.TDI.SystemList;
using System;
using System.Collections.Generic;
using System.Text;
using WPFLab;

namespace D3DLab.Debugger {
    public static class DependencyExtenstions {
        public static IDependencyRegisterService RegisterDebugger(this IDependencyRegisterService service) {
            Syncfusion.Licensing.SyncfusionLicenseProvider
               .RegisterLicense(Debugger.Properties.Resources.ResourceManager.GetString("SyncfusionLicense"));

            service.Register<IDockingTabManager, TabDockingManager>()
                .Register<SystemsViewModel>()
                .Register<ComponetsViewModel>()
                .Register<IDebuggerMainViewModel, DebuggerMainWindowViewModel>()
                .RegisterView<DebuggerWindow>()
                .Register<DebuggerPopup>();

            return service;
        }
    }
}
