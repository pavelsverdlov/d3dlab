using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal class TerrainRenderStrategy : RenderStrategy, IRenderStrategy {
        readonly List<Tuple<D3DTerrainRenderComponent, IGeometryComponent>> entities;

        public TerrainRenderStrategy(D3DShaderCompilator compilator, IRenderTechniquePass pass, VertexLayoutConstructor layoutConstructor) : base(compilator, pass, layoutConstructor) {
            entities = new List<Tuple<D3DTerrainRenderComponent, IGeometryComponent>>();
        }

        public void Cleanup() {
            entities.Clear();
        }

        protected override void Rendering(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
            context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

            foreach (var en in entities) {
                var renderCom = en.Item1;
                var geo = en.Item2;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;

                if (geo.IsModified) {
                    var pos = geo.Positions;
                    var vertices = new StategyStaticShaders.Terrain.TerrainVertex[geo.Indices.Length];

                    for (var i = 0; i < pos.Length; i++) {
                        vertices[i] = new StategyStaticShaders.Terrain.TerrainVertex {
                            position = pos[i], color = V4Colors.Green
                        };
                    }

                    geo.MarkAsRendered();

                    renderCom.VertexBuffer?.Dispose();
                    renderCom.IndexBuffer?.Dispose();

                    renderCom.VertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
                    renderCom.IndexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray());
                }

                var tr = new TransforStructBuffer(Matrix4x4.Identity);
                var TransformBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref tr);

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, TransformBuffer);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, Unsafe.SizeOf<StategyStaticShaders.Terrain.TerrainVertex>(), 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt


                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());

                //graphics.ImmediateContext.DrawIndexed(indexCount, 0, 0);
                graphics.ImmediateContext.Draw(geo.Positions.Length, 0);

            }
        }

        internal void RegisterEntity(D3DTerrainRenderComponent com, IGeometryComponent geo) {
            entities.Add(Tuple.Create(com, geo));
        }
    }
}

