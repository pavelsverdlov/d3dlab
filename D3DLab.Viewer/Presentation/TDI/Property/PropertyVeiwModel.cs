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
        readonly IRenderUpdater updater;

        public PropertyVeiwModel(IEditingProperties properties, IRenderUpdater updater) {
            this.properties = properties;
            this.updater = updater;
            ValueChanged = new WpfActionCommand<ValueChangedEventArgs>(OnValueChanged);
        }

        void OnValueChanged(ValueChangedEventArgs obj) {
            if (obj.Property.IsReadOnly) {
                return;
            }

            var property = obj.Property.PropertyInformation;
            var selected = obj.Property.SelectedObject;

            if (obj.NewValue != obj.OldValue) {
                property.SetValue(selected, obj.NewValue);
                properties.MarkAsModified();
                updater.Update();
            }
        }
    }
}
