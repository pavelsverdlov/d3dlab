using D3DLab.Core.Components;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Forms;
using HelixToolkit.Wpf.SharpDX;

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
            currentEvent = new TargetInputState { Type = TargetingInputStates.Target, Date = state };
            return false;
        }
        public void UnTarget(InputStateDate state) {
            currentEvent = new TargetInputState();
        }

        private struct TargetInputState {//
            public TargetingInputStates Type;
            public InputStateDate Date;
        }

        private TargetInputState currentEvent;
        private TargetInputState prevEvent;

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
                switch (currentEvent.Type) {
                    case TargetingInputStates.Target:
                        var geo = entity.GetComponent<GeometryComponent>();
                        var camera = emanager.GetEntities().Where(x => x.GetComponent<CameraBuilder.CameraComponent>() != null).First().GetComponent<CameraBuilder.CameraComponent>();

                        var date = currentEvent.Date;
                        var transform = entity.GetComponent<TransformComponent>();
                        var ray = camera.UnProject(date.CurrentPosition, ctx);
                        if (ray.Intersects(geo.Geometry.Transform(transform.Matrix).Bounds)) {
                            var targetComponent = new TargetedComponent();

                            entity.AddComponent(targetComponent);
                        }

                        prevEvent = currentEvent;

                        break;
                }
            }
        }

     
    }
}
