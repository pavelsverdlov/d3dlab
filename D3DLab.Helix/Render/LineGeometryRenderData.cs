using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	internal class LineGeometryRenderData : GeometryRenderData
	{
		private Buffer vertexBuffer;
		private Buffer indexBuffer;

		public LineGeometryRenderData()
			: base(Techniques.RenderLines)
		{
		}

		public Color4 Color { get; set; }

		protected override void AttachCore(RenderContext renderContext)
		{
			base.AttachCore(renderContext);

			UpdateBuffers(renderContext, RenderSources.ToArrays(), Color);
		}

		private void UpdateBuffers(RenderContext renderContext, RenderArrays geometry, Color4 color)
		{
			DetachGeometry();

			if (geometry.Positions == null || geometry.Positions.Length == 0 ||
				geometry.Indices == null || geometry.Indices.Length == 0)
			{
				return;
			}

			/// --- init vertex buffer
			var colors = geometry.Colors != null ? geometry.Colors : new Color4[0]; 
             var positions = geometry.Positions;
			var vertexCount = positions.Length;
			var result = new LinesVertex[vertexCount];

			for (var i = 0; i < vertexCount; i++)
			{
				result[i] = new LinesVertex
				{
					Position = new Vector4(positions[i], 1f),
					Color = i < colors.Length ? colors[i] : color,
				};
			}
			this.vertexBuffer = renderContext.Device.CreateBuffer(BindFlags.VertexBuffer, LinesVertex.SizeInBytes, result);

			/// --- init index buffer
			this.indexBuffer = renderContext.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices);
		}

		protected override void UpdateRasterState(RenderContext renderContext, int depthBias, CullMode cullMaterialMode)
		{
			UpdateRasterState(renderContext, new RasterizerStateDescription()
			{
				FillMode = FillMode.Solid,
				CullMode = cullMaterialMode,
				DepthBias = depthBias,
				DepthBiasClamp = -1000,
				SlopeScaledDepthBias = -2,
				IsDepthClipEnabled = true,
				IsFrontCounterClockwise = false,

				IsMultisampleEnabled = true,
				IsAntialiasedLineEnabled = true, // Intel HD 3000 doesn't like this (#10051) and it's not needed
				//IsScissorEnabled = true,
			});
		}

		protected override void DetachCore()
		{
			DetachGeometry();
			base.DetachCore();
		}

		private void DetachGeometry()
		{
			Disposer.RemoveAndDispose(ref vertexBuffer);
			Disposer.RemoveAndDispose(ref indexBuffer);
		}

		protected override bool CanRender(RenderContext renderContext)
		{
			if (!base.CanRender(renderContext))
				return false;
			if (this.vertexBuffer == null)
				return false;

			return true;
		}

		public override void DoProcessAttachDetach(RenderContext renderContext)
		{
			base.DoProcessAttachDetach(renderContext);

			if (IsAttached)
			{
				RenderArrays arrays;
				if (RenderSources.CheckForUpdate(out arrays))
					UpdateBuffers(renderContext, arrays, Color);
			}
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			base.RenderCore(renderContext);
			Draw(renderContext, PrimitiveTopology.LineList, this.indexBuffer, this.vertexBuffer, LinesVertex.SizeInBytes);
		}
	}
}
