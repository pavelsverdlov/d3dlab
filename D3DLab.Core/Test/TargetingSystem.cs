using D3DLab.Core.Components;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX;
using D3DLab.Core.Input;

namespace D3DLab.Core.Test {
    public sealed class TargetingSystem : IComponentSystem {

        public void Execute(IEntityManager emanager, IContext ctx) {
            var input = ctx;
            //return;
            foreach (var entity in emanager.GetEntities()) {
                var hitable = entity.GetComponent<HitableComponent>();                
                if (hitable == null || !input.Events.Any()) {
                    continue;
                }
                var events = ctx.Events.ToArray();
                foreach (var ev in events) {
                    Handle(ev, emanager, entity, ctx);
                }                
            }
        }

        private void Handle(InputEventState ev, IEntityManager emanager, Entity entity, IContext ctx) {
            switch (ev.Type) {
                case AllInputStates.Target:
                    var geo = entity.GetComponent<GeometryComponent>();
                    var camera = emanager.GetEntities().Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null).First().GetComponent<CameraBuilder.CameraComponent>();

                    var date = ev.Data;
                    var transform = entity.GetComponent<TransformComponent>();
                    var ray = camera.UnProject(date.CurrentPosition, ctx);

                    var bounds = geo.Geometry.Bounds;

                    var invert = transform.Matrix.Inverted();
                    var trRay = new Ray(Vector3.TransformCoordinate(ray.Position, invert),Vector3.TransformNormal(ray.Direction, invert));
                    
                    if (trRay.Intersects(bounds)) {
                        entity.AddComponent(new TargetedComponent());
                        entity.GetComponent<MaterialComponent>().Setected();

                        ctx.RemoveEvent(ev);
                    }
                    break;
                case AllInputStates.UnTarget:
                    var targeted = entity.GetComponent<TargetedComponent>();
                    if (targeted != null) {
                        //untarget
                        entity.GetComponent<MaterialComponent>().UnSetected();
                        entity.RemoveComponent(targeted);

                        ctx.RemoveEvent(ev);
                    }
                    break;
            }
        }

     
    }
}
