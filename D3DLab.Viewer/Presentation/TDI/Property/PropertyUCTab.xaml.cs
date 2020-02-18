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

namespace D3DLab.Viewer.Presentation.TDI.Property {
    /// <summary>
    /// Interaction logic for PropertyUCTab.xaml
    /// </summary>
    public partial class PropertyUCTab : UserControl {
        public PropertyUCTab() {
            InitializeComponent();
        }

        private void PropertyGrid_ValueChanged(object sender, Syncfusion.Windows.PropertyGrid.ValueChangedEventArgs args) {

        }
    }
}
