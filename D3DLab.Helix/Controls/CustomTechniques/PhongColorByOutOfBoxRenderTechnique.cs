using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX.Model.Shader;
using HelixToolkit.Wpf.SharpDX.Render;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Controls.CustomTechniques {
    public sealed class PhongColorByOutOfBoxRenderTechnique : RenderTechnique {
        private BoundingBoxColoring box;
	    private Color _color;
        public PhongColorByOutOfBoxRenderTechnique() : base("RenderPhongColorByOutOfBox") { }

        public override void UpdateVariables(Effect variables) {
            if (box != null) {
                var param = variables.Variables().OutOfBoxParams;
                param.Max.Set(box.Box.Maximum);
                param.Min.Set(box.Box.Minimum);
	            param.Radius.Set(box.BlockRadius);
                param.Color.Set(box.InvalidPartsColor.ToColor4());
            }
            base.UpdateVariables(variables);
        }

        public void UpdateBox(BoundingBoxColoring box) {
            this.box = box;
        }
    }
}
