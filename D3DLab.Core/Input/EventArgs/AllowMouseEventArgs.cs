using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core.Input.EventArgs {
    public class AllowMouseEventArgs : MouseEventArgs {
        public AllowMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta) {
        }

        public bool Allow { get; set; }
    }
}
