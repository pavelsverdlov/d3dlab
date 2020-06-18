using D3DLab.Debugger.ECSDebug;
using Syncfusion.Windows.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation.TDI.Property {
    class PropertyVeiwModel : BaseNotify {
        public ICommand ValueChanged { get; }
        public IEditingProperties EditingProperties { get; }

        readonly IRenderUpdater updater;
        

        public PropertyVeiwModel(IEditingProperties properties, IRenderUpdater updater) {
            this.EditingProperties = properties;
            this.updater = updater;
            ValueChanged = new WpfActionCommand<ValueChangedEventArgs>(OnValueChanged);
        }

        void OnValueChanged(ValueChangedEventArgs obj) {
            if (obj.Property.IsReadOnly) {
                return;
            }

            var property = obj.Property.PropertyInformation;
            var selected = obj.Property.SelectedObject;

            if (property.PropertyType.IsPrimitive) {
                switch (Type.GetTypeCode(property.PropertyType)) {
                    case TypeCode.Single:
                        if(!float.Equals(obj.NewValue, obj.OldValue)) {

                        }
                        break;
                }
            }

            if (!Equals(obj.NewValue, obj.OldValue)) {
                //property.SetValue(selected, obj.NewValue);
                EditingProperties.MarkAsModified();
                updater.Update();
            }
        }
    }
}
