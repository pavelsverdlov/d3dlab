using D3DLab.Core.Components;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public InputTargetState(StateProcessor processor) : base(processor) { }

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
            return false;
        }

        private struct TargetInputState {//
            public TargetingInputStates Type;
            public InputStateDate Date;
        }

        private TargetInputState lastEvent;

        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {
                var hitable = entity.GetComponent<HitableComponent>();
                if (hitable == null) {
                    continue;
                }
                switch (lastEvent.Type) {
                    case TargetingInputStates.Target:
                        var geo = entity.GetComponent<GeometryComponent>();

                        var date = lastEvent.Date;

                        //unproject 


                        var ray = new Ray();
                        if (ray.Intersects(geo.Geometry.Bounds)) {

                        }

                        break;
                }
            }
        }
    }


    public class CameraInputSystem : InputComponent, IComponentSystem, CameraInputSystem.IInputHandler {
        public interface IInputHandler : InputComponent.IHandler {
            bool Rotate(InputComponent.InputStateDate state);
            void Zoom(InputComponent.InputStateDate state);
        }
        public enum CameraInputStates {
            Idle = 0,
            Rotate = 1,
            Pan = 2,
            Zoom = 3
        }
        protected abstract class CameraStateMachine : InputStateMachine {
            protected CameraStateMachine(StateProcessor processor) : base(processor) { }

        }
        protected sealed class InputIdleState : CameraStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) { }
            public override bool OnMouseDown(InputStateDate state) {
                switch (state.Buttons) {
                    case MouseButtons.Left | MouseButtons.Right:
                        SwitchTo((int)CameraInputStates.Pan, state);
                        break;
                    case MouseButtons.Right:
                        SwitchTo((int)CameraInputStates.Rotate, state);
                        break;
                    case MouseButtons.Middle:
                        //    SwitchTo(InputControllerState.DownMiddle, state);
                        break;
                }
                return base.OnMouseDown(state);
            }

            public override bool OnMouseWheel(InputStateDate state) {
                SwitchTo((int)CameraInputStates.Zoom, state);
                return base.OnMouseWheel(state);
            }
        }
        protected sealed class InputRotateState : CameraStateMachine {
            public InputRotateState(StateProcessor processor) : base(processor) {
                // Cursor.Hide();
            }
            public override bool OnMouseUp(InputStateDate state) {
                if ((state.Buttons & MouseButtons.Right) != MouseButtons.Right) {
                    Cursor.Show();
                    SwitchTo((int)CameraInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
            public override bool OnMouseDown(InputStateDate state) {
                switch (state.Buttons) {
                    case MouseButtons.Left | MouseButtons.Right:
                        SwitchTo((int)CameraInputStates.Pan, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseMove(InputStateDate state) {
                Cursor.Position = state.ButtonsStates[MouseButtons.Right].CursorPointDown;
                Processor.InvokeHandler<IInputHandler>(x => x.Rotate(state));
                return true;
            }
        }
        protected sealed class InputPanState : CameraStateMachine {
            public InputPanState(StateProcessor processor) : base(processor) {
            }
            public override bool OnMouseUp(InputStateDate state) {
                if ((state.Buttons & MouseButtons.Right) != MouseButtons.Right) {
                    Cursor.Show();
                    SwitchTo((int)CameraInputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
        }

        protected sealed class InputZoomState : CameraStateMachine {
            public InputZoomState(StateProcessor processor) : base(processor) { }
            public override bool OnMouseWheel(InputStateDate state) {
                Processor.InvokeHandler<IInputHandler>(x => x.Zoom(state));
                return true;
            }

            public override bool OnMouseMove(InputStateDate state) {
                SwitchTo((int)CameraInputStates.Idle, state);
                return false;
            }
        }

        public CameraInputSystem(Control control) : base(control) { }
        protected override InputState GetIdleState() {
            var states = new StateDictionary();
            states.Add((int)CameraInputStates.Idle, s => new InputIdleState(s));
            states.Add((int)CameraInputStates.Rotate, s => new InputRotateState(s));
            states.Add((int)CameraInputStates.Zoom, s => new InputZoomState(s));

            var router = new StateHandleProcessor<IInputHandler>(states, this);
            router.SwitchTo((int)CameraInputStates.Idle, new InputStateDate(control));
            return router;
        }


        private struct CameraInputState {//
            public CameraInputStates Type;
            public InputStateDate Date;
        }

        public void Zoom(InputStateDate state) {
            events.Add(new CameraInputState { Date = state, Type = CameraInputStates.Zoom });
        }
        public bool Rotate(InputStateDate state) {
            events.Add(new CameraInputState { Date = state, Type = CameraInputStates.Rotate });
            return false;
        }

        private List<CameraInputState> events = new List<CameraInputState>();

        public void Execute(IEntityManager emanager, IContext ctx) {
            foreach (var entity in emanager.GetEntities()) {
                var ccom = entity.GetComponent<CameraBuilder.CameraComponent>();
                if (ccom == null || !events.Any()) {
                    continue;
                }
                var input = events.Last();
                var state = input.Date;
                switch (input.Type) {
                    case CameraInputStates.Zoom:

                        var panK = ccom.Width / state.ControlWidth;

                        int delta = state.Delta;
                        int x = state.CursorCurrentPosition.X;
                        int y = state.CursorCurrentPosition.Y;

                        var p1 = new Vector2(x * panK, y * panK);
                        var p0 = new Vector2(state.ControlWidth * 0.5f * panK, state.ControlHeight * 0.5f * panK);

                        var d = 1 - delta * 0.001f;
                        var prevWidth = ccom.Width;
                        //Width *= d;
                        var newWidth = ccom.Width * d;
                        CheckMinMax(ref newWidth);
                        d = newWidth / prevWidth;

                        var pan = (p1 - p0) * (d - 1);

                        var up = ccom.UpDirection.Normalized();
                        var left = Vector3.Cross(up, ccom.LookDirection.Normalized());
                        left.Normalize();

                        var panVector = left * pan.X + up * pan.Y;

                        ccom.UpDirection = ccom.UpDirection;
                        ccom.Position = ccom.Position + panVector;
                        ccom.Width = newWidth;
                        events.Clear();
                        break;
                    case CameraInputStates.Rotate:
                        var p11 = state.ButtonsStates[MouseButtons.Right].PointDown;
                        var p2 = state.CurrentPosition;

                        var moveV = p2 - p11;
                        if (moveV.Length() < 0.5f || moveV == Vector2.Zero) {
                            continue;
                        }

                        float dx = moveV.X;
                        float dy = moveV.Y;
                        //            CameraRotateMode rotateMode = CameraRotateMode.Rotate3D;

                        var matrixRotate = GetMatrixRotate3D(dx, dy, ccom);// rotateMode == CameraRotateMode.Rotate3D ? GetMatrixRotate3D(dx, dy) 
                                                                           //                :
                                                                           //                rotateMode == CameraRotateMode.RotateAroundX ? GetMatrixRotateAroundX(dx, dy) :
                                                                           //                rotateMode == CameraRotateMode.RotateAroundY ? GetMatrixRotateAroundY(dx, dy) :
                                                                           //                rotateMode == CameraRotateMode.RotateAroundZ ? GetMatrixRotateAroundZ(dx, dy) :
                                                                           //                rotateMode == CameraRotateMode.RotateAroundZInverted ? GetMatrixRotateAroundZ(-dx, -dy)
                                                                           //                : Matrix.Identity;

                        if (matrixRotate.IsIdentity) {
                            continue;
                        }

                        ccom.UpDirection = Vector3.TransformNormal(ccom.UpDirection.Normalized(), matrixRotate).Normalized();
                        ccom.LookDirection = Vector3.TransformNormal(ccom.LookDirection.Normalized(), matrixRotate).Normalized();
                        ccom.Position = Vector3.TransformCoordinate(ccom.Position, matrixRotate);
                        events.Clear();
                        break;
                }

            }
        }


        #region Camera

        static float kr = -0.35f;
        private void CheckMinMax(ref float value) {
            var MinWidth = 1;
            var MaxWidth = 3000;
            if (value < MinWidth)
                value = MinWidth;
            if (value > MaxWidth)
                value = MaxWidth;
        }

        private Matrix GetMatrixRotate3D(float dx, float dy, CameraBuilder.CameraComponent data) {
            var v2Up = new Vector2(0, -1);
            var mouseMove = new Vector2(-dx, -dy);
            var angleLook = Vector2Extensions.AngleBetween(v2Up, mouseMove.Normalized()).ToRad();

            var look = data.LookDirection;
            look.Normalize();
            var up = data.UpDirection;
            up.Normalize();

            var axis = Vector3.TransformNormal(up, Matrix.RotationAxis(look, angleLook + MathUtil.PiOverTwo));
            var angle = mouseMove.Length() * kr;

            var orthCamera = data;

            if (orthCamera != null && orthCamera.Width < 500) {
                var k = ((float)orthCamera.Width / 500f) * 0.7f + 0.3f;
                angle *= k;
            }

            return ToRotateMatrix(axis, angle);
        }
        private Matrix ToRotateMatrix(Vector3 axis, float angle) {
            var matrixRotate = SDXCommonExtensions.RotateAroundAxis(axis, angle, Vector3.Zero);
            return matrixRotate;
        }

        #endregion
    }
}
