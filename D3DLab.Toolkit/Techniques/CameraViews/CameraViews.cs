using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit.Components;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.CameraViews {
    struct CameraViewsComponent : IGraphicComponent {

        public static CameraViewsComponent Create() {
            return new CameraViewsComponent {
                Size = 0.15f,
                StencilStateDescription = D3DDepthStencilStateDescriptions.DepthDisabled,
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled
            };
        }

        public ElementTag Tag { get; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public float Size;
        public DepthStencilStateDescription StencilStateDescription;
        public BlendStateDescription BlendStateDescription;
    }

    class CameraViewsRenderComponent : D3DRenderComponent {
        public CameraViewsRenderComponent() {
            
            PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            var rasterizerStateDescription = new RasterizerStateDescription2() {
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
            var d = new RasterizerStateDescription() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,

                IsFrontCounterClockwise = false,
                IsScissorEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                SlopeScaledDepthBias = .0f
            };
            RasterizerStateDescription = new D3DRasterizerState(rasterizerStateDescription);
        }
    }

    public class CameraViewsRenderTechnique<TProperties>
        : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
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

        public CameraViewsRenderTechnique() { }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };


        protected D3DRenderComponent GetRenderComponent(GraphicEntity entity)=> entity.GetComponent<CameraViewsRenderComponent>();
        protected D3DShaderTechniquePass GetTechniquePass() => pass;
        protected VertexLayoutConstructor GetVertexLayout() => layconst;

        #region render

        protected void UpdateDepthStencil(GraphicsDevice graphics, D3DRenderComponent buffers, ref RenderableComponent renderable) {
            if (!buffers.DepthStencilState.HasValue && renderable.HasDepthStencil) {
                buffers.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, 
                    renderable.DepthStencilStateDescription));
            }
        }
        protected void RenderDepthStencil(GraphicsDevice graphics, D3DRenderComponent buffers) {
            if (buffers.DepthStencilState.HasValue) {
                graphics.ImmediateContext.OutputMerger
                    .SetDepthStencilState(buffers.DepthStencilState.Get(), 0);
            }
        }


        protected void UpdateBlendingState(GraphicsDevice graphics, D3DRenderComponent buffers, ref RenderableComponent renderable) {
            if (!buffers.BlendingState.HasValue && renderable.HasBlendState) {
                buffers.BlendingState.Set(
                    new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
            }
        }
        protected void RenderBlendingState(GraphicsDevice graphics, D3DRenderComponent buffers) {
            if (buffers.BlendingState.HasValue) {
                graphics.ImmediateContext.OutputMerger
                    .SetBlendState(buffers.BlendingState.Get(),
                        new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
            }
        }






        protected void UpdateGeometryBuffers(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {
            var geo = entity.GetComponent<GeometryComponent>();
            var color = entity.GetComponent<ColorComponent>();

            if (geo.IsModified) {

                var vertex = new Vertex[geo.Positions.Length];
                var c = color.Color;
                for (var index = 0; index < vertex.Length; index++) {
                    vertex[index] = new Vertex(geo.Positions[index], geo.Normals[index], c);
                }

                render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

            }
        }

        void UpdateTransformWorldBuffer(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity entity) {
            //var cameraPostion = frameProperties.CameraState.Position +
            //       frameProperties.CameraState.LookDirection * 2f;
            //var worldMatrix = Matrix4x4.CreateTranslation(cameraPostion.X, cameraPostion.Y, cameraPostion.Z);

            //var tr = TransformComponent.Create(worldMatrix);

            //ApplyTransformWorldBufferToRenderComp(graphics, render, tr);
        }
        
        protected void Draw(GraphicsDevice graphics, GraphicEntity entity) {
            var geo = entity.GetComponent<GeometryComponent>();
            graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
        }

        protected void UpdateConstantBuffers(GraphicsDevice graphics, D3DRenderComponent render, TProperties properties) {
           
        }

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            throw new NotImplementedException();
        }

        public override bool IsAplicable(GraphicEntity entity) {
            throw new NotImplementedException();
        }


        #endregion
    }

    public class CameraViewsObject {
        public static CameraViewsObject Create(IEntityManager manager) {

            throw new NotImplementedException();

            var cvcom = CameraViewsComponent.Create();

            //var halfSize = cvcom.Size * 0.5f;
            //var boxgeo = GeometryBuilder.BuildGeoBox(new AxisAlignedBox(new Vector3(-halfSize, -halfSize, -halfSize), new Vector3(halfSize, halfSize, halfSize)));
            
            //var move = Matrix4x4.CreateTranslation(new Vector3(1, 0, 0));
            //var geoc = new SimpleGeometryComponent();
            //geoc.Positions = boxgeo.Positions.ToArray()
            //   /// .Transform(ref move)
            //    .ToImmutableArray();
            //geoc.Indices = boxgeo.Indices.ToImmutableArray();
            //geoc.Normals = boxgeo.Positions.CalculateNormals(boxgeo.Indices).ToImmutableArray();

            

            //manager.CreateEntity(new ElementTag("CameraViews"))
            //    .AddComponents(
            //        new CameraViewsRenderComponent(),
            //        cvcom,
            //        geoc,
            //        ColorComponent.CreateAmbient(V4Colors.Blue).ApplyOpacity(0.2f)
            //    );


            return new CameraViewsObject();
        }
    }

}
