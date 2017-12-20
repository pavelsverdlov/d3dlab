using D3DLab.Core.Entities;
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
        ElementTag Tag { get; }
        ElementTag EntityTag { get; set; }
    }

    public abstract class D3DComponent : ID3DComponent {
        public ElementTag Tag { get; }
        public ElementTag EntityTag { get; set; }

        protected D3DComponent() {
            Tag = new ElementTag(Guid.NewGuid().ToString());
        }

        public void Dispose() {

        }
    }
}
