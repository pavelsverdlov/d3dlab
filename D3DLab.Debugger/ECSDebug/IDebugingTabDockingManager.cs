using D3DLab.ECS.Shaders;
using D3DLab.Debugger.ECSDebug;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WPFLab.MVVM;

namespace D3DLab.Debugger.ECSDebug {
    public interface IDebugingTabDockingManager {
        void OpenPropertiesTab(IEditingProperties properties, IRenderUpdater updater);
        void OpenShaderEditerTab(IShadersContainer mv, IRenderUpdater updater);
    }
}
