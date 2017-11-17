using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Input.EventArgs;

namespace D3DLab.Core.Input {
    public class PointData {
        internal PointData(MouseButtons mouseButtons) {
            this.mouseButtons = mouseButtons;
        }

        private readonly MouseButtons mouseButtons;

        Point point;
        public Point Point {
            get { return point; }
            set {
                point = value;
                time = DateTime.Now;
            }
        }

        public DateTime time;
        public DateTime Time { get { return time; } }

        public MouseEventArgs ToMouseArgs(MouseButtons button = MouseButtons.None) {
            if (button == MouseButtons.None)
                button = mouseButtons;
            return new MouseEventArgs(button, 1, Point.X, Point.Y, 0);
        }

        public AllowMouseEventArgs ToMouseArgsAllow(MouseButtons button = MouseButtons.None) {
            if (button == MouseButtons.None)
                button = mouseButtons;
            return new AllowMouseEventArgs(button, 1, Point.X, Point.Y, 0);
        }
    }
    public class InputControllerStateContext {
        public InputControllerStateContext() {
            DownPointLeft = new PointData(MouseButtons.Left);
            DownPointRight = new PointData(MouseButtons.Right);
            DownPointMiddle = new PointData(MouseButtons.Middle);
            PrevMovePoint = new PointData(MouseButtons.None);
        }

        public PointData DownPointLeft { get; private set; }
        public PointData DownPointRight { get; private set; }
        public PointData DownPointMiddle { get; private set; }
        public PointData DownPointLast {
            get {
                if (DownPointLeft.Time > DownPointRight.Time && DownPointLeft.Time > DownPointMiddle.Time)
                    return DownPointLeft;
                else if (DownPointRight.Time > DownPointMiddle.Time)
                    return DownPointRight;
                else
                    return DownPointMiddle;
            }
        }

        public PointData PrevMovePoint { get; private set; }
    }
}
