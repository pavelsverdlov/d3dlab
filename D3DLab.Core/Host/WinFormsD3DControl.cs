using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Input;
using SharpDX;

namespace D3DLab.Core.Host {
    public sealed class WinFormsD3DControl : System.Windows.Forms.UserControl , IControlWndMessageRiser, IViewportControl {
       // public event Action HandleCreated = () => { };
        public WinFormsD3DControl() {
            BackColor = System.Drawing.Color.Black;
        }
        
        protected override void CreateHandle() {
            base.CreateHandle();
        }

        protected override void DestroyHandle() {

            base.DestroyHandle();
        }

        void IControlWndMessageRiser.WndProc(ref System.Windows.Forms.Message m) { 
            WndProc(ref m);
        }
        protected override System.Windows.Forms.CreateParams CreateParams {
            get {
                var prms = base.CreateParams;
                prms.Style |= 0x00010000; //WS_TABSTOP
                return prms;
            }
        }

        #region IViewportControl

        public Matrix GetViewportTransform() {
            return new Matrix(
                Width / 2f,
                0,
                0,
                0,
                0,
                -Height / 2f,
                0,
                0,
                0,
                0,
                1,
                0,
                Width / 2f,
                Height / 2f,
                0,
                1);
        }

        #endregion
    }
}
