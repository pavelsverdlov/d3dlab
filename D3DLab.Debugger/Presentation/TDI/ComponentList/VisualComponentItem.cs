using D3DLab.ECS;
using D3DLab.Debugger.ECSDebug;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation.TDI.ComponentList {   
    interface IPropertyTabManager {
        void Open(IVisualComponentItem item);
    }
    class OpenPropertiesTabCommand : BaseWPFCommand<MouseButtonEventArgs> {
        readonly IVisualComponentItem item;
        readonly IPropertyTabManager tab;

        public OpenPropertiesTabCommand(IVisualComponentItem item, IPropertyTabManager docker) {
            this.item = item;
            this.tab = docker;
        }

        public override void Execute(MouseButtonEventArgs args) {
            if (args.ClickCount > 1) {
                tab.Open(item);
                //tab.OpenPropertiesTab(new EditingPropertiesComponentItem(item), updater);
            }
        }        
    }

    class VisualComponentItem : BaseNotify, IVisualComponentItem{
        public ICommand OpenPropertiesTab { get; }

        IGraphicComponent com;

        public VisualComponentItem(IGraphicComponent com, IPropertyTabManager propertyTabManager) {
            this.com = com;

            OpenPropertiesTab = new OpenPropertiesTabCommand(this, propertyTabManager);
        }

        public string Name { get { return com.GetType().Name; } }

        public ElementTag Guid { get { return com.Tag; } }

        public string Value { get; set; }

        public IGraphicComponent GetOriginComponent() {
            return com;
        }

        public void MarkAsModified() {
            //com.IsModified = true;
            //context.GetEntityOf(com).UpdateComponent(com);
        }

        public void Refresh(IGraphicComponent component) {
            com = component;
            SetPropertyChanged(nameof(Name));
        }

    }
}
