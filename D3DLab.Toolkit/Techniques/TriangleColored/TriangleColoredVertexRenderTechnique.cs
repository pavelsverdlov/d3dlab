using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.TriangleColored {
    public class TriangleColoredVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.TriangleColored.colored_vertex.hlsl";
        const string gs_flat_shading = @"D3DLab.Toolkit.Techniques.TriangleColored.gs_flat_shading.hlsl";
        const string wireframe = @"D3DLab.Toolkit.Techniques.TriangleColored.wireframe.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly D3DShaderTechniquePass flatShadingPass;
        static readonly D3DShaderTechniquePass wireframePass;
        static readonly VertexLayoutConstructor layconst;
        //static readonly ShaderDebugMode debug1;
        //static readonly ShaderDebugMode debug2;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public Vertex(Vector3 position, Vector3 normal) {
                Position = position;
                Normal = normal;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        static TriangleColoredVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(TriangleColoredVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CV_"));
            flatShadingPass = new D3DShaderTechniquePass(d.Load(gs_flat_shading, "FCV_"));
            wireframePass = new D3DShaderTechniquePass(d.Load(wireframe, "WCV_"));

            //debug1 = new ShaderDebugMode(new DirectoryInfo(@"D:\Storage_D\trash\archive\shaders\"), pass);
            //debug1.Activate();
            //debug2 = new ShaderDebugMode(new DirectoryInfo(@"D:\Storage_D\trash\archive\shaders\"), flatShadingPass);
            //debug2.Activate();
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass, flatShadingPass, wireframePass };


        public TriangleColoredVertexRenderTechnique()
            : base(new EntityHasSet(
                typeof(D3DTriangleColoredVertexRenderComponent),
                typeof(TransformComponent))) {

            depthStencilStateDesc = D3DDepthStencilStateDescriptions.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            flatShadingGS = new DisposableSetter<GeometryShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);

            wireframePS = new DisposableSetter<PixelShader>(disposer);
            wireframeGS = new DisposableSetter<GeometryShader>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;

        readonly DisposableSetter<GeometryShader> flatShadingGS;

        readonly DisposableSetter<PixelShader> wireframePS;
        readonly DisposableSetter<GeometryShader> wireframeGS;

        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;

        protected override void Rendering(GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;
            
            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            if (!flatShadingPass.IsCompiled) {
                flatShadingPass.Compile(graphics.Compilator);
                flatShadingGS.Set(new GeometryShader(device, flatShadingPass.GeometryShader.ReadCompiledBytes()));
            }

            if (!wireframePass.IsCompiled) {
                wireframePass.Compile(graphics.Compilator);
                wireframeGS.Set(new GeometryShader(device, wireframePass.GeometryShader.ReadCompiledBytes()));
                wireframePS.Set(new PixelShader(device, wireframePass.PixelShader.ReadCompiledBytes()));
            }

            //clear shaders off prev. technique 
            graphics.ClearAllShader();

            {//set shaders that shader for all entities
                context.VertexShader.Set(vertexShader.Get());
            }

            foreach (var en in entities) {
                var render = en.GetComponent<D3DTriangleColoredVertexRenderComponent>();
                var geo = en.GetComponent<GeometryComponent>();
                var colors = en.GetComponents<MaterialColorComponent>();
                var transform = en.GetComponent<TransformComponent>();

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc));
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDesc));
                }

                {
                    if (en.Has<GeometryFlatShadingComponent>()) {
                        context.GeometryShader.Set(null);
                        context.PixelShader.Set(null);
                        context.GeometryShader.Set(flatShadingGS.Get());
                        context.PixelShader.Set(pixelShader.Get());
                    } else if (en.Has<WireframeGeometryComponent>()) {
                        context.GeometryShader.Set(null);
                        context.PixelShader.Set(null);
                        context.GeometryShader.Set(wireframeGS.Get());
                        context.PixelShader.Set(wireframePS.Get());
                    } else {
                        context.PixelShader.Set(null);
                        context.PixelShader.Set(pixelShader.Get());
                    }
                }

                if (geo.IsModified || (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue)) {
                    var vertex = new Vertex[geo.Positions.Length];
                    for (var index = 0; index < vertex.Length; index++) {
                        vertex[index] = new Vertex(geo.Positions[index], geo.Normals[index]);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.IsModified = false;
                }
                if (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue) {
                    throw RenderTechniqueException.NoVertexAndIndexBuffers;
                }

                if (colors.Any()) {
                    var color = colors.Single();

                    var material = MaterialStructBuffer.From(color);

                    if (render.MaterialBuffer.HasValue) {
                        var buff = render.MaterialBuffer.Get();
                        graphics.UpdateDynamicBuffer(ref material, buff, MaterialStructBuffer.RegisterResourceSlot);
                    } else {
                        var buff = graphics.CreateDynamicBuffer(ref material, Unsafe.SizeOf<MaterialStructBuffer>());
                        render.MaterialBuffer.Set(buff);
                    }
                }

                ApplyTransformWorldBufferToRenderComp(graphics, render, transform);

                if (!render.TransformWorldBuffer.HasValue) {
                    throw RenderTechniqueException.NoWorldTransformBuffers;
                }

                {//Update constant buffers
                    context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());

                    context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, props.Lights);
                    if (render.MaterialBuffer.HasValue) {
                        context.PixelShader.SetConstantBuffer(MaterialStructBuffer.RegisterResourceSlot, render.MaterialBuffer.Get());
                    }
                }
                {
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                        layconst.VertexSize, 0));
                    context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

                    context.InputAssembler.InputLayout = inputLayout.Get();
                    context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;

                    context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                    context.OutputMerger.SetBlendState(render.BlendingState.Get(), new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
                }

                var stated = render.RasterizerStateDescription.GetDescription();
                using (var rasterizerState = new RasterizerState2(graphics.D3DDevice, stated)) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }

        }

        public override void CleanupRenderCache() {
            pass.ClearCache();
            flatShadingPass.ClearCache();
            wireframePass.ClearCache();
            base.CleanupRenderCache();
        }

    }
}
