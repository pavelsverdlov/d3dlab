using D3DLab.Core.Components;
using D3DLab.Core.Context;
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
        public static Entity Build(IEntityManager context, Vector3 axis, SharpDX.Color color) {
            var entity = context.CreateEntity(new ElementTag("Arrow" + Guid.NewGuid()));


            var arrow = new LineGeometryComponent {
                Start = Vector3.Zero - axis * 300,
                End = Vector3.Zero + axis * 300,
                Diameter = 10
            };
            arrow.RefreshGeometry();
            entity.AddComponent(arrow);

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = color,
                SpecularColor = color,
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

            entity.AddComponent(new HitableComponent());

            entity.AddComponent(new AxisManipulateComponent(axis));            

            return entity;
        }
    }
}
