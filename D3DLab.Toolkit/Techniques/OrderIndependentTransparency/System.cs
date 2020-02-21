using D3DLab.ECS;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques.OrderIndependentTransparency {
    public class OITComponent : D3DRenderComponent {
        public Texture2D UnorderedViewTexture { get; set; }
        public UnorderedAccessView UnorderedView { get; set; }
    }
    public class CameraViewsRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>,
        IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.D3D.CameraViews.camera_views.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static CameraViewsRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ECS.Common.ManifestResourceLoader(typeof(CameraViewsRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CameraViews_"));
        }

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

        public CameraViewsRenderTechnique() : base(new EntityHasSet(
                typeof(OITComponent))) {

        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };


        protected D3DRenderComponent GetRenderComponent(GraphicEntity entity) => entity.GetComponent<OITComponent>();
        protected D3DShaderTechniquePass GetTechniquePass() => pass;
        protected VertexLayoutConstructor GetVertexLayout() => layconst;

        #region render

        protected void UpdateDepthStencil(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {

        }

        protected void UpdateBlendingState(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {

        }

        protected void UpdateGeometryBuffers(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {

        }

        protected void UpdateTransformWorldBuffer(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {

        }

        protected void Draw(GraphicsDevice graphics, GraphicEntity entity) {
            //var geo = entity.GetComponent<IGeometryComponent>();
            //graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
        }

        protected void UpdateConstantBuffers(GraphicsDevice graphics, D3DRenderComponent _base, TProperties properties) {
            var render = (OITComponent)_base;

            graphics.ImmediateContext.ClearUnorderedAccessView(render.UnorderedView,
                new SharpDX.Mathematics.Interop.RawInt4(0, 0, 0, 0));//0xffffffff

            var text = graphics.GetBackBuffer();

            render.UnorderedViewTexture = new Texture2D(graphics.D3DDevice, new Texture2DDescription() {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            });

            render.UnorderedView = new UnorderedAccessView(graphics.D3DDevice, render.UnorderedViewTexture, new UnorderedAccessViewDescription() {
                Format = Format.R8G8B8A8_UNorm,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = { MipSlice = 0 }
            });


        }

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            throw new NotImplementedException();
        }


        #endregion
    }
}
