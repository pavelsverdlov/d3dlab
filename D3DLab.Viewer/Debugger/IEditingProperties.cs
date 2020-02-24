using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Viewer.Debugger {
    public interface IEditingProperties {
        string Titile { get; }
        object TargetObject { get; }
        void MarkAsModified();
        void Refresh();
       // void TryUpdateInternalComponent(ECS.IGraphicComponent component);
    }
}
