namespace D3DLab.Core.Test {
    public class UpdateRenderTechniqueSystem : IComponentSystem {
        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {
                var tech = entity.GetComponent<RenderTechniqueComponent>();
                if(tech == null) {
                    continue;
                }
                switch (tech) {
                    case LightBuilder.LightTechniqueRenderComponent light:
                        var com = entity.GetComponent<LightBuilder.LightRenderComponent>();
                        light.Update(ctx.Graphics,ctx.World, com.Color);
                        break;
                    case CameraBuilder.CameraTechniqueRenderComponent camera:
                        var ccom = entity.GetComponent<CameraBuilder.CameraComponent>();
                        camera.Update(ctx.Graphics, ctx.World, ccom);
                        break;
                }
            }


        }

    }
}
