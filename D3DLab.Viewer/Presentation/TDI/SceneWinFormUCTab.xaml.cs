using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Input;
using D3DLab.Render;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Presentation.TDI.Scene;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DLab.Viewer.Presentation.TDI {
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
                scene.SetSurfaceHost(host);
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
