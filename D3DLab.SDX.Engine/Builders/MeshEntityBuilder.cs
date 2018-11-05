using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.SDX.Engine.Builders {
    public static class MeshEntityBuilder {
        public static ElementTag Build(IEntityManager manager, List<Vector3> pos, List<int> indexes, List<Vector4> colors) {
            return manager
                .CreateEntity(new ElementTag("Geometry"+Guid.NewGuid()))
                .AddComponent(new Std.Engine.Core.Components.GeometryComponent() {
                    Positions = pos,
                    Indices = indexes,
                    Colors = colors
                })
                .AddComponent(new SDX.Engine.Components.D3DTriangleColoredVertexesRenderComponent())
                .Tag;
        }
    }
}
