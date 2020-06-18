using D3DLab.Debugger.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DLab.Debugger {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DebuggerWindow : Window {
        public DebuggerWindow() {
            InitializeComponent();
            this.Drop += MainWindow_Drop;
        }

        void MainWindow_Drop(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is IDropFiles drop) {
                drop.Dropped(files);
            }
        }

        private void _DockingManager_DockStateChanged(FrameworkElement sender, Syncfusion.Windows.Tools.Controls.DockStateEventArgs e) {
            var cc = (ContentControl)sender;
            if (e.NewState == Syncfusion.Windows.Tools.Controls.DockState.Hidden) {
                if (DataContext is ITabStateChanged dc) {
                    dc.Closed((UserControl)cc.Content);
                }
            }
        }
    }
}
