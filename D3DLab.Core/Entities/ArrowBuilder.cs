using D3DLab.Core.Components;
using D3DLab.Core.Test;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Entities {
    public static class ArrowBuilder {
        public static Entity Build(IEntityManager context) {
            var entity = context.CreateEntity("Arrow" + Guid.NewGuid());


            var arrow = new LineGeometryComponent {
                Start = Vector3.Zero,
                End = Vector3.Zero + Vector3.UnitZ * 50,
                Diameter = 1
            };
            arrow.RefreshGeometry();
            entity.AddComponent(arrow);

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Yellow,
                SpecularColor = SharpDX.Color.Yellow,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };
            entity.AddComponent(new MaterialComponent {
                Material = mat,
                BackMaterial = mat,
                CullMaterial = CullMode.Back
            });

            entity.AddComponent(new PhongTechniqueRenderComponent());
            entity.AddComponent(new TransformComponent());

            return entity;
        }
    }
}
