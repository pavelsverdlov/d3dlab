using System.Windows;
using System.Windows.Controls;

namespace D3DLab.Viewer.Presentation {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
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
            if(e.NewState == Syncfusion.Windows.Tools.Controls.DockState.Hidden) {
                if (DataContext is ITabStateChanged dc) {
                    dc.Closed((UserControl)cc.Content);
                }
            }
        }
    }

    
}
