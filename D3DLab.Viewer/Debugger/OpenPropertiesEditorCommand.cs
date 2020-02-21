using System;
using System.Collections.Generic;
using System.Text;
using WPFLab;

namespace D3DLab.Viewer.Debugger {
    public abstract class OpenPropertiesEditorCommand<T> : BaseWPFCommand<T> {
        readonly IDockingManager docker;
        readonly IRenderUpdater updater;

        public OpenPropertiesEditorCommand(IDockingManager docker, IRenderUpdater updater) {
            this.docker = docker;
            this.updater = updater;
        }

        public override void Execute(T parameter) {
            docker.OpenPropertiesTab(Convert(parameter), updater);
        }

        protected abstract IEditingProperties Convert(T item);
    }
}
