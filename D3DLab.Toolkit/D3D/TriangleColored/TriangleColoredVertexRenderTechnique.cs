using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.D3D.TriangleColored {
    public class TriangleColoredVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.D3D.TriangleColored.colored_vertex.hlsl";
        const string gs_flat_shading = @"D3DLab.Toolkit.D3D.TriangleColored.gs_flat_shading.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly D3DShaderTechniquePass flatShadingPass;
        static readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector4 Color;
            public Vertex(Vector3 position, Vector3 normal, Vector4 color) {
                Position = position;
                Normal = normal;
                Color = color;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        static TriangleColoredVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(typeof(TriangleColoredVertexRenderTechnique<>));
            pass = new D3DShaderTechniquePass(d.Load(path, "CV_"));
            flatShadingPass = new D3DShaderTechniquePass(d.Load(gs_flat_shading, "FCV_"));
        }

        public IRenderTechniquePass GetPass() => pass;


        public TriangleColoredVertexRenderTechnique()
            : base(new EntityHasSet(
                typeof(D3DTriangleColoredVertexRenderComponent),
                typeof(TransformComponent))) {

            depthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled;


            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            flatShadingGeometryShader = new DisposableSetter<GeometryShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly DisposableSetter<GeometryShader> flatShadingGeometryShader;

        protected override void Rendering(GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                flatShadingPass.Compile(graphics.Compilator);

                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));

                if (flatShadingPass.GeometryShader != null) {
                    flatShadingGeometryShader.Set(new GeometryShader(device, flatShadingPass.GeometryShader.ReadCompiledBytes()));
                }
                if (pass.PixelShader != null) {
                    pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
                }
            }

            foreach (var en in entities) {
                var render = en.GetComponent<D3DTriangleColoredVertexRenderComponent>();

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDescription));
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDescription));
                }

                UpdateGeometryBuffers(graphics, render, en);

                if (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue) {
                    throw RenderTechniqueException.NoVertexAndIndexBuffers;
                }

                ApplyTransformWorldBufferToRenderComp(graphics, render, en.GetComponent<TransformComponent>());

                if (!render.TransformWorldBuffer.HasValue) {
                    throw RenderTechniqueException.NoWorldTransformBuffers;
                }

                context.VertexShader.Set(vertexShader.Get());
                if (en.Has<GeometryFlatShadingComponent>()) {
                    context.GeometryShader.Set(flatShadingGeometryShader.Get());
                } else {
                    context.GeometryShader.Set(null);
                }
                context.PixelShader.Set(pixelShader.Get());

                UpdateConstantBuffers(graphics, render, props);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                    layconst.VertexSize, 0));
                context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

                context.InputAssembler.InputLayout = inputLayout.Get();
                context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;

                context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                context.OutputMerger.SetBlendState(render.BlendingState.Get(), new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

                using (var rasterizerState = new RasterizerState2(graphics.D3DDevice, render.RasterizerStateDescription.GetDescription())) {
                    context.Rasterizer.State = rasterizerState;

                    var geo = en.GetComponent<IGeometryComponent>();
                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }

        }
        void UpdateConstantBuffers(GraphicsDevice graphics, D3DRenderComponent render, TProperties properties) {
            var context = graphics.ImmediateContext;

            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, properties.Game);
            context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());

            context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, properties.Game);
            context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, properties.Lights);
        }
        void UpdateGeometryBuffers(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {
            var geo = entity.GetComponent<IGeometryComponent>();
            var color = entity.GetComponents<ColorComponent>();

            //if (geo.IsModified || color.Any(x => x.IsModified)) {
            if (geo.IsModified || color.Any(x => x.IsModified) || (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue)) {
                var vertex = new Vertex[geo.Positions.Length];
                if (color.Any()) {
                    var c = color.Single();
                    for (var index = 0; index < vertex.Length; index++) {
                        vertex[index] = new Vertex(
                            geo.Positions[index], geo.Normals[index], c.Color);
                    }
                } else {
                    for (var index = 0; index < vertex.Length; index++) {
                        vertex[index] = new Vertex(
                            geo.Positions[index], geo.Normals[index], geo.Colors[index]);
                    }
                }

                render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                geo.MarkAsRendered();
            }
        }

    }
}
