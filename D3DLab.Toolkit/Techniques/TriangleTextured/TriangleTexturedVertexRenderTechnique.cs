using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.TriangleTextured {

    public class TriangleTexturedVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.TriangleTextured.textured_vertex.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;
        static readonly ShaderDebugMode debug;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector2 TexCoor;
            public Vertex(Vector3 position, Vector3 normal, Vector2 texCoor) {
                Position = position;
                Normal = normal;
                TexCoor = texCoor;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        static TriangleTexturedVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddTexCoorElementAsVector2();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(TriangleTexturedVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "TV_"));
            //debug = new ShaderDebugMode(new DirectoryInfo(@"D:\Storage_D\trash\archive\shaders_tex\"), pass);
            //debug.Activate();
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

        public TriangleTexturedVertexRenderTechnique()
            : base(new EntityHasSet(
                typeof(D3DTriangleTexturedVertexRenderComponent),
                typeof(TransformComponent))) {

            depthStencilStateDesc = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;

        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;


        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));

                if (pass.PixelShader != null) {
                    pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
                }
            }

            foreach (var en in entities) {
                //mandatory
                var render = en.GetComponent<D3DTriangleTexturedVertexRenderComponent>();
                var geo = en.GetComponent<GeometryComponent>();
                var transform = en.GetComponent<TransformComponent>();
                //optional
                var colors = en.GetComponents<MaterialColorComponent>();
                var textures = en.GetComponents<D3DTexturedMaterialSamplerComponent>();

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc));
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDesc));
                }

                ApplyTransformWorldBufferToRenderComp(graphics, render, transform);

                if (geo.IsModified) {
                    var vertex = new Vertex[geo.Positions.Length];
                    for (var index = 0; index < vertex.Length; index++) {
                        vertex[index] = new Vertex(
                            geo.Positions[index], geo.Normals[index], geo.TexCoor[index]);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.IsModified = false;
                }

                if (colors.Any()) {
                    var color = colors.Single();
                    var material = new MaterialStructBuffer();
                    material.SpecularFactor = color.SpecularFactor;

                    if (render.MaterialBuffer.HasValue) {
                        var buff = render.MaterialBuffer.Get();
                        graphics.UpdateDynamicBuffer(ref material, buff, MaterialStructBuffer.RegisterResourceSlot);
                    } else {
                        var buff = graphics.CreateDynamicBuffer(ref material, Unsafe.SizeOf<MaterialStructBuffer>());
                        render.MaterialBuffer.Set(buff);
                    }
                }

                if (textures.Any(x => x.IsModified)) {
                    var texture = textures.Single();

                    render.TextureResources.Set(ConvertToResources(texture, graphics.TexturedLoader));
                    render.SampleState.Set(graphics.CreateSampler(texture.SampleDescription));
                    texture.IsModified = false;
                }

                {
                    context.VertexShader.Set(vertexShader.Get());
                    context.PixelShader.Set(pixelShader.Get());

                    context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, 
                        render.TransformWorldBuffer.Get());
                    
                    context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                    context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, game.Lights);
                    if (render.MaterialBuffer.HasValue) {
                        context.PixelShader.SetConstantBuffer(MaterialStructBuffer.RegisterResourceSlot, 
                            render.MaterialBuffer.Get());
                    }
                    if (render.TextureResources.HasValue && render.SampleState.HasValue) {
                        context.PixelShader.SetShaderResources(0, render.TextureResources.Get());
                        context.PixelShader.SetSampler(0, render.SampleState.Get());
                    }
                }

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                    layconst.VertexSize, 0));
                context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

                context.InputAssembler.InputLayout = inputLayout.Get();
                context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;

                context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                context.OutputMerger.SetBlendState(render.BlendingState.Get(), new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

                using (var rasterizerState = new RasterizerState2(graphics.D3DDevice, render.RasterizerStateDescription.GetDescription())) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }

        }

        public override void CleanupRenderCache() {
            pass.ClearCache();
            base.CleanupRenderCache();
        }
    }
}
