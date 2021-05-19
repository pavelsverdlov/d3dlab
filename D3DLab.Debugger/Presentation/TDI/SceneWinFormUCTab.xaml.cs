using D3DLab.Debugger.Presentation.TDI.Scene;

using System.Windows;

namespace D3DLab.Debugger.Presentation.TDI {
    /// <summary>
    /// Interaction logic for SceneWinFormUCTab.xaml
    /// </summary>
    public partial class SceneWinFormUCTab : System.Windows.Controls.UserControl {

        bool check;
        public SceneWinFormUCTab() {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += SceneWinFormUCTab_DataContextChanged;
        }

        private void SceneWinFormUCTab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext is SceneViewModel scene) {
                scene.SetSurfaceHost(host, this);
            }
        }

        void OnHandleCreated(System.Windows.Forms.Control obj) {
            if (!check) {
                check = true;
                return;
            }
            
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
           // scene.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
           
        }
    }
}
