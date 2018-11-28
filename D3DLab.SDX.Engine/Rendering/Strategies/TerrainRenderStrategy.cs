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
        readonly List<Tuple<D3DTerrainRenderComponent, IGeometryComponent, D3DTexturedMaterialComponent>> entities;

        public TerrainRenderStrategy(D3DShaderCompilator compilator, IRenderTechniquePass pass, VertexLayoutConstructor layoutConstructor) : base(compilator, pass, layoutConstructor) {
            entities = new List<Tuple<D3DTerrainRenderComponent, IGeometryComponent, D3DTexturedMaterialComponent>>();
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
                var material = en.Item3;

                context.InputAssembler.PrimitiveTopology = renderCom.PrimitiveTopology;

                if (material.IsModified) {
                    material.TextureResource = graphics.TexturedLoader.LoadShaderResource(material.Image);
                    material.SampleState = graphics.CreateSampler(material.SampleDescription);
                    material.IsModified = false;
                }

                if (geo.IsModified) {
                    var pos = geo.Positions;
                    var normals = geo.Normals;
                    var vertices = new StategyStaticShaders.Terrain.TerrainVertex[pos.Length];

                    for (var i = 0; i < pos.Length; i++) {
                        vertices[i] = new StategyStaticShaders.Terrain.TerrainVertex {
                            position = pos[i], normal = normals[i], color = geo.Colors[i], texcoor = geo.TextureCoordinates[i]
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

                context.PixelShader.SetShaderResource(0, material.TextureResource);
                context.PixelShader.SetSampler(0, material.SampleState);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderCom.VertexBuffer, StategyStaticShaders.Terrain.TerrainVertex.Size, 0));
                context.InputAssembler.SetIndexBuffer(renderCom.IndexBuffer, Format.R32_UInt, 0);//R32_SInt

                //

                graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());

                graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                //graphics.ImmediateContext.Draw(geo.Positions.Length, 0);

            }
        }

        internal void RegisterEntity(D3DTerrainRenderComponent com, IGeometryComponent geo, D3DTexturedMaterialComponent material) {
            entities.Add(Tuple.Create(com, geo, material));
        }
    }
}

