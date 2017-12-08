using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Render;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Test {
    public class Simple3DMovementSystem : IComponentSystem {
        private object lastPoint;


        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {

                var movable = entity.GetComponent<Simple3DMovable>();
                var transformComponent = entity.GetComponent<TransformComponent>();

                if (movable != null && transformComponent != null) {

                    var captured = entity.GetComponent<Simple3DMovementCaptured>();
                    var target = entity.GetComponent<TargetedComponent>();
                    var isLeftButton = ctx.World.MouseButtons == MouseButtons.Left;

                    if (!isLeftButton && target == null) {
                        entity.RemoveComponent(captured);
                    } else {
                        if (captured != null) {
                            var deltaMovement = Matrix.Identity;

                            var camera = emanager.GetEntities().Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null).First().GetComponent<CameraBuilder.CameraComponent>();
                            var rayCurrent = camera.UnProject(-ctx.World.MousePoint, ctx);
                            var rayPrev = camera.UnProject(-PreviousMouse, ctx);
                            var deltaVector = rayPrev.Position - rayCurrent.Position;
                            deltaMovement = Matrix.Translation(deltaVector);
                            transformComponent.Matrix *= deltaMovement;
                        } else if (target != null) {
                            entity.AddComponent(new Simple3DMovementCaptured());
                        }
                        PreviousMouse = ctx.World.MousePoint;
                    }
                }
            }
        }

        public Vector2 PreviousMouse { get; set; }
    }
}
