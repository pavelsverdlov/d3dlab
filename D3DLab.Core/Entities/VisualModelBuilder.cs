using D3DLab.Core.Components;
using D3DLab.Core.Context;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace D3DLab.Core.Test {
    //builders

    public static class VisualModelBuilder {
        static TransformComponent TransformComponent;
        public static Entity Build(IEntityManager context, MeshGeometry3D geo, string tag) {
            //var geo = new MeshGeometry3D(new Vector3[] { Vector3.Zero, Vector3.Zero + Vector3.UnitX * 100, Vector3.Zero + Vector3.UnitY * 100 }, new int[] { 0, 1, 2 }, null);

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Blue,
                SpecularColor = SharpDX.Color.Blue,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };

            var random = new Random();


            var entity = context.CreateEntity(new ElementTag(tag));
            entity.AddComponent(new GeometryComponent() { Geometry = geo });
            entity.AddComponent(new MaterialComponent {
                Material = mat,
                BackMaterial = mat,
                CullMaterial = CullMode.Back
            });
            entity.AddComponent(new PhongTechniqueRenderComponent());
            //var tr = new Test.TransformComponent { Matrix = Matrix.Translation(new Vector3((float)random.Next(100, 500), (float)random.Next(100, 500), (float)random.Next(100, 500))) };
            var tr = new TransformComponent ();

            //if(TransformComponent != null) {
            //    entity.AddComponent(new RefTransformComponent(TransformComponent, Matrix.Translation(new Vector3((float)random.Next(100, 300), (float)random.Next(100, 300), (float)random.Next(100, 300)))));
            //} else {
            entity.AddComponent(tr);
            //}            
            // entity.AddComponent(TransformComponent);
            entity.AddComponent(new HitableComponent());
            entity.AddComponent(new ManipulationComponent());

            TransformComponent = tr;

            return entity;
        }
    }


}
