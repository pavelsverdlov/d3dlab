using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Numerics;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTransformComponent : TransformComponent, ID3DRenderable {
        public SharpDX.Direct3D11.Buffer TransformBuffer { get; set; }
        public D3DTransformComponent() {
            IsModified = true;
        }

        public override void Dispose() {
            TransformBuffer?.Dispose();
            base.Dispose();
        }

        void ID3DRenderable.Update(GraphicsDevice graphics) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            var tr = new TransforStructBuffer(Matrix4x4.Transpose(MatrixWorld));
            if (TransformBuffer.IsNull()) {                
                TransformBuffer = graphics.CreateDynamicBuffer(ref tr, Utilities.SizeOf<TransforStructBuffer>());
                IsModified = false;
            } else {
                graphics.UpdateDynamicBuffer(ref tr, TransformBuffer);
                IsModified = false;
            }
        }
        void ID3DRenderable.Render(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;
            context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, TransformBuffer);
        }
    }


}
