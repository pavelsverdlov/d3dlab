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
        readonly IDockingManager docker;
        readonly IRenderUpdater updater;

        public OpenPropertiesTabCommand(IVisualComponentItem item, IDockingManager docker, IRenderUpdater updater) {
            this.item = item;
            this.docker = docker;
            this.updater = updater;
        }

        public override void Execute(MouseButtonEventArgs args) {
            if (args.ClickCount > 1) {
                docker.OpenPropertiesTab(new EditingPropertiesComponentItem(item), updater);
            }
        }        
    }

    public class VisualComponentItem : BaseNotify, IVisualComponentItem{
        public ICommand OpenPropertiesTab { get; }

        readonly IGraphicComponent com;
        readonly IDockingManager docker;

        public VisualComponentItem(IGraphicComponent com, IDockingManager docker,IRenderUpdater updater) {
            this.com = com;
            this.docker = docker;

            OpenPropertiesTab = new OpenPropertiesTabCommand(this, docker, updater);
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
