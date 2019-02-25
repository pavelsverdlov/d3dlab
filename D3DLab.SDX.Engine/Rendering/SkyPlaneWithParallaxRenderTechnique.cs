using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace D3DLab.SDX.Engine.Rendering {
    class SkyGradientColoringRenderTechnique : RenderTechniqueSystem, IRenderTechniqueSystem {
        const string path = @"D3DLab.SDX.Engine.Rendering.Shaders.Custom.sky.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public Vertex(Vector3 position) {
                Position = position;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct GradientBuffer {
            public Vector4 apexColor;
            public Vector4 centerColor;
        }


        static SkyGradientColoringRenderTechnique() {
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3();

            var d = new CombinedShadersLoader();
            pass = new D3DShaderTechniquePass(d.Load(path, "SKY_"));
        }

        public SkyGradientColoringRenderTechnique()
            : base(new EntityHasSet(typeof(D3DSkyRenderComponent))) {
            rasterizerStateDescription = new RasterizerStateDescription() {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = .0f
            };
            // disable depth because SKY dom is close to camera and with correct depth overlap all objects
            depthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthDisabled;
            blendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled;
        }

        public IRenderTechniquePass GetPass() => pass;

        protected override void Rendering(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            foreach (var en in entities) {
                var render = en.GetComponent<D3DSkyRenderComponent>();
                var gradient = en.GetComponent<GradientMaterialComponent>();
                var geo = en.GetComponent<IGeometryComponent>();
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

                if (gradient.IsModified) {
                    var str = new GradientBuffer {
                        apexColor = gradient.Apex,
                        centerColor = gradient.Center
                    };
                    render.GradientBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref str);
                    gradient.IsModified = false;
                }

                context.PixelShader.SetConstantBuffer(0, render.GradientBuffer);
                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);

                if (geo.IsModified) {
                    var vertex = new Vertex[geo.Positions.Length];
                    for (var i = 0; i < geo.Positions.Length; i++) {
                        vertex[i] = new Vertex(geo.Positions[i]);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.MarkAsRendered();
                }

                Render(graphics, context, render, Vertex.Size);

                foreach (var com in components) {
                    com.Render(graphics);
                }

                graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
            }
        }
    }

    class SkyPlaneWithParallaxRenderTechnique : RenderTechniqueSystem, IRenderTechniqueSystem {

        const string path = @"D3DLab.SDX.Engine.Rendering.Shaders.Custom.skyplane.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector2 Tex;
            public Vertex(Vector3 position, Vector2 tex) {
                Position = position;
                Tex = tex;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ParallaxAnimationParams {
            internal Vector2 firstTranslation;
            internal Vector2 secondTranslation;
            internal float brightness;
            internal Vector3 padding;
        }

        static SkyPlaneWithParallaxRenderTechnique() {
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3()
               .AddTexCoorElementAsVector2();

            var d = new CombinedShadersLoader();
            pass = new D3DShaderTechniquePass(d.Load(path, "SKYPL_"));
        }

        public SkyPlaneWithParallaxRenderTechnique() 
            : base(new EntityHasSet(typeof(D3DSkyPlaneRenderComponent),typeof(SkyPlaneParallaxAnimationComponent))){
            rasterizerStateDescription = new RasterizerStateDescription() {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None, //Back
                DepthBias = 0,
                DepthBiasClamp = .0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = .0f
            };
            // Enable additive blending so the clouds blend with the sky dome color.
            blendStateDescription = new BlendStateDescription();
            blendStateDescription.RenderTarget[0].IsBlendEnabled = true;
            blendStateDescription.RenderTarget[0].SourceBlend = BlendOption.One;
            blendStateDescription.RenderTarget[0].DestinationBlend = BlendOption.One;
            blendStateDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            depthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthDisabled;
        }

        public IRenderTechniquePass GetPass() => pass;

        protected override void Rendering(GraphicsDevice graphics,
            SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            foreach (var en in entities) {
                var render = en.GetComponent<D3DSkyPlaneRenderComponent>();
                var geo = en.GetComponent<IGeometryComponent>();
                var animation = en.GetComponent<SkyPlaneParallaxAnimationComponent>();
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

                //render
                SetShaders(context, render);

                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, gameDataBuffer);
                //context.VertexShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, lightDataBuffer);

                //update animate buff
                if (animation.IsModified) {
                    var ani = new ParallaxAnimationParams {
                        firstTranslation = animation.translate1,
                        secondTranslation = animation.translate2,
                        brightness = 1,
                        padding = Vector3.One
                    };
                    if (render.ParallaxAnimation == null) {
                        render.ParallaxAnimation = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref ani);
                    } else {
                        graphics.UpdateSubresource(ref ani, render.ParallaxAnimation, 0);
                    }
                }

                context.PixelShader.SetConstantBuffer(0, render.ParallaxAnimation);

                if (geo.IsModified) {
                    var vertex = new Vertex[geo.Positions.Length];
                    for (var i = 0; i < geo.Positions.Length; i++) {
                        vertex[i] = new Vertex(geo.Positions[i], geo.TextureCoordinates[i]);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.MarkAsRendered();
                }

                Render(graphics, context, render, Vertex.Size);

                foreach (var com in components) {
                    com.Render(graphics);
                }

                graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
            }



        }
    }
}
