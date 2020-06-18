using System.Windows;
using System.Windows.Controls;

namespace D3DLab.Viewer.Presentation {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            formhost.Overlay = overlay;
            this.Drop += MainWindow_Drop;
        }

        void MainWindow_Drop(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DataContext is IDropFiles drop) {
                drop.Dropped(files);
            }
        }
    }

    
}
