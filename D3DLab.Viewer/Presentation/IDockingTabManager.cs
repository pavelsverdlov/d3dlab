using D3DLab.Viewer.Debugger;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Controls;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation {
    interface IDockingTabManager : IDebugingTabDockingManager, IPropertyTabUpdater {
        ObservableCollection<DockItem> Tabs { get; }

        void OpenSystemsTab(BaseNotify mv);
        void OpenSceneTab(BaseNotify mv);
        void OpenComponetsTab(BaseNotify mv);

        void TabClosed(UserControl control);
    }
    interface IPropertyTabUpdater {
        void Update();
    }
}
