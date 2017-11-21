using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public sealed class GeometryComponent : Component {
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Geometry { get; set; }
    }
    public sealed class MaterialComponent : Component {
        public HelixToolkit.Wpf.SharpDX.PhongMaterial Material { get; set; }
        public HelixToolkit.Wpf.SharpDX.PhongMaterial BackMaterial { get; set; }
        public CullMode CullMaterial { get; set; }
    }
    public abstract class RenderComponent : Component {
        public HelixToolkit.Wpf.SharpDX.RenderTechnique RenderTechnique { get; set; }
    }

    public sealed class VisualRenderComponent : RenderComponent {
    }
    public sealed class LightRenderComponent : RenderComponent {
        public Color4 Color { get; set; }
    }

    public sealed class TransformComponent : Component {
        public Matrix Matrix { get; set; }
    }
   
    public static class VisualModelBuilder {
        public static Entity Build(IEntityContext context) {
            var geo = new MeshGeometry3D(new Vector3[] { Vector3.Zero, Vector3.Zero + Vector3.UnitX * 100, Vector3.Zero + Vector3.UnitY * 100 }, new int[] { 0, 1, 2 }, null);

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Blue,
                SpecularColor = SharpDX.Color.Blue,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };

            var entity = context.CreateEntity("triangle");
            entity.AddComponent(new GeometryComponent() { Geometry = geo });
            entity.AddComponent(new MaterialComponent {
                Material = mat,
                BackMaterial = mat,
                CullMaterial = CullMode.Back
            });
            entity.AddComponent(new Test.VisualRenderComponent { RenderTechnique = HelixToolkit.Wpf.SharpDX.Techniques.RenderPhong });
            entity.AddComponent(new Test.TransformComponent { Matrix = SharpDX.Matrix.Identity });

            return entity;
        }
    }

    public static class LightBuilder {
        public static Entity BuildDirectionalLight(IEntityContext context) {
            var entity = context.CreateEntity("DirectionalLight");

            entity.AddComponent(new LightRenderComponent {
                RenderTechnique = Techniques.RenderPhong,
                Color = Color.White
            });

            return entity;
        }
    }
}
