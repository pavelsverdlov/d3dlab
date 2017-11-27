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
using System.Windows.Shapes;

namespace D3DLab.Debugger.Windows {
    /// <summary>
    /// Interaction logic for VisualTreeviewer.xaml
    /// </summary>
    public partial class VisualTreeviewer : Window {
        public VisualTreeviewer() {
            InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                ((VisualTreeviewerViewModel)DataContext).Execute(((TextBox)e.OriginalSource).Text);
            }
        }
    }
}
