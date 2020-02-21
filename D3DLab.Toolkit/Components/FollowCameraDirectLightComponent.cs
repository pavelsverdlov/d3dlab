using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct FollowCameraDirectLightComponent : IGraphicComponent {

        public static FollowCameraDirectLightComponent Create() {
            return new FollowCameraDirectLightComponent {
                Tag = new ElementTag(Guid.NewGuid().ToString())
            };
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid => true;
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
        }
    }
}
