using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Input {
    public interface IControlWndMessageRiser {
        void WndProc(ref System.Windows.Forms.Message m);
    }

    public sealed class MouseKeyboardMessageFilter : System.Windows.Forms.IMessageFilter, IDisposable {
        enum WndMessages {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MOUSEWHEEL = 0x020A,
        }

        private readonly System.Windows.Forms.Control owner;
        public MouseKeyboardMessageFilter(System.Windows.Forms.Control owner) {
            this.owner = owner;
           // System.Windows.Forms.Application.AddMessageFilter(this);
            owner.Disposed += owner_Disposed;
        }

        public void Dispose() {
            System.Windows.Forms.Application.RemoveMessageFilter(this);
        }

        void owner_Disposed(object sender, System.EventArgs e) {
            Dispose();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        private bool IsChild(IntPtr hwnd1, IntPtr hwnd2) {
            IntPtr cur = hwnd1;
            while (cur != IntPtr.Zero) {
                if (cur == hwnd2)
                    return true;
                cur = GetParent(cur);
            }
            return false;
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m) {
            if (!owner.Visible || !owner.IsHandleCreated)
                return false;

            var msg = (WndMessages)m.Msg;
            if (msg != WndMessages.WM_MOUSEMOVE && msg != WndMessages.WM_MOUSEWHEEL && msg != WndMessages.WM_RBUTTONDOWN)
                return false;

            System.Drawing.Point p;
            var wnd = GetWindowUnderCursor(out p);

            bool focused = owner.Focused;
            if (msg == WndMessages.WM_MOUSEWHEEL || msg == WndMessages.WM_RBUTTONDOWN) {
                if (m.HWnd != wnd) {
                    SendMessage(wnd, m.Msg, m.WParam, m.LParam);
                    //SetFocus(wnd);
                    return true;
                }
                return false;
            }

            if (msg == WndMessages.WM_MOUSEMOVE && !focused && owner.Handle == wnd) {
                ((IControlWndMessageRiser)owner).WndProc(ref m);
                return true;
            }

            return false;
        }

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        public static IntPtr GetWindowUnderCursor(out System.Drawing.Point p) {
            if (!GetCursorPos(out p))
                return IntPtr.Zero;

            return WindowFromPoint(p);
        }
    }
}
