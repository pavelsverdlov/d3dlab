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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DLab.Debugger.Windows {
    public class ShaderTextBox : RichTextBox {
        public static readonly DependencyProperty ShaderDocumentProperty;
        public static readonly DependencyProperty WordSelectedCommandProperty;

        static ShaderTextBox() {
            // DefaultStyleKeyProperty.OverrideMetadata(typeof(ShaderTextBox), new FrameworkPropertyMetadata(typeof(ShaderTextBox)));

            ShaderDocumentProperty = DependencyProperty.Register(
                   nameof(ShaderDocument),
                   typeof(FlowDocument),
                   typeof(ShaderTextBox),
                   new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDocumentChanged)));

            WordSelectedCommandProperty = DependencyProperty.Register(
                    nameof(WordSelectedCommand),
                    typeof(ICommand),
                    typeof(RichTextBox));
        }


        public ShaderTextBox() {
            this.SelectionChanged += ShaderTextBox_SelectionChanged;
        }

        private void ShaderTextBox_SelectionChanged(object sender, RoutedEventArgs e) {
            var pointer = this.CaretPosition;
            var str = pointer.GetTextInRun(LogicalDirection.Backward) + pointer.GetTextInRun(LogicalDirection.Forward);
            var cmd = WordSelectedCommand;
            if (cmd != null) {
                cmd.Execute(str);
            }
        }



        public ICommand WordSelectedCommand {
            get { return (ICommand)GetValue(WordSelectedCommandProperty); }
            set { SetValue(WordSelectedCommandProperty, value); }
        }

        public FlowDocument ShaderDocument {
            get { return (FlowDocument)GetValue(ShaderDocumentProperty); }
            set { SetValue(ShaderDocumentProperty, value); }
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var box = (ShaderTextBox)d;
            box.Document = (FlowDocument)e.NewValue;
        }
    }
}
