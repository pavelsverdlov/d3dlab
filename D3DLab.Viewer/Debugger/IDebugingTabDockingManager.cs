using D3DLab.ECS.Shaders;
using D3DLab.Viewer.Debugger;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Debugger {
    public interface IDebugingTabDockingManager {
        void OpenPropertiesTab(IEditingProperties properties, IRenderUpdater updater);
        void OpenShaderEditerTab(IShadersContainer mv, IRenderUpdater updater);
    }
}
