using D3DLab.ECS.Shaders;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Debugger {
    public interface IDockingManager {
        ObservableCollection<DockItem> Tabs { get; }

        void OpenSystemsTab(BaseNotify mv);
        void OpenSceneTab(BaseNotify mv);
        void OpenComponetsTab(BaseNotify mv);
        void OpenPropertiesTab(IEditingProperties properties);
        void OpenShaderEditerTab(IShadersContainer mv, IRenderUpdater updater);
    }
}
