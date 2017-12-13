using D3DLab.Core.Common;
using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public abstract class RenderTechniqueComponent : D3DComponent {
        public RenderTechnique RenderTechnique { get; set; }
    }
    public class PhongTechniqueRenderComponent : RenderTechniqueComponent {
        public PhongTechniqueRenderComponent() {
            RenderTechnique = Techniques.RenderPhong;
        }
        public override string ToString() {
            return $"[{RenderTechnique.Name}]";
        }
    }
}
