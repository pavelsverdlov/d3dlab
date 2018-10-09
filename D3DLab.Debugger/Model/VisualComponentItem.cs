using D3DLab.Debugger.Infrastructure;
using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace D3DLab.Debugger.Model {
    public class VisualComponentItem : IVisualComponentItem, System.ComponentModel.INotifyPropertyChanged {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenShaderEditor { get; }
        public ICommand OpenPropertiesEditor { get; }
        


        private IGraphicComponent com;

        public VisualComponentItem(IGraphicComponent com) {
            this.com = com;

            OpenShaderEditor = new Windows.VisualTreeviewerViewModel.OpenShaderEditorCommand();
            OpenPropertiesEditor = new Windows.VisualTreeviewerViewModel.OpenPropertiesEditorCommand();
        
        }

        public string Name { get { return com.GetType().Name; } }

        public ElementTag Guid { get { return com.Tag; } }

        public string Value { get; set; }

        public IGraphicComponent GetOriginComponent() {
            return com;
        }

        public void Refresh() {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
