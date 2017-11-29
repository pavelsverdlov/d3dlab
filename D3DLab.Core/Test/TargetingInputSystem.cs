using D3DLab.Core.Components;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Forms;

namespace D3DLab.Core.Test {
    public sealed class TargetingInputSystem : InputComponent, IComponentSystem, TargetingInputSystem.IInputHandler {

        public TargetingInputSystem(Control control) : base(control) {
        }

        public enum TargetingInputStates {
            Idle = 0,
            Target = 1
        }
        public interface IInputHandler : InputComponent.IHandler {
            bool Target(InputComponent.InputStateDate state);
            void UnTarget(InputStateDate state);
        }

        protected override InputState GetIdleState() {
            var states = new StateDictionary();
            states.Add((int)TargetingInputStates.Idle, s => new InputIdleState(s));
            states.Add((int)TargetingInputStates.Target, s => new InputTargetState(s));

            var router = new StateHandleProcessor<IInputHandler>(states, this);
            router.SwitchTo((int)TargetingInputStates.Idle, new InputStateDate(control));
            return router;
        }

        protected abstract class TargetingStateMachine : InputStateMachine {
            protected TargetingStateMachine(StateProcessor processor) : base(processor) { }

        }
        protected sealed class InputTargetState : TargetingStateMachine {
            public InputTargetState(StateProcessor processor) : base(processor) {
                
            }

            public override void EnterState(InputStateDate state) {
                Processor.InvokeHandler<IInputHandler>(x => x.Target(state));
            }

            public override bool OnMouseDown(InputStateDate state) {
                switch (state.Buttons) {
                    case MouseButtons.Left:
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseUp(InputStateDate state) {
                if ((state.Buttons & MouseButtons.Left) != MouseButtons.Left) {
                    Processor.InvokeHandler<IInputHandler>(x => x.UnTarget(state));
                    SwitchTo((int)TargetingInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
            public override bool OnMouseMove(InputStateDate state) {
                  
                return true;
            }
        }
        protected sealed class InputIdleState : TargetingStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) { }
            public override bool OnMouseDown(InputStateDate state) {
                switch (state.Buttons) {
                    case MouseButtons.Left:
                        SwitchTo((int)TargetingInputStates.Target, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
        }

        public bool Target(InputStateDate state) {
            lastEvent = new TargetInputState { Type = TargetingInputStates.Target, Date = state };
            return false;
        }
        public void UnTarget(InputStateDate state) {
            lastEvent = new TargetInputState();
        }

        private struct TargetInputState {//
            public TargetingInputStates Type;
            public InputStateDate Date;
        }

        private TargetInputState lastEvent;

        public void Execute(IEntityManager emanager, IContext ctx) {
            //return;
            foreach (var entity in emanager.GetEntities()) {
                var targeted = entity.GetComponent<TargetedComponent>();
                if (targeted != null) {
                    //untarget
                    entity.GetComponent<MaterialComponent>().UnSetected();
                    entity.RemoveComponent(targeted);
                }

                var hitable = entity.GetComponent<HitableComponent>();                
                if (hitable == null) {
                    continue;
                }
                switch (lastEvent.Type) {
                    case TargetingInputStates.Target:
                        var geo = entity.GetComponent<GeometryComponent>();
                        var camera = emanager.GetEntities().Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null).First().GetComponent<CameraBuilder.CameraComponent>();

                        var date = lastEvent.Date;

                        var ray = UnProject(date.CurrentPosition, ctx, camera);
                        if (ray.Intersects(geo.Geometry.Bounds)) {
                            entity.AddComponent(new TargetedComponent());
                        }
                        break;
                }
            }
        }

        public static Ray UnProject(Vector2 point2d, IContext ctx, CameraBuilder.CameraComponent camera)//, out Vector3 pointNear, out Vector3 pointFar)
        {
            var p = new Vector3((float)point2d.X, (float)point2d.Y, 1);

            var vp = ctx.World.ViewMatrix * ctx.World.ProjectionMatrix * ctx.World.GetViewportMatrix();
            var vpi = Matrix.Invert(vp);
            p.Z = 0;
            Vector3.TransformCoordinate(ref p, ref vpi, out Vector3 zn);
            p.Z = 1;
            Vector3.TransformCoordinate(ref p, ref vpi, out Vector3 zf);
            Vector3 r = zf - zn;
            r.Normalize();

            switch (camera.CameraType) {
                case CameraBuilder.CameraTypes.Orthographic:
                    if (double.IsNaN(zn.X) || double.IsNaN(zn.Y) || double.IsNaN(zn.Z)) {
                        zn = new Vector3(0, 0, 0);
                    }
                    if (double.IsNaN(r.X) || double.IsNaN(r.Y) || double.IsNaN(r.Z) ||
                        (r.X == 0 && r.Y == 0 && r.Z == 0)) {
                        r = new Vector3(0, 0, 1);
                    }
                    //fix for not valid inverted matrix
                    return new Ray(zn, r);
                case CameraBuilder.CameraTypes.Perspective:
                    return new Ray(camera.Position, r);
                default:
                    throw new NotImplementedException();
            }
        }

        
    }
}
