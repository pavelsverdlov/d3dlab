using D3DLab.ECS.Shaders;
using System;
using System.Collections.Generic;
using System.Text;
using WPFLab;

namespace D3DLab.Viewer.Debugger {
    public abstract class OpenShaderEditorCommand<T> : BaseWPFCommand<T> {
        readonly IDockingManager docker;
        readonly IRenderUpdater updater;

        public OpenShaderEditorCommand(IDockingManager docking, IRenderUpdater updater) {
            this.docker = docking;
            this.updater = updater;
        }

        protected abstract IShadersContainer Convert(T i);

        public override void Execute(T item) {
            var editable = Convert(item);
            docker.OpenShaderEditerTab(editable, updater);
        }
    }
}
