using System;
using System.Collections.Generic;
using System.Windows.Forms;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace D3DLab.Core.Components.Behaviors {
    public class InputCameraBehavior: InputComponent, IAttachTo<OrthographicCameraEntity>, InputCameraBehavior.IInputHandler {
        public enum InputStates {
            Idle =0,
            Rotate = 1,
            Pan = 2,
            Zoom = 3
        }
        public interface IInputHandler : IHandler{
            bool Rotate(InputStateData state);
            void Zoom(InputStateData state);
        }
       
        protected abstract class CameraStateMachine : InputStateMachine {
            protected CameraStateMachine(StateProcessor processor) : base(processor) {}
          
        }

        protected sealed class InputIdleState : CameraStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) {}
            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        SwitchTo((int)InputStates.Pan, state);
                        break;
                    case GeneralMouseButtons.Right:
                        SwitchTo((int)InputStates.Rotate, state);
                        break;
                    //case GeneralMouseButtons.Middle:
                    //    //    SwitchTo(InputControllerState.DownMiddle, state);
                    //    break;
                }
                return base.OnMouseDown(state);
            }

            public override bool OnMouseWheel(InputStateData state) {
                SwitchTo((int)InputStates.Zoom, state);
                return base.OnMouseWheel(state);
            }
        }

        protected sealed class InputRotateState : CameraStateMachine {
            public InputRotateState(StateProcessor processor) : base(processor) {
                // Cursor.Hide();
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    Cursor.Show();
                    SwitchTo((int)InputStates.Idle, state);
                }
                
                return base.OnMouseUp(state);
            }
            public override bool OnMouseDown(InputStateData state) {
                switch (state.Buttons) {
                    case GeneralMouseButtons.Left | GeneralMouseButtons.Right:
                        SwitchTo((int)InputStates.Pan, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
            public override bool OnMouseMove(InputStateData state) {
                Cursor.Position = state.ButtonsStates[GeneralMouseButtons.Right].CursorPointDown;
                Processor.InvokeHandler<IInputHandler>(x=>x.Rotate(state));
                return true;
            }
        }
        protected sealed class InputPanState : CameraStateMachine {
            public InputPanState(StateProcessor processor) : base(processor) {
            }
            public override bool OnMouseUp(InputStateData state) {
                if ((state.Buttons & GeneralMouseButtons.Right) != GeneralMouseButtons.Right) {
                    Cursor.Show();
                    SwitchTo((int)InputStates.Idle, state);
                }

                return base.OnMouseUp(state);
            }
        }

        protected sealed class InputZoomState : CameraStateMachine {
            public InputZoomState(StateProcessor processor) : base(processor) {}
            public override bool OnMouseWheel(InputStateData state) {
                Processor.InvokeHandler<IInputHandler>(x => x.Zoom(state));
                return true;
            }

            public override bool OnMouseMove(InputStateData state) {
                SwitchTo((int)InputStates.Idle, state);
                return false;
            }
        }

        public InputCameraBehavior(Control control) : base(control) {}
        protected override InputState GetIdleState() {
            var states = new StateDictionary();
            states.Add((int)InputStates.Idle, s => new InputIdleState(s));
            states.Add((int)InputStates.Rotate, s => new InputRotateState(s));
            states.Add((int)InputStates.Zoom, s => new InputZoomState(s));

            var router = new StateHandleProcessor<IInputHandler>(states, this);
            router.SwitchTo((int)InputStates.Idle, new InputStateData());
            return router;
        }

        public OrthographicCameraEntity Parent { get; private set; }

        public void OnAttach(OrthographicCameraEntity camera) {
            this.Parent = camera ;
        }

        #region IInputHandler

        static float kr = -0.35f;

        public void Zoom(InputStateData state) {
            var panK = Parent.Data.Width / state.ControlWidth;

            int delta = state.Delta;
            int x = state.CursorCurrentPosition.X;
            int y = state.CursorCurrentPosition.Y;

            var p1 = new Vector2(x * panK, y * panK);
            var p0 = new Vector2(state.ControlWidth * 0.5f * panK, state.ControlHeight * 0.5f * panK);

            var d = 1 - delta * 0.001f;
            var prevWidth = Parent.Data.Width;
            //Width *= d;
            var newWidth = Parent.Data.Width * d;
            CheckMinMax(ref newWidth);
            d = newWidth / prevWidth;

            var pan = (p1 - p0) * (d - 1);

            var up = Parent.Data.UpDirection.Normalized();
            var left = Vector3.Cross(up, Parent.Data.LookDirection.Normalized());
            left.Normalize();

            var panVector = left * pan.X + up * pan.Y;

            Parent.Data = new CameraData {
                UpDirection = Parent.Data.UpDirection,
                LookDirection = Parent.Data.LookDirection,
                FarPlaneDistance = Parent.Data.FarPlaneDistance,
                NearPlaneDistance = Parent.Data.NearPlaneDistance,
                Position = Parent.Data.Position + panVector,
                Width = newWidth
            };
        }

        private void CheckMinMax(ref float value) {
            var MinWidth = 1;
            var MaxWidth = 3000;
            if (value < MinWidth)
                value = MinWidth;
            if (value > MaxWidth)
                value = MaxWidth;
        }


        public bool Rotate(InputStateData state) {
            var p1 = state.ButtonsStates[MouseButtons.Right].PointDown;
            var p2 = state.CurrentPosition;

            var moveV = p2 - p1;
            if (moveV.Length() < 0.5f || moveV == Vector2.Zero) {
                return false;
            }

            float dx = moveV.X;
            float dy = moveV.Y;
            //            CameraRotateMode rotateMode = CameraRotateMode.Rotate3D;

            var matrixRotate = GetMatrixRotate3D(dx, dy);// rotateMode == CameraRotateMode.Rotate3D ? GetMatrixRotate3D(dx, dy) 
                                                         //                :
                                                         //                rotateMode == CameraRotateMode.RotateAroundX ? GetMatrixRotateAroundX(dx, dy) :
                                                         //                rotateMode == CameraRotateMode.RotateAroundY ? GetMatrixRotateAroundY(dx, dy) :
                                                         //                rotateMode == CameraRotateMode.RotateAroundZ ? GetMatrixRotateAroundZ(dx, dy) :
                                                         //                rotateMode == CameraRotateMode.RotateAroundZInverted ? GetMatrixRotateAroundZ(-dx, -dy)
                                                         //                : Matrix.Identity;

            if (matrixRotate.IsIdentity)
                return false;

            var data = new CameraData {
                UpDirection = Vector3.TransformNormal(Parent.Data.UpDirection.Normalized(), matrixRotate).Normalized(),
                LookDirection = Vector3.TransformNormal(Parent.Data.LookDirection.Normalized(), matrixRotate).Normalized(),
                FarPlaneDistance = Parent.Data.FarPlaneDistance,
                NearPlaneDistance = Parent.Data.NearPlaneDistance,
                Position = Vector3.TransformCoordinate(Parent.Data.Position, matrixRotate),
                Width = Parent.Data.Width
            };

            Parent.Data = data;

            // Debug.WriteLine("{0} Rotate {1}", Thread.CurrentThread.ApartmentState,Parent.LookDirection);

            return true;
        }
        private Matrix GetMatrixRotate3D(float dx, float dy) {
            var v2Up = new Vector2(0, -1);
            var mouseMove = new Vector2(-dx, -dy);
            var angleLook = Vector2Extensions.AngleBetween(v2Up, mouseMove.Normalized()).ToRad();

            var look = Parent.Data.LookDirection;
            look.Normalize();
            var up = Parent.Data.UpDirection;
            up.Normalize();

            var axis = Vector3.TransformNormal(up, Matrix.RotationAxis(look, angleLook + MathUtil.PiOverTwo));
            var angle = mouseMove.Length() * kr;

            var orthCamera = Parent;

            if (orthCamera != null && orthCamera.Data.Width < 500) {
                var k = ((float)orthCamera.Data.Width / 500f) * 0.7f + 0.3f;
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