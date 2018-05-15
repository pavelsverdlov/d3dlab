using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace D3DLab.Debugger.Infrastructure {
    public interface IVisualEntity {

    }
    public interface IEntityComponent {
        ElementTag Guid { get; }
        string Name { get; }
        string Value { get; }

        IGraphicComponent GetOriginComponent();

        void Refresh();
    }

    public interface IVisualTreeEntity {
        ElementTag Name { get; }
        ObservableCollection<IEntityComponent> Components { get; }
        void Add(IEntityComponent com);
        void Remove(IEntityComponent com);
        void Clear();
        bool TryRefresh(IGraphicComponent com);
        //void Refresh();


        ICommand OpenShaderEditor { get; }
    }
}
