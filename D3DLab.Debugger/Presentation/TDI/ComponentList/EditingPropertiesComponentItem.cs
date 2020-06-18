using D3DLab.Debugger.ECSDebug;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation.TDI.ComponentList {
    class EditingPropertiesComponentItem : BaseNotify, IEditingProperties {
        readonly IVisualComponentItem item;

        public string Titile => item.Name;
        public object TargetObject { get; private set; }

        public EditingPropertiesComponentItem(IVisualComponentItem item) {
            this.item = item;
            TargetObject = item.GetOriginComponent();
        }
        public void MarkAsModified()  => item.MarkAsModified();

        public void Refresh() {
            TargetObject = null; //need to set null to force reread all properties of TargetObject if obj is refference type
            SetPropertyChanged(nameof(TargetObject));
            TargetObject = item.GetOriginComponent();
            SetPropertyChanged(nameof(TargetObject));

        }
    }
}
