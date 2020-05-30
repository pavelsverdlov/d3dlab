using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Components;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using D3DLab.Toolkit.Techniques.TriangleColored;
using D3DLab.Toolkit.Components;
using SharpDX.Direct3D11;
using D3DLab.Toolkit;
using System.IO;
using D3DLab.SDX.Engine.Components;
using D3DLab.Toolkit.Math3D;

namespace D3DLab.Render{
    public static class EntityBuilders {
        public static GraphicEntity BuildColored(IContextState context,
            List<Vector3> pos, List<int> indexes, Vector4 v4color, CullMode cullMode) {

            var material = MaterialColorComponent.Create(v4color);
            material.SpecularFactor = 400f;

            var manager = context.GetEntityManager();

            var geo = context.GetGeometryPool()
                .AddGeometry(new ImmutableGeometryData(
                    pos.AsReadOnly(),
                    pos.CalculateNormals(indexes).AsReadOnly(), 
                    indexes.AsReadOnly()));

            return manager.CreateEntity(new ElementTag("Geometry" + Guid.NewGuid()))
                .AddComponents(
                    TransformComponent.Identity(),
                    material,
                    geo,
                    RenderableComponent.AsTriangleColoredList(cullMode)
                );
        }

        public static GraphicEntity BuildTextured(IContextState context, 
            List<Vector3> pos, List<int> indexes, Vector2[] texCoor, FileInfo texture, CullMode cullMode) {
            if (texCoor == null) {
                throw new Exception("Geo must have TextCoor.");
            }
            var manager = context.GetEntityManager();
            var en = manager.CreateEntity(new ElementTag("TexturedGeometry" + Guid.NewGuid()));

            var material = MaterialColorComponent.Create();
            material.SetAlpha(1);


            var geo = context.GetGeometryPool()
              .AddGeometry(new ImmutableGeometryData(
                  pos.AsReadOnly(),
                  pos.CalculateNormals(indexes).AsReadOnly(),
                  indexes.AsReadOnly(),
                  texCoor.AsReadOnly()));

            en.AddComponents(
                    TransformComponent.Identity(),
                    geo,
                    material,
                    new D3DTexturedMaterialSamplerComponent(texture),
                    RenderableComponent.AsTriangleTexturedList(cullMode)
                );

            return en;
        }
    }
}
