using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Toolkit._CommonShaders;
using System;
using System.Runtime.CompilerServices;

namespace D3DLab.Toolkit {

    public class RenderTechniqueException : Exception {
        public RenderTechniqueException(string mess) : base(mess) { }
        public static RenderTechniqueException NoVertexAndIndexBuffers =>
            new RenderTechniqueException("Should be initialized at least one Vertex OR Index buffer.");
        public static RenderTechniqueException NoWorldTransformBuffers =>
           new RenderTechniqueException("Should be initialized World Transform buffer.");
    }

    public abstract class NestedRenderTechniqueSystem<TProperties> : D3DAbstractRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {

        protected NestedRenderTechniqueSystem() {
            disposer = new DisposeObserver();
        }

        [Obsolete("Remake with IsModified")]
        protected void ApplyTransformWorldBufferToRenderComp(GraphicsDevice graphics, D3DRenderComponent render, TransformComponent transform) {
            if (transform.IsModified || !render.TransformWorldBuffer.HasValue) {
                var tr = TransforStructBuffer.ToTranspose(transform.MatrixWorld);

                if (render.TransformWorldBuffer.HasValue) {
                    var buff = render.TransformWorldBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref tr, buff);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
                    render.TransformWorldBuffer.Set(buff);
                }

                //transform.IsModified = false;
            }
        }
        protected SharpDX.Direct3D11.Buffer CreateTransformWorldBuffer(GraphicsDevice graphics, ref TransformComponent transform) {
            var tr = TransforStructBuffer.ToTranspose(transform.MatrixWorld);
            return graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
        }

        protected readonly DisposeObserver disposer;

        public override void CleanupRenderCache() {
            disposer.DisposeObservables();
            base.CleanupRenderCache();
        }

    }
}
