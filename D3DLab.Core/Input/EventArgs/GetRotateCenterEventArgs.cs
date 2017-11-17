using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace D3DLab.Core.Input.EventArgs {
    public class GetRotateCenterEventArgs : System.EventArgs {
        public GetRotateCenterEventArgs(int x, int y) {
            X = x;
            Y = y;
            RotateCenter = null;
        }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Vector3? RotateCenter { get; set; }
        public bool Handled { get; set; }
    }
}
