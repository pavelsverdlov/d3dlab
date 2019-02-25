using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Filter;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.Wpf.Engine.App.D3D.Techniques {
    class TerrainRenderTechnique : RenderTechniqueSystem, IRenderTechniqueSystem {
        const string path = @"D3DLab.Wpf.Engine.App.D3D.Shaders.terrain.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static TerrainRenderTechnique() {
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4()
               .AddTexCoorElementAsVector2();

            var d = new CombinedShadersLoader(typeof(TerrainRenderTechnique));

            pass = new D3DShaderTechniquePass(d.Load(path, "TRR_"));
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TerrainVertex {
            internal Vector3 position;
            internal Vector3 normal;
            internal Vector4 color;
            internal Vector2 texcoor;

            public static readonly int Size = Unsafe.SizeOf<TerrainVertex>();
        }

        public TerrainRenderTechnique()
            : base(new EntityHasSet(typeof(D3DTerrainRenderComponent), typeof(D3DTexturedMaterialComponent))) {
            rasterizerStateDescription = new RasterizerStateDescription() {
                CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false
            };
            // disable depth because SKY dom is close to camera and with correct depth overlap all objects
            depthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled;
        }

        public IRenderTechniquePass GetPass() => pass;

        protected override void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            foreach (var en in entities) {
                var render = en.GetComponent<D3DTerrainRenderComponent>();
                var geo = en.GetComponent<IGeometryComponent>();
                var material = en.GetComponent<D3DTexturedMaterialComponent>();
                var components = en.GetComponents<ID3DRenderable>();

                foreach (var com in components) {
                    if (com.IsModified) {
                        com.Update(graphics);
                    }
                }

                if (render.IsModified) {//update
                    if (!pass.IsCompiled) {
                        pass.Compile(graphics.Compilator);
                    }

                    UpdateShaders(graphics, render, pass, layconst);
                    render.PrimitiveTopology = PrimitiveTopology.TriangleList;

                    render.IsModified = false;
                }

                SetShaders(context, render);

                {// material
                    if (material.IsModified) {
                        render.TextureResources.Set(ConvertToResources(material, graphics.TexturedLoader));
                        render.SampleState.Set(graphics.CreateSampler(material.SampleDescription));
                    }
                    context.PixelShader.SetShaderResources(0, render.TextureResources.Get());
                    context.PixelShader.SetSampler(0, render.SampleState.Get());
                }

                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
                context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

                if (geo.IsModified) {
                    var pos = geo.Positions;
                    var normals = geo.Normals;
                    var vertex = new TerrainVertex[pos.Length];

                    for (var i = 0; i < pos.Length; i++) {
                        vertex[i] = new TerrainVertex {
                            position = pos[i],
                            normal = normals[i],
                            color = geo.Colors[i],
                            texcoor = geo.TextureCoordinates[i]
                        };
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.MarkAsRendered();
                }

                

                Render(graphics, context, render, TerrainVertex.Size);

                foreach (var com in components) {
                    com.Render(graphics);
                }

                graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
            }

            
        }
    }
}
