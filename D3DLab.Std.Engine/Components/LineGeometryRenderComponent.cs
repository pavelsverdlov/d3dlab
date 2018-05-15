using D3DLab.Std.Engine.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using D3DLab.Std.Engine.Core.Shaders;
using System.Runtime.InteropServices;

namespace D3DLab.Std.Engine.Components {
    public class LineGeometryRenderComponent : ShaderComponent, IRenderableComponent {
        readonly Geometry3D geometry;

        DeviceBuffer _indexBuffer;
        DeviceBuffer _vertexBuffer;

        DeviceBuffer _worldBuffer;
        DeviceBuffer _projectionBuffer;
        DeviceBuffer _viewBuffer;

        ResourceLayout projViewLayout;
        ResourceSet _projViewSet;

        public LineGeometryRenderComponent(IShaderInfo[] shaders, Geometry3D geometry) : base(shaders) {
            this.geometry = geometry;
        }

        public override VertexLayoutDescription[] GetLayoutDescription() {
            return new[] {
                new VertexLayoutDescription(
                        new VertexElementDescription("p", VertexElementSemantic.Position, VertexElementFormat.Float4),
                        new VertexElementDescription("c", VertexElementSemantic.Color, VertexElementFormat.Float4))
            };
        }

        public void Update(RenderState state) {
            var cmd = state.Commands;
            var factory = state.factory;
            //
            if (!TechniquePass.IsCached) {
                UpdateShader(factory);
            }
            //
            factory.CreateIfNullBuffer(ref _projectionBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));
            factory.CreateIfNullBuffer(ref _viewBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));
            factory.CreateIfNullBuffer(ref _worldBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));

            cmd.UpdateBuffer(_projectionBuffer, 0, state.Viewport.ProjectionMatrix);
            cmd.UpdateBuffer(_viewBuffer, 0, state.Viewport.ViewMatrix);
            cmd.UpdateBuffer(_worldBuffer, 0, Matrix4x4.Identity);
            //
            var vertices = ConvertVertexToShaderStructure(geometry);
            factory.CreateIfNullBuffer(ref _vertexBuffer, new BufferDescription((uint)(LinesVertex.SizeInBytes * vertices.Length),
                BufferUsage.VertexBuffer));
            cmd.UpdateBuffer(_vertexBuffer, 0, vertices);

            ushort[] indices = ConvertToShaderIndices(geometry);//GetCubeIndices();
            factory.CreateIfNullBuffer(ref _indexBuffer, new BufferDescription(sizeof(ushort) * (uint)indices.Length,
                BufferUsage.IndexBuffer));
            cmd.UpdateBuffer(_indexBuffer, 0, indices);
            //
            if (projViewLayout == null) {
                projViewLayout = factory.CreateResourceLayout(
                  new ResourceLayoutDescription(
                      new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                      new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                      new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex)));
            }
            if (_projViewSet != null) {
                _projViewSet.Dispose();
            }
            _projViewSet = factory.CreateResourceSet(new ResourceSetDescription(
                   projViewLayout,
                   _projectionBuffer,
                   _viewBuffer,
                   _worldBuffer));
        }

        public void Render(RenderState state) {
            var cmd = state.Commands;
            var factory = state.factory;
            var gd = state.gd;
            var _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                    BlendStateDescription.SingleOverrideBlend,
                    DepthStencilStateDescription.Disabled,
                    RasterizerStateDescription.CullNone,
                    PrimitiveTopology.LineList,//LineList TriangleList
                    ShaderSetDesc,
                    new[] { projViewLayout },
                    gd.SwapchainFramebuffer.OutputDescription));

            cmd.SetPipeline(_pipeline);
            cmd.SetGraphicsResourceSet(0, _projViewSet);
            cmd.SetVertexBuffer(0, _vertexBuffer);
            cmd.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cmd.DrawIndexed((uint)geometry.Indices.Count, 1, 0, 0, 0);
        }

        LinesVertex[] ConvertVertexToShaderStructure(Geometry3D geo) {
            var res = new List<LinesVertex>();

            for (int i = 0; i < geo.Positions.Count; i++) {
                var pos = geo.Positions[i];
                res.Add(new LinesVertex() { Position = new Vector4(pos.X, pos.Y, pos.Z, 1), Color = RgbaFloat.Red.ToVector4() });//
            }

            return res.ToArray();
        }
        ushort[] ConvertToShaderIndices(Geometry3D geo) {
            return geo.Indices.Select(x => (ushort)x).ToArray();
        }



        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct LinesVertex {
            public Vector4 Position;
            public Vector4 Color;
            public const int SizeInBytes = 4 * (4 + 3);
        }
    }
}
