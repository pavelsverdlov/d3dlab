using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
using D3DLab.Toolkit;
using D3DLab.Toolkit.Components;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace D3DLab.Toolkit.Techniques.Line {
    public class LineVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.Line.Lines.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector4 Position;
            public readonly Vector4 Color;

            public Vertex(Vector3 position, Vector4 color) {
                Position = new Vector4(position, 1);
                Color = color;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        static LineVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector4()
               .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ECS.Common.ManifestResourceLoader(typeof(LineVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "LV_"));
        }

        public LineVertexRenderTechnique()
            : base(new EntityHasSet(
                typeof(LineGeometryComponent),
                typeof(D3DLineVertexRenderComponent),
                typeof(TransformComponent))) {

            depthStencilStateDesc = D3DDepthStencilStateDescriptions.DepthEnabled;
            //  depthStencilStateDesc.IsStencilEnabled = false;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateDisabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;
        readonly DisposableSetter<InputLayout> inputLayout;


        IEnumerable<IRenderTechniquePass> IRenderTechnique<TProperties>.GetPass() => new[] { pass };

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

                inputLayout.Set(graphics.CreateInputLayout(vertexShaderByteCode, layconst.ConstuctElements()));

                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            { //clear shaders off prev. technique 
                graphics.ClearAllShader();
                //all shaders shared for all entity with LineVertexRenderComponent
                context.VertexShader.Set(vertexShader.Get());
                context.PixelShader.Set(pixelShader.Get());
            
                //Update constant buffers ones becase shaders will not changed
                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                //shared for all entity
                context.InputAssembler.InputLayout = inputLayout.Get();
            }

            foreach (var en in entities) {
                var render = en.GetComponent<D3DLineVertexRenderComponent>();
                var geo = en.GetComponent<LineGeometryComponent>();
                var material = en.GetComponent<MaterialColorComponent>();
                var transform = en.GetComponent<TransformComponent>();

                if (!render.DepthStencilState.HasValue) { // can be cached 
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc));
                }

                if (!render.BlendingState.HasValue) {// can be cached 
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDesc));
                }

                ApplyTransformWorldBufferToRenderComp(graphics, render, transform);

                if (geo.IsModified) {
                    var pos = geo.Positions;
                    var color = material.Diffuse;

                    var vertex = new Vertex[pos.Length];
                    for (var i = 0; i < pos.Length; i++) {
                        vertex[i] = new Vertex(pos[i], color);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    geo.IsModified = false;
                }
                
                {
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot,
                      render.TransformWorldBuffer.Get());
                }

                {
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                        layconst.VertexSize, 0));

                    
                    context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;

                    context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                    context.OutputMerger.SetBlendState(render.BlendingState.Get(),
                        new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
                }

                using (var rasterizerState = new RasterizerState2(graphics.D3DDevice,
                            render.RasterizerStateDescription.GetDescription())) {
                    context.Rasterizer.State = rasterizerState;
                    graphics.ImmediateContext.Draw(geo.Positions.Length, 0);
                }
            }
        }

        public override void CleanupRenderCache() {
            pass.ClearCache();
            base.CleanupRenderCache();
        }
    }
}
