using D3DLab.Debugger.IDE;
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
    public class TextPoiterChanged {
        public TextPointer StartPointer;
        public TextPointer EndPointer;
        
        public TextRange GetRange() {
            return new TextRange(StartPointer, EndPointer);
        }
    }

    public class TextPoiterSelectionChanged {
        public readonly TextPointer CaretPointer;

        public TextPoiterSelectionChanged(TextPointer caretPointer) {
            CaretPointer = caretPointer;
        }

        public TextRange GetRange() {
            var range = new TextRange(CaretPointer.GetNextContextPosition(LogicalDirection.Backward), CaretPointer.GetNextContextPosition(LogicalDirection.Forward));
            range.Select(CaretPointer.GetNextContextPosition(LogicalDirection.Backward), CaretPointer.GetNextContextPosition(LogicalDirection.Forward));

            return range;
        }
    }

    public class ShaderTextBox : RichTextBox {
        public static readonly DependencyProperty ShaderDocumentProperty;
        public static readonly DependencyProperty WordSelectedCommandProperty;
        public static readonly DependencyProperty IntellisenseInvokedCommandProperty;

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
            IntellisenseInvokedCommandProperty = DependencyProperty.Register(
                    nameof(IntellisenseInvokedCommand),
                    typeof(ICommand),
                    typeof(RichTextBox));
        }

        

        public ShaderTextBox() {
        
            this.SelectionChanged += ShaderTextBox_SelectionChanged;
            this.TextChanged += ShaderTextBox_TextChanged;
            this.KeyDown += ShaderTextBox_KeyDown;
        }

        private void ShaderTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.OemPeriod) {
                var startPoiter = this.CaretPosition;
                var name = startPoiter.GetVariableName();
                var endPoiter = startPoiter.GetPositionAtOffset(1);
                var change = new TextRange(startPoiter, endPoiter);
                var text = change.Text;

                var pr = IntellisensePopup.Build(this, endPoiter, new TextPoiterChanged { EndPointer = startPoiter, StartPointer = endPoiter });
                IntellisenseInvokedCommand.RiseCommand(pr);
            }
        }

        private void ShaderTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            return;
            //this.CaretPosition
            var changes = e.Changes;
            if (!changes.Any()) {
                return;
            }
            var c = changes.Last();
            var startPoiter = Document.ContentStart.GetPositionAtOffset(c.Offset);
            var endPoiter = startPoiter.GetTextPointAt(c.AddedLength);
                //Document.ContentStart.GetPositionAtOffset(c.Offset + c.AddedLength);
            var change = new TextRange(startPoiter, endPoiter);
            var text = change.Text;

            switch (text) {
                case ".":
                    var left = startPoiter.GetNextContextPosition(LogicalDirection.Backward);
                    var pr = IntellisensePopup.Build(this, endPoiter, new TextPoiterChanged { EndPointer = startPoiter, StartPointer = left });
                    IntellisenseInvokedCommand.RiseCommand(pr);
                    break;
            }
        }

        private void ShaderTextBox_SelectionChanged(object sender, RoutedEventArgs e) {
            var pointer = this.CaretPosition;
            WordSelectedCommand.RiseCommand(new TextPoiterSelectionChanged(pointer));
        }


        public ICommand IntellisenseInvokedCommand {
            get { return (ICommand)GetValue(IntellisenseInvokedCommandProperty); }
            set { SetValue(IntellisenseInvokedCommandProperty, value); }
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
