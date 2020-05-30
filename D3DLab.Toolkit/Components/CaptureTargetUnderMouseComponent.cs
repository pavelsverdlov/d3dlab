using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct CaptureTargetUnderMouseComponent : IGraphicComponent {
        internal static CaptureTargetUnderMouseComponent Create(Vector2 v2)
            => new CaptureTargetUnderMouseComponent(v2);

        public readonly Vector2 ScreenPosition;

        public CaptureTargetUnderMouseComponent(Vector2 screenPosition) : this() {
            ScreenPosition = screenPosition;
            Tag = ElementTag.New();
            IsValid = true;
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; private set; }
        public void Dispose() {
            IsDisposed = true;
        }


    }

}
