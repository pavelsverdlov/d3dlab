using D3DLab.Debugger.Infrastructure;
using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Debugger.Model {
    public class VisualProperty : IEntityComponent, System.ComponentModel.INotifyPropertyChanged {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private IGraphicComponent com;

        public VisualProperty(IGraphicComponent com) {
            this.com = com;
        }

        public string Name { get { return com.ToString(); } }

        public ElementTag Guid { get { return com.Tag; } }

        public string Value { get; set; }

        public IGraphicComponent GetOriginComponent() {
            return com;
        }

        public void Refresh() {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
