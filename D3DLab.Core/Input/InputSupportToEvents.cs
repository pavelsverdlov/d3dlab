using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core.Input {
    public interface IInputSupport {
        bool OnMouseDown(MouseEventArgs ea);
        bool OnMouseUp(MouseEventArgs ea);
        bool OnMouseWheel(MouseEventArgs ea);
        bool OnMouseMove(MouseEventArgs ea);
        bool OnMouseDoubleClick(MouseEventArgs ea);
        bool OnKeyDown(KeyEventArgs e);
        bool OnKeyUp(KeyEventArgs e);
    }
   
}
