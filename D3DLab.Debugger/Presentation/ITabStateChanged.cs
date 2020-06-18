using System.Windows.Controls;

namespace D3DLab.Debugger.Presentation {
    public interface ITabStateChanged {
        void Closed(UserControl control);
    }
}
