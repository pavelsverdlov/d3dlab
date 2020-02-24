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

            //if (obj.NewValue != obj.OldValue) {
            //    property.SetValue(selected, obj.NewValue);
            //    EditingProperties.MarkAsModified();
            //    updater.Update();
            //}
        }

        public void Test(Syncfusion.Windows.PropertyGrid.PropertyGrid grid) {
            ((ComponentList.EditingPropertiesComponentItem)EditingProperties).Test(grid);
        }

        //public void RefreshEditingComponent(IEnumerable<GraphicEntityDecorator> en) {
        //    foreach (var entity in en) {
        //        foreach (var com in entity.GetComponents()) {
        //          //  properties.TryUpdateInternalComponent(com);
        //        }
        //    }
        //}
    }
}
