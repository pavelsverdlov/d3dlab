using System.Windows.Controls;

namespace D3DLab.Viewer.Presentation {
    public interface ITabStateChanged {
        void Closed(UserControl control);
    }
}
