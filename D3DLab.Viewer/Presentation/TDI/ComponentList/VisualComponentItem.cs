using D3DLab.ECS;
using D3DLab.Viewer.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.TDI.ComponentList {    
    public class OpenPropertiesTabCommand : BaseWPFCommand<MouseButtonEventArgs> {
        readonly IVisualComponentItem item;
        readonly IDockingManager manager;

        public OpenPropertiesTabCommand(IVisualComponentItem item, IDockingManager manager) {
            this.item = item;
            this.manager = manager;
        }

        public override void Execute(MouseButtonEventArgs args) {
            if (args.ClickCount > 1) {
                manager.OpenPropertiesTab(new EditingPropertiesComponentItem(item));
            }
        }        
    }

    public class VisualComponentItem : BaseNotify, IVisualComponentItem{
        public ICommand OpenPropertiesTab { get; }

        readonly IGraphicComponent com;
        readonly IDockingManager updater;

        public VisualComponentItem(IGraphicComponent com, IDockingManager updater) {
            this.com = com;
            this.updater = updater;

            OpenPropertiesTab = new OpenPropertiesTabCommand(this, updater);
        }

        public string Name { get { return com.GetType().Name; } }

        public ElementTag Guid { get { return com.Tag; } }

        public string Value { get; set; }

        public IGraphicComponent GetOriginComponent() {
            return com;
        }

        public void MarkAsModified() {
            com.IsModified = true;
        }

        public void Refresh() {
            SetPropertyChanged(nameof(Name));
        }
    }
}
