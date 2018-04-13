using SharpDX;
using System.Linq;
using HelixToolkit.Wpf.SharpDX;
using D3DLab.Core.Input;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using D3DLab.Core.Context;
using D3DLab.Std.Engine.Core.Input;

namespace D3DLab.Core.Test {
    public sealed class TargetingSystem : IComponentSystem {
        public IViewportContext ctx { get; set; }
        public void Execute(IEntityManager emanager, InputSnapshot input) {
            //return;
            foreach (var entity in emanager.GetEntities()) {
                var hitable = entity.GetComponent<HitableComponent>();                
                if (hitable == null || !input.Events.Any()) {
                    continue;
                }
                var events = input.Events.ToArray();
                foreach (var ev in events) {
                    Handle(ev, emanager, entity, input);
                }                
            }
        }

        private void Handle(InputEventState ev, IEntityManager emanager, Entity entity, InputSnapshot input) {
            switch ((AllInputStates)ev.Type) {
                case AllInputStates.Target:
                    var geo = entity.GetComponent<GeometryComponent>();
                    var camera = emanager.GetEntities()
                        .Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null)
                        .First()
                        .GetComponent<CameraBuilder.CameraComponent>();

                    var date = ev.Data;
                    var transform = entity.GetComponent<TransformComponent>();
                    var ray = camera.UnProject(date.CurrentPosition.ToSharpDX(), ctx);

                    var bounds = geo.Geometry.Bounds;

                    var invert = transform.GetMatrix().Inverted();
                    var trRay = new Ray(Vector3.TransformCoordinate(ray.Position, invert),Vector3.TransformNormal(ray.Direction, invert));
                    
                    if (trRay.Intersects(bounds)) {
                        entity.AddComponent(new TargetedComponent());
                        entity.GetComponent<MaterialComponent>().Setected();

                        input.RemoveEvent(ev);
                    }
                    break;
                case AllInputStates.UnTarget:
                    var targeted = entity.GetComponent<TargetedComponent>();
                    if (targeted != null) {
                        //untarget
                        entity.GetComponent<MaterialComponent>().UnSetected();
                        entity.RemoveComponent(targeted);

                        input.RemoveEvent(ev);
                    }
                    break;
            }
        }

     
    }
}
