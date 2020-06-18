using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace D3DLab.Debugger.ECSDebug {
    
    public interface IVisualComponentItem {
        event PropertyChangedEventHandler PropertyChanged;
        ElementTag Guid { get; }
        string Name { get; }
        string Value { get; }

        IGraphicComponent GetOriginComponent();

        void Refresh(IGraphicComponent component);
        void MarkAsModified();
    }

    public interface IVisualTreeEntityItem {
        ElementTag Name { get; }
        ObservableCollection<IVisualComponentItem> Components { get; }
        void Add(IVisualComponentItem com);
        void Remove(IVisualComponentItem com);
        void Clear();
        bool TryRefresh(IGraphicComponent com);
        //void Refresh();


        //ICommand OpenShaderEditor { get; }
    }
}
