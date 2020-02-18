using System;
using System.Collections.Generic;
using System.Text;
using WPFLab;

namespace D3DLab.Viewer.Debugger {
    public abstract class OpenPropertiesEditorCommand<T> : BaseWPFCommand<T> {
        readonly IDockingManager manager;

        public OpenPropertiesEditorCommand(IDockingManager manager) {
            this.manager = manager;
        }

        public override void Execute(T parameter) {
            manager.OpenPropertiesTab(Convert(parameter));
        }

        protected abstract IEditingProperties Convert(T item);
    }
}
