using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Toolkit.Techniques.TriangleColored;
using D3DLab.Toolkit.Techniques.TriangleTextured;

namespace D3DLab.Toolkit {
    static class TriangleRenderComponentCreator {
        public static D3DTriangleColoredVertexRenderComponent RenderColoredAsStrip() {
            return new D3DTriangleColoredVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
        }

        public static D3DTriangleColoredVertexRenderComponent RenderColoredAsTriangleList(CullMode mode) {
            return new D3DTriangleColoredVertexRenderComponent(mode) {
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };
        }

        public static D3DTriangleTexturedVertexRenderComponent RenderTexturedAsStrip() {
            return new D3DTriangleTexturedVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.TriangleStrip
            };
        }

        public static D3DTriangleTexturedVertexRenderComponent RenderTexturedAsTriangleList(CullMode mode) {
            return new D3DTriangleTexturedVertexRenderComponent(mode) {
                PrimitiveTopology = PrimitiveTopology.TriangleList
            };
        }
    }
}
