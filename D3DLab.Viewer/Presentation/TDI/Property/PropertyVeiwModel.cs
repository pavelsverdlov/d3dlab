using D3DLab.Viewer.Debugger;
using Syncfusion.Windows.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.TDI.Property {
    class PropertyVeiwModel : BaseNotify {

        public object TargetObject => properties.TargetObject;
        public ICommand ValueChanged { get; }


        readonly IEditingProperties properties;
        public PropertyVeiwModel(IEditingProperties properties) {
            this.properties = properties;
            ValueChanged = new WpfActionCommand<ValueChangedEventArgs>(OnValueChanged);
        }

        void OnValueChanged(ValueChangedEventArgs obj) {
        
        }
    }
}
