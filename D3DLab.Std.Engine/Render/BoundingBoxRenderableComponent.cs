using D3DLab.Std.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Shaders;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace D3DLab.Std.Engine.Render {
    public class BoundingBoxRenderableComponent : ShaderComponent, IRenderableComponent {

        public BoundingBox Box { get; set; }

        public BoundingBoxRenderableComponent(ShaderTechniquePass technique):base(new[] { technique }) {
            EntityTag = new ElementTag("BoundingBox");
        }

        public override void Dispose() {

        }

        public void Render(RenderState state) {

        }

        public void Update(RenderState state) {

        }

        public override VertexLayoutDescription[] GetLayoutDescription() {
            return new[] {
                new VertexLayoutDescription(
                        new VertexElementDescription("p", VertexElementSemantic.Position, VertexElementFormat.Float4),
                        new VertexElementDescription("c", VertexElementSemantic.Color, VertexElementFormat.Float4))
            };
        }
    }
}
