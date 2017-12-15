using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Components;
using D3DLab.Core.Render;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Test {
    public class MovementSystem : IComponentSystem {
        private object lastPoint;


        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {

                var movable = entity.GetComponent<ManipulationComponent>();

                if (movable == null) { continue; }

                var transformComponent = entity.GetComponent<TransformComponent>();
                var target = entity.GetComponent<TargetedComponent>();
                var captured = entity.GetComponent<Simple3DMovementCaptured>();

                if (target == null) { // moving was finished
                    entity.RemoveComponent(captured);
                    continue;
                }

                if (captured != null) {//already in moving
                    var deltaMovement = Matrix.Identity;

                    var camera = emanager.GetEntities().Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null).First().GetComponent<CameraBuilder.CameraComponent>();
                    var rayCurrent = camera.UnProject(-ctx.World.MousePoint, ctx);
                    var rayPrev = camera.UnProject(-PreviousMouse, ctx);

                    var matrix = movable.CalculateDelta(new ManipulationComponent.Input() { PrevRay = rayPrev, CurrentRay = rayCurrent });

                    transformComponent.AddDeltaMatrix(matrix);
                } else {//start moving
                    entity.AddComponent(new Simple3DMovementCaptured());
                }

                PreviousMouse = ctx.World.MousePoint;
            }
        }

        public Vector2 PreviousMouse { get; set; }
    }
}
