using System;
using System.Collections.Generic;
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

namespace D3DLab.Debugger.Presentation.TDI.Property {
    /// <summary>
    /// Interaction logic for PropertyUCTab.xaml
    /// </summary>
    public partial class PropertyUCTab : UserControl {
        public PropertyUCTab() {
            InitializeComponent();
            //DataContextChanged += PropertyUCTab_DataContextChanged;
            //prgr.SelectedObjectChanged += Prgr_SelectedObjectChanged;
        }

        private void Prgr_SelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            
            Dispatcher.InvokeAsync(() => {

               // prgr.SelectedObject = new PropertyUCTab();
            });

        }

        private void PropertyUCTab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext is PropertyVeiwModel vm) {
               // vm.Test(prgr);
            }
            
        }
    }
}
