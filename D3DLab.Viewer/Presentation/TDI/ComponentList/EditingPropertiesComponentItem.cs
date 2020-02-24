using D3DLab.Viewer.Debugger;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.TDI.ComponentList {
    struct Test1 {
        public string Y { get; set; }
    }
    class EditingPropertiesComponentItem : BaseNotify, IEditingProperties {
        readonly IVisualComponentItem item;

        public string Titile => item.Name;
        public object TargetObject => obj;
        
        Syncfusion.Windows.PropertyGrid.PropertyGrid grid;
        object obj;
        public EditingPropertiesComponentItem(IVisualComponentItem item) {
            this.item = item;
            obj = item.GetOriginComponent();
        }
        public void MarkAsModified()  => item.MarkAsModified();

        public void Refresh() {
            obj = null; //need to set null to force reread all properties of TargetObject if obj is refference type
            SetPropertyChanged(nameof(TargetObject));
            obj = item.GetOriginComponent();
            SetPropertyChanged(nameof(TargetObject));

        }
    }
}
