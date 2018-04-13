using System.Threading.Tasks;
using System.Windows;

namespace D3DLab {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            Loaded -= OnLoaded;
            var dc = DataContext as MainWindowViewModel;
            dc.Init(Host, overlay);
        }
    }
}
