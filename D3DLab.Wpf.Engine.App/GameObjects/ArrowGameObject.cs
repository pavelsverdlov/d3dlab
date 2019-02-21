using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Utilities;
using System;

namespace D3DLab.Wpf.Engine.App {
    public class ArrowGameObject {
        public ElementTag Tag { get; }

        public ArrowGameObject(ElementTag tag) {
            Tag = tag;
        }

        public static ArrowGameObject Build(IEntityManager manager, ArrowData data) {
            var geo = GeometryBuilder.BuildArrow(data);
            geo.Color = data.color;
            var en = manager
              .CreateEntity(data.tag)
              .AddComponents(
                geo,
                SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip(),
                new SDX.Engine.Components.D3DTransformComponent()
                );

            return new ArrowGameObject(en.Tag);
        }

        [Obsolete("Remove")]
        public static ElementTag BuildArrow(IEntityManager manager, ArrowData data) {
            var geo = GeometryBuilder.BuildArrow(data);
            return manager
               .CreateEntity(data.tag)
               .AddComponent(geo)
               .AddComponent(SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent.AsStrip())
               .AddComponent(new SDX.Engine.Components.D3DTransformComponent())
               .Tag;
        }
    }
}
