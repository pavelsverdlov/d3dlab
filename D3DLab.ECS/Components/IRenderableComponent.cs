using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Components {
    public interface IRenderableComponent : IGraphicComponent {
        bool CanRender { get; set; }
    }
}
