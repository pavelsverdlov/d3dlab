using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Input {
    public sealed class CameraZoomCommand : IInputCommand {
        const float scrollSpeed = 0.5f;
        readonly InputStateData istate;

        public CameraZoomCommand(InputStateData state) {
            this.istate = state;
        }

        public bool Execute(Entity entity) {
            var find = entity.GetComponents<CameraBuilder.CameraComponent>();
            if (!find.Any()) {
                return false;
            }
            
            var ccom = find.First();
            var state = istate;

            var delta = state.Delta;
            /*
            var PanK = 1;

            var x = state.CursorCurrentPosition.X;
            var y = state.CursorCurrentPosition.Y;

            var kx = x / ccom.VWidth;
            var ky = y / ccom.VHeight;

            var p1 = new Vector2(x * PanK, y * PanK);
            var p0 = new Vector2(ccom.VWidth * 0.5f * PanK, ccom.VHeight * 0.5f * PanK);

            var d = 1 - delta * 0.001f;
            var prevWidth = ccom.Width;

            var newWidth = ccom.Width * d;
            d = newWidth / prevWidth;

            var pan = (p1 - p0) * (d - 1);

            var left = Vector3.Cross(ccom.UpDirection.Normalize(), ccom.LookDirection.Normalize());
            left.Normalize();

            var panVector = left * pan.X + ccom.UpDirection.Normalize() * pan.Y;

            ccom.Width = newWidth;
            ccom.Position = ccom.Position + panVector;
            */
            //ccom.UpdatePerspectiveMatrix();

            //ccom.Position = ccom.Position + ccom.LookDirection*2;

            //ccom.UpdateViewMatrix();
            var nscale = ccom.Scale + (delta * 0.001f);// * (float)Math.Sin(delta)
            if (nscale > 0) {
                ccom.Scale = nscale;
                ccom.UpdatePerspectiveMatrix();
            }

            return true;
        }
    }
}
