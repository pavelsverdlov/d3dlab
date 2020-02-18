using D3DLab.Viewer.Debugger;

namespace D3DLab.Viewer.Presentation.TDI.ComponentList {
    class EditingPropertiesComponentItem : IEditingProperties {
        readonly IVisualComponentItem item;

        public string Titile => item.Name;
        public object TargetObject => item.GetOriginComponent();

        public EditingPropertiesComponentItem(IVisualComponentItem item) {
            this.item = item;
        }

        public void MarkAsModified() {
            item.MarkAsModified();
        }
    }
}
