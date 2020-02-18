using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.D3D {

    public class RenderTechniqueException : Exception {
        public RenderTechniqueException(string mess) : base(mess) { }
        public static RenderTechniqueException NoVertexAndIndexBuffers =>
            new RenderTechniqueException("Should be initialized at least one Vertex OR Index buffer.");
        public static RenderTechniqueException NoWorldTransformBuffers =>
           new RenderTechniqueException("Should be initialized World Transform buffer.");
    }

    public abstract class NestedRenderTechniqueSystem<TProperties> : D3DAbstractRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {

        protected NestedRenderTechniqueSystem(EntityHasSet entityHasSet) : base(entityHasSet) {
            disposer = new DisposeObserver();
        }

        protected void ApplyTransformWorldBufferToRenderComp(GraphicsDevice graphics, ID3DTransformWorldRenderComponent render, TransformComponent transform) {
            if (transform.IsModified || !render.TransformWorldBuffer.HasValue) {
                var tr = TransforStructBuffer.ToTranspose(transform.MatrixWorld);

                if (render.TransformWorldBuffer.HasValue) {
                    var buff = render.TransformWorldBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref tr, buff, TransforStructBuffer.RegisterResourceSlot);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
                    render.TransformWorldBuffer.Set(buff);
                }

                transform.IsModified = false;
            }
        }

        protected readonly DisposeObserver disposer;


    }
}
