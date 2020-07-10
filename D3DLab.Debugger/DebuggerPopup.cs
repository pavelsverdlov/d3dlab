using D3DLab.Debugger.Presentation;
using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Debugger {
    public interface IDebuggerMainViewModel {
        void SetContext(IContextState context, EngineNotificator notificator);
    }
    public class DebuggerPopup {
        readonly DebuggerWindow v;
        readonly IDebuggerMainViewModel vm;
        bool isShowed;
        public DebuggerPopup(DebuggerWindow v, IDebuggerMainViewModel vm) {
            this.v = v;
            this.vm = vm;
        }

        public void SetContext(IContextState context, EngineNotificator notificator) {
            vm.SetContext(context, notificator);
        }

        public void Show() {
            if (!isShowed) {
                v.DataContext = vm;
            }
            v.Show();
        }

        public void Dispose() {
            if (v != null) {
                v.DataContext = null;
                v.Close();
            }
        }
    }
}
