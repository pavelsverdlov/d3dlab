using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace D3DLab.Debugger {
    public abstract class BaseWPFCommand : ICommand {
        public event EventHandler CanExecuteChanged = (s, r) => { };
        public virtual bool CanExecute(object parameter) {
            return true;
        }

        public abstract void Execute(object parameter);
    }
    public abstract class BaseWPFCommand<TParam> : ICommand {
        public event EventHandler CanExecuteChanged = (s, r) => { };
        public virtual bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (parameter is TParam param) {
                Execute(param);
            }
        }
        public abstract void Execute(TParam parameter);
    }
    public static class WPFEx {
        public static void RiseCommand(this ICommand cmd, object param) {
            var temp = cmd;
            if(temp != null) {
                temp.Execute(param);
            }
        }

        public static void For(this int count, Action<int> action) {
            for(var i = 0; i < count; i++) {
                action(i);
            }
        }
    }
}
