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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using D3DLab.Core;
using D3DLab.Core.Host;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Shader;
using SharpDX;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;

namespace D3DLab {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Loaded += OnLoaded;
//  
            Task.Run(() => {
//                D3DViewport v = new D3DViewport();
//
//                var visual = new VisualElement();
//                visual.Geometry = new GeometryModel(new[] {
//                    new Vector3(-0.5f, 0.5f, 0.0f),
//                    new Vector3(0.5f, 0.5f, 0.0f),
//                    new Vector3(0.0f, -0.5f, 0.0f)
//                }, new int[0], new Color4[] {
//                     SharpDX.Color.Red,
//                    SharpDX.Color.Green,
//                     SharpDX.Color.Blue
//                });
//                v.Children.Add(visual);
//
//                v.Run();
                
            });
        }
        
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            Loaded -= OnLoaded;
            var dc = DataContext as MainWindowViewModel;
            dc.Init(Host);
        }

//        private void UIElement_OnLoaded(object sender, RoutedEventArgs e) {
//            var host = (FormsHost)sender;
//            var v = host.Viewport;

//            var visual = new Visual3DElement();
//            visual.Geometry = new GeometryModel(new[] {
//                                            new Vector3(-0.5f, 0.5f, 0.0f),
//                                            new Vector3(0.5f, 0.5f, 0.0f),
//                                            new Vector3(0.0f, -0.5f, 0.0f),
//
//                                            new Vector3(-0.2f, 0.2f, 0.0f),
//                                            new Vector3(0.2f, 0.2f, 0.0f),
//                                            new Vector3(0.0f, -0.2f, 0.0f)
//                                        }, new[]{
//                                            0,1,2,
//                                            3,4,5
//                                        }, new Color4[] {
//                                             SharpDX.Color.Red,
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Blue,
//
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Green
//                                        });
//            visual.Rotation = Vector3.Zero;
//            v.Children.Add(visual);

//        }
    }
}
