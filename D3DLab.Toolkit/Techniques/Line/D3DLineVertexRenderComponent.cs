using D3DLab.SDX.Engine.Components;
using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct3D;

namespace D3DLab.Toolkit.Techniques.Line {
    class D3DLineVertexRenderComponent : D3DRenderComponent {
        public static D3DLineVertexRenderComponent AsLineStrip() {
            return new D3DLineVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.LineStrip
            };
        }
        public static D3DLineVertexRenderComponent AsLineList() {
            return new D3DLineVertexRenderComponent() {
                PrimitiveTopology = PrimitiveTopology.LineList
            };
        }

        public D3DLineVertexRenderComponent() : base() {
            PrimitiveTopology = PrimitiveTopology.LineStrip;
            RasterizerStateDescription = new SDX.Engine.Rendering.D3DRasterizerState(
                new SharpDX.Direct3D11.RasterizerStateDescription2() {
                    CullMode = SharpDX.Direct3D11.CullMode.None,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                    IsMultisampleEnabled = true,
                    IsAntialiasedLineEnabled = true,
                    IsFrontCounterClockwise = true
                }) ;
        }
    }
}
