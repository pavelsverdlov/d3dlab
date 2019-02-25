using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal class SphereRenderStrategy : RenderTechniqueSystem {
        readonly List<Tuple<D3DSphereRenderComponent, IGeometryComponent>> entities;

        public SphereRenderStrategy() :base(null){ 
            entities = new List<Tuple<D3DSphereRenderComponent, IGeometryComponent>>();
        }

        public new void Cleanup() {
            entities.Clear();
        }
        public void RegisterEntity(D3DSphereRenderComponent rcom, IGeometryComponent geocom) {
            entities.Add(Tuple.Create(rcom, geocom));
        }

        protected override void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            
            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);

            context.GeometryShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
            context.GeometryShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geometryCom = en.Item2;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;
                if (geometryCom.IsModified) {
                    var center = geometryCom.Positions[0];

                    var vertices = new StategyStaticShaders.SphereByPoint.SpherePoint[1];
                    vertices[0] = new StategyStaticShaders.SphereByPoint.SpherePoint(center, geometryCom.Colors[0]);

                    geometryCom.MarkAsRendered();

                    renderCom.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertices));
                    renderCom.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer,new[] { 0 }));
                }
                var tr = new TransforStructBuffer(Matrix4x4.Identity);
                var TransformBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref tr);

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, TransformBuffer);
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer.Get(), SharpDX.Utilities.SizeOf<StategyStaticShaders.SphereByPoint.SpherePoint>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer.Get(), Format.R32_UInt, 0);

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());
                //graphics.ImmediateContext.DrawIndexed(indexCount, 0, 0);
                graphics.ImmediateContext.Draw(geometryCom.Positions.Length, 0);
            }
        }
    }
}
