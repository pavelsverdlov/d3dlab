using D3DLab.ECS;
using D3DLab.ECS.Shaders;
using D3DLab.Debugger.ECSDebug;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation.TDI.ComponentList {
    public interface ITreeItemActions {
        void Removed(VisualTreeItem item);
    }
    public class VisualTreeItem : BaseNotify, IVisualTreeEntityItem {
        public ICommand RemoveItem { get; }

        public Visibility CanEditShader { get; private set; }
        public ElementTag Name { get { return entity.Tag; } }

        //  public System.ComponentModel.ICollectionView Components { get; set; }
        public ObservableCollection<IVisualComponentItem> Components { get; set; }

        //public ICommand OpenShaderEditor { get; }
        //public ICommand OpenPropertiesEditor { get; }

        readonly Dictionary<ElementTag, IVisualComponentItem> hash;

        readonly GraphicEntityDecorator entity;
        readonly ITreeItemActions actions;

        public VisualTreeItem(GraphicEntityDecorator entity, ITreeItemActions actions) {
            this.entity = entity;
            this.actions = actions;
            Components = new ObservableCollection<IVisualComponentItem>();
            hash = new Dictionary<ElementTag, IVisualComponentItem>();
            RemoveItem = new WpfActionCommand(OnSelfRemove);
        }

        private void OnSelfRemove() {
            entity.Remove();
            actions.Removed(this);
        }

        public void Add(IVisualComponentItem com) {
            Components.Add(com);
            hash.Add(com.Guid, com);

            CanEditShader = com.GetOriginComponent() is IShadersContainer ? Visibility.Visible : Visibility.Collapsed;
        }
        public void Clear() {
            Components.Clear();
            hash.Clear();
        }
        public void Remove(IVisualComponentItem com) {
            Components.Remove(com);
            hash.Remove(com.Guid);
            CanEditShader = !(com.GetOriginComponent() is IShadersContainer) ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool TryRefresh(IGraphicComponent com) {
            if (!hash.ContainsKey(com.Tag)) {
                return false;
            }
            hash[com.Tag].Refresh(com);
            return true;
        }

        //public void Refresh() {
        //    foreach (var i in Components) {

        //        i.Refresh();
        //    }
        //}
    }
}
