using D3DLab.Core.Components;
using D3DLab.Core.Context;
using D3DLab.Core.Input;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;
using System.Linq;
using D3DLab.Std.Engine.Core.Input;

namespace D3DLab.Core.Test {
    public class CameraSystem : IComponentSystem {
        public IViewportContext ctx {get; set;}
        public void Execute(IEntityManager emanager, InputSnapshot input) {
            foreach (var entity in emanager.GetEntities()) {
                var ccom = entity.GetComponent<CameraBuilder.CameraComponent>();
                // Debug.WriteLine("object = {0}, events.Type = {1}", ccom?.Guid.ToString() ?? "Null", events.Type);
                if (ccom == null || !input.Events.Any()) {
                    continue;
                }

                // Debug.WriteLine("HIT");
                var events = input.Events.ToArray();
                foreach (var ev in events) {
                    Handle(ev, ccom, input);
                }
            }
        }
        
        private void Handle(InputEventState ev, CameraBuilder.CameraComponent ccom, InputSnapshot input) {
            var state = ev.Data;
            switch ((AllInputStates)ev.Type) {
                case AllInputStates.Zoom:
                    input.RemoveEvent(ev);

                    var panK = ccom.Width / ctx.Graphics.SharpDevice.Width;

                    int delta = state.Delta;
                    int x = state.CursorCurrentPosition.X;
                    int y = state.CursorCurrentPosition.Y;

                    var p1 = new Vector2(x * panK, y * panK);
                    var p0 = new Vector2(ctx.Graphics.SharpDevice.Width * 0.5f * panK, ctx.Graphics.SharpDevice.Height * 0.5f * panK);

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

                    break;
                case AllInputStates.Rotate:
                    input.RemoveEvent(ev);

                    var p11 = state.ButtonsStates[GeneralMouseButtons.Right].PointDown;
                    var p2 = state.CurrentPosition;

                    var moveV = p2 - p11;
                    if (moveV.ToSharpDX() == Vector2.Zero) {
                        return;
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
                        return;
                    }

                    ccom.UpDirection = Vector3.TransformNormal(ccom.UpDirection.Normalized(), matrixRotate).Normalized();
                    ccom.LookDirection = Vector3.TransformNormal(ccom.LookDirection.Normalized(), matrixRotate).Normalized();
                    ccom.Position = Vector3.TransformCoordinate(ccom.Position, matrixRotate);


                    break;
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
