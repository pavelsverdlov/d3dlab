﻿using System;
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
    /// Interaction logic for ShaderEditor.xaml
    /// </summary>
    public partial class ShaderEditor : Window {
        public ShaderEditor() {
            InitializeComponent();
            //DataContextChanged += ShaderEditor_DataContextChanged;
            //var dc = (ShaderEditorViewModel)DataContext;
            //rtb.Document = dc.ShaderDocument;
        }

        //private void ShaderEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        //    var dc = (ShaderEditorViewModel)e.NewValue ;
        //    rtb.Document = dc.ShaderDocument;
        //}
    }
}
