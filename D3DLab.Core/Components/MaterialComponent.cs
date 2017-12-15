﻿using D3DLab.Core.Common;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public sealed class MaterialComponent : D3DComponent {
        public HelixToolkit.Wpf.SharpDX.PhongMaterial Material { get; set; }
        public HelixToolkit.Wpf.SharpDX.PhongMaterial BackMaterial { get; set; }
        public CullMode CullMaterial { get; set; }

        private HelixToolkit.Wpf.SharpDX.PhongMaterial normal;

        public void Setected() {
            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Red,
                SpecularColor = SharpDX.Color.Red,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };
            if (normal == null) {
                normal = Material;
            }

            Material = mat;
            BackMaterial = mat;
        }
        public void UnSetected() {
            Material = normal;
            BackMaterial = normal;
        }

        public override string ToString() {
            return $"MaterialComponent[{Material.DiffuseColor}]";
        }
    }
}
