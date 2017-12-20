using D3DLab.Core.Render;
using SharpDX;
using D3DLab.Core.Common;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;

namespace D3DLab.Core.Test {
    public static class LightBuilder {

        public sealed class LightTechniqueRenderComponent : PhongTechniqueRenderComponent {
            public void Update(Graphics graphics, World world, Color4 color) {
                var variables = graphics.Variables(this.RenderTechnique);

                world.LightCount++;

                variables.LightCount.Set(world.LightCount);
                /// --- update lighting variables               
                variables.LightDir.Set(-world.LookDirection);
                variables.LightColor.Set(new[] { color });
                variables.LightType.Set(new[] { 1 /* (int)Light3D.Type.Directional*/ });

            }
        }

        public sealed class LightRenderComponent : D3DComponent {
            public Color4 Color { get; set; }
            public override string ToString() {
                return $"[Color:{Color}]";
            }
        }

        public static Entity BuildDirectionalLight(IEntityManager context) {
            var entity = context.CreateEntity(new ElementTag("DirectionalLight"));

            entity.AddComponent(new LightTechniqueRenderComponent());
            entity.AddComponent(new LightRenderComponent {
                Color = Color.White
            });

            return entity;
        }
    }


}
