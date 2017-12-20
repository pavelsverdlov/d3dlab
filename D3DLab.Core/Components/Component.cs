using D3DLab.Core.Render;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core.Common {
    public interface ID3DComponent : IDisposable {
        Guid Guid { get; }
        string EntityTag { get; set; }
    }

    public abstract class D3DComponent : ID3DComponent {
        public Guid Guid { get; }
        public string EntityTag { get; set; }

        protected D3DComponent() {
            Guid = Guid.NewGuid();
        }

        public void Dispose() {

        }
    }
}
