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
using D3DLab.Toolkit.Techniques.TriangleColored;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.OrderIndependentTransparency {
    public class OITTriangleColoredVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        //const string path = @"D3DLab.Toolkit.D3D.OrderIndependentTransparency.oit_colored_vertex.hlsl";
        const string path = @"D3DLab.Toolkit.D3D.TriangleColored.colored_vertex.hlsl";

        static readonly D3DShaderTechniquePass pass;
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

        static OITTriangleColoredVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(OITTriangleColoredVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CV_"));
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };


        public OITTriangleColoredVertexRenderTechnique()
            : base(new EntityHasSet(
                typeof(D3DTriangleColoredVertexRenderComponent),
                typeof(TransformComponent))) {

            depthStencilStateDesc = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            geometryShader = new DisposableSetter<GeometryShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly DisposableSetter<GeometryShader> geometryShader;

        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;


        #region OIT

        [StructLayout(LayoutKind.Sequential)]
        struct ListNode {
            public uint packedColor;
            public uint depthAndCoverage;
            public uint next;
            public uint temp;
        };

        Texture2D UnorderedViewTexture;
        UnorderedAccessView headBuffer;
        SharpDX.Direct3D11.Buffer buf;
        UnorderedAccessView fragmentsList;

        Texture2D RWTexture2D_V3;
        UnorderedAccessView UnorderedRWTexture2D_V4;
        Texture2D RWTexture2D_V1;
        UnorderedAccessView UnorderedRWTexture2D_V1;

        UnorderedAccessView[] accessViews;


        #endregion

        protected override void Rendering(GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (UnorderedViewTexture == null) {
                var srvFormat = Format.R32_Typeless;
                var uavFormat = Format.R32_Typeless;

                //UnorderedViewTexture = new Texture2D(graphics.D3DDevice, new Texture2DDescription() {
                //    BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource | BindFlags.RenderTarget,
                //    Format = Format.R8G8B8A8_Typeless,//R8G8B8A8_UNorm
                //    Width = graphics.Size.Width,
                //    Height = graphics.Size.Height,
                //    OptionFlags = ResourceOptionFlags.None,
                //    MipLevels = 1,
                //    ArraySize = 1,
                //    Usage = ResourceUsage.Default,
                //    SampleDescription = { Count = 1, Quality = 0 }
                //});

                //headBuffer = new UnorderedAccessView(graphics.D3DDevice, UnorderedViewTexture,
                //    new UnorderedAccessViewDescription() {
                //        Format = Format.R32_UInt,
                //        Dimension = UnorderedAccessViewDimension.Texture2D,
                //        Texture2D = { MipSlice = 0 }
                //    });

                //var i = new ListNode[2];

                //var description = new UnorderedAccessViewDescription() {
                //    Format = Format.R32_Typeless,
                //    Dimension = UnorderedAccessViewDimension.Buffer,
                //    Buffer = {
                //        ElementCount = i.Length,
                //        FirstElement = 0,
                //        Flags = UnorderedAccessViewBufferFlags.Counter
                //    },  
                //};

                //buf = graphics.CreateUnorderedRWStructuredBuffer<ListNode>(ref i);
                //fragmentsList = new UnorderedAccessView(graphics.D3DDevice, buf, description);

            }
            //graphics.ImmediateContext.ClearUnorderedAccessView(UnorderedRWTexture2D_V1,
            //    new SharpDX.Mathematics.Interop.RawInt4(0, 0, 0, 0));//0xffffffff

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));

                if (pass.GeometryShader != null) {
                    geometryShader.Set(new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes()));
                }
                if (pass.PixelShader != null) {
                    pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
                }
            }

            foreach (var en in entities) {
                var render = en.GetComponent<D3DTriangleColoredVertexRenderComponent>();

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc));
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDesc));
                }

                UpdateGeometryBuffers(graphics, render, en);

                if (!vertexShader.HasValue && !render.IndexBuffer.HasValue) {
                    throw RenderTechniqueException.NoVertexAndIndexBuffers;
                }

                ApplyTransformWorldBufferToRenderComp(graphics, render, en.GetComponent<TransformComponent>());

                if (!render.TransformWorldBuffer.HasValue) {
                    throw RenderTechniqueException.NoWorldTransformBuffers;
                }

                context.VertexShader.Set(vertexShader.Get());
                context.GeometryShader.Set(geometryShader.Get());
                context.PixelShader.Set(pixelShader.Get());

                //  context.PixelShader.SetShaderResource(1, UnorderedRWTexture2D_V1);

                UpdateConstantBuffers(graphics, render, props);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(), 
                    layconst.VertexSize, 0));
                context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

                context.InputAssembler.InputLayout = inputLayout.Get();
                context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;


                //OMSetRenderTargetsAndUnorderedAccessViews( 0, NULL, m_pDSView, 1, 3, pUAVs, NULL );
                //context.OutputMerger.ResetTargets();
                //context.OutputMerger.GetRenderTargets(out var depthStencilViewRef);
                //context.OutputMerger.SetTargets(depthStencilViewRef, 0, new RenderTargetView[0]);

                context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                context.OutputMerger.SetBlendState(render.BlendingState.Get(), new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

                //context.OutputMerger.SetUnorderedAccessViews(1, accessViews);

                //context.OutputMerger.SetUnorderedAccessView(1, UnorderedRWTexture2D_V4);
                //context.OutputMerger.SetUnorderedAccessView(2, UnorderedRWTexture2D_V1);

                using (var rasterizerState = new RasterizerState2(graphics.D3DDevice, render.RasterizerStateDescription.GetDescription())) {
                    context.Rasterizer.State = rasterizerState;

                    var geo = en.GetComponent<IGeometryComponent>();
                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }
        }
        void UpdateGeometryBuffers(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {
            var geo = entity.GetComponent<IGeometryComponent>();
            var color = entity.GetComponents<ColorComponent>();

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
        void UpdateConstantBuffers(GraphicsDevice graphics, D3DRenderComponent render, TProperties properties) {
            var context = graphics.ImmediateContext;

            //context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, properties.Game);
            ////context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, properties.Lights);

            //context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, properties.Game);
            //context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, properties.Lights);

            //if (render.TransformWorldBuffer.HasValue) {
            //    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());
            //}


            context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, properties.Game);
            context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, properties.Lights);

            if (render.TransformWorldBuffer.HasValue) {
                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());
            }
        }
    }
}
