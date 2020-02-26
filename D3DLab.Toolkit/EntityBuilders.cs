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

namespace D3DLab.Render{
    public static class EntityBuilders {
        //public static GraphicEntity BuildMeshElement(this IEntityManager manager, List<Vector3> pos, List<int> indexes, Vector4 color) {
        //    return Build(manager, pos, indexes, pos.Select(x => color).ToList());
        //}

        //public static ElementTag BuildLineEntity(this IEntityManager manager, Vector3[] points) {
        //    var geo = new SimpleGeometryComponent() {
        //        Positions = points.ToImmutableArray(),
        //        Indices = ImmutableArray.Create<int>(),
        //        Color = new Vector4(0, 1, 0, 1)
        //    };
        //    return manager
        //       .CreateEntity(new ElementTag("Points" + Guid.NewGuid()))
        //       .AddComponent(geo)
        //       .AddComponent(new D3DLineVertexRenderComponent())
        //       .Tag;
        //}


        #region components 
        public static IRenderableComponent GetObjGroupsRender() {
            return D3DTriangleColoredVertexRenderComponent.AsTriangleListCullNone();
        }

        public static IRenderableComponent GetRenderAsTriangleColored() {
            return new D3DTriangleColoredVertexRenderComponent();
        }
        public static TransformComponent GetTransformation() {
            return TransformComponent.Identity();
        }

        #endregion

        //public static GraphicEntity Build(IEntityManager manager, List<Vector3> pos, List<int> indexes, List<Vector4> colors) {
        //    var geo = new GeometryComponent(pos, pos.CalculateNormals(indexes), indexes);
        //    return manager
        //        .CreateEntity(new ElementTag("Geometry" + Guid.NewGuid()))
        //        .AddComponent(geo)
        //        .AddComponent(TransformComponent.Identity())
        //        .AddComponent(new D3DTriangleColoredVertexRenderComponent(SharpDX.Direct3D11.CullMode.Front))//D3DTriangleColoredVertexRenderComponent.AsTriangleListCullNone()
        //        ;
        //}

        public static GraphicEntity BuildColored(IEntityManager manager,
            List<Vector3> pos, List<int> indexes, Vector4 v4color, CullMode cullMode) {

            var geo = new GeometryComponent(
               pos.ToArray(),
               pos.CalculateNormals(indexes).ToArray(),
               indexes.ToArray());

            var material = MaterialColorComponent.Create();
            material.Ambient = v4color;
            material.Diffuse = v4color;
            material.Specular = v4color;
            material.SpecularFactor = 400f;

            return manager.CreateEntity(new ElementTag("Geometry" + Guid.NewGuid()))
                .AddComponents(
                    TransformComponent.Identity(),
                    material,
                    geo,
                    TriangleRenderComponentCreator.RenderColoredAsTriangleList(cullMode)
                );
        }

        public static GraphicEntity BuildTextured(IEntityManager manager, 
            List<Vector3> pos, List<int> indexes, Vector2[] texCoor, FileInfo texture, CullMode cullMode) {
            if (texCoor == null) {
                throw new Exception("Geo must have TextCoor.");
            }
            var en = manager.CreateEntity(new ElementTag("TexturedGeometry" + Guid.NewGuid()));

            var geo = new GeometryComponent(
               pos.ToArray(),
               pos.CalculateNormals(indexes).ToArray(),
               indexes.ToArray(),
               texCoor.ToArray());

            var material = MaterialColorComponent.Create();
            material.SpecularFactor = 0;

            en.AddComponents(
                    TransformComponent.Identity(),
                    geo,
                    material,
                    new D3DTexturedMaterialSamplerComponent(texture),
                    TriangleRenderComponentCreator.RenderTexturedAsTriangleList(cullMode)
                );

            return en;
        }
    }
}
