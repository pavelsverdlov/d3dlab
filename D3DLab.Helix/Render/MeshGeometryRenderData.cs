using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace HelixToolkit.Wpf.SharpDX.Render
{
   


    public class MeshGeometryRenderData : MaterialGeometryRenderData
	{
		private global::SharpDX.Direct3D11.Buffer vertexBuffer;
		private global::SharpDX.Direct3D11.Buffer indexBuffer;

		public MeshGeometryRenderData(RenderTechnique _renderTechnique)
			: base( _renderTechnique)
		{
		}

		public bool DisableBlandDiffuseColors { get; set; }

		protected override void AttachCore(RenderContext renderContext)
		{
			base.AttachCore(renderContext);
            //if (renderContext.IsMakingPhoto)
				UpdateBuffers(renderContext, RenderSources.ToArrays(), TextureCoordScale);
            //if (updateTask != null)
			//	updateTask.Wait();
			//updateTask = null;
		}

		//Task updateTask;
		protected override void DoStartUpdate(RenderArrays obj)
		{
			//updateTask = Task.Run(() =>
			//{
			//	UpdateBuffers(this.host.RenderContext, RenderSources.ToArrays(), TextureCoordScale);
			//});
		}

	    public void UpdateBuffers(SharpDevice device, RenderArrays geometry, Vector2 texScale) {
            DetachGeometry();

            if (geometry.Positions == null || geometry.Positions.Length == 0 ||
                geometry.Indices == null || geometry.Indices.Length == 0) {
                return;
            }

            /// --- init vertex buffer
            var colors = geometry.Colors != null && geometry.Colors.Length >= geometry.Positions.Length ? geometry.Colors : null;
            var textureCoordinates = (geometry.TextureCoordinates != null && geometry.TextureCoordinates.Length >= geometry.Positions.Length) ? geometry.TextureCoordinates : null;
            var normals = geometry.Normals != null ? geometry.Normals : null;
            //var tangents = geometry.Tangents != null ? geometry.Tangents : null;
            //var bitangents = geometry.BiTangents != null ? geometry.BiTangents : null;
            var positions = geometry.Positions;
            var vertexCount = positions.Length;
            var result = new DefaultVertex[vertexCount];
            CopyArrays(texScale, colors, textureCoordinates, normals, positions, 0, positions.Length, result);
            
            /// --- init index buffer
            this.indexBuffer = device.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices);

            //Task.WaitAll(tasks.ToArray());
            this.vertexBuffer = device.Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, result);
        }


        unsafe private void UpdateBuffers(RenderContext renderContext, RenderArrays geometry, Vector2 texScale)
		{
			DetachGeometry();

			if (geometry.Positions == null || geometry.Positions.Length == 0 ||
				geometry.Indices == null || geometry.Indices.Length == 0)
			{
				return;
			}

			/// --- init vertex buffer
			var colors = geometry.Colors != null && geometry.Colors.Length >= geometry.Positions.Length ? geometry.Colors : null;
			var textureCoordinates = (geometry.TextureCoordinates != null && geometry.TextureCoordinates.Length >= geometry.Positions.Length) ? geometry.TextureCoordinates : null;
			var normals = geometry.Normals != null ? geometry.Normals : null;
			//var tangents = geometry.Tangents != null ? geometry.Tangents : null;
			//var bitangents = geometry.BiTangents != null ? geometry.BiTangents : null;
			var positions = geometry.Positions;
			var vertexCount = positions.Length;
			var result = new DefaultVertex[vertexCount];
            /*
			int cnt = Environment.ProcessorCount - 1;
			List<Task> tasks = new List<Task>();
			if (cnt > 1 && positions.Length > 20000)
			{
				int sizePart = positions.Length / cnt;
				for (int i = 0; i < cnt; i++)
				{
					int _start = i * sizePart;
					int _size = (i == cnt - 1)
						? positions.Length - _start
						: sizePart;
					var t = Task.Run(() =>
					{
						CopyArrays(texScale, colors, textureCoordinates, normals, positions, _start, _size, result);
					});
					tasks.Add(t);
				}
			}
			else
			{
				var t = Task.Run(() =>
						{
            */
					CopyArrays(texScale, colors, textureCoordinates, normals, positions, 0, positions.Length, result);
            /*
				});
				tasks.Add(t);
			}
            */
			/// --- init index buffer
			this.indexBuffer = renderContext.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices);

			//Task.WaitAll(tasks.ToArray());
			this.vertexBuffer = renderContext.Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, result);
		}

		private unsafe static void CopyArrays(Vector2 texScale,
			Color4[] colors,
			Vector2[] textureCoordinates,
			Vector3[] normals,
			Vector3[] positions,
			int vertexStart,
			int vertexCount,
			DefaultVertex[] result)
		{
			fixed (DefaultVertex* _vertex = result)
			fixed (Vector3* _p = positions)
			fixed (Vector3* _n = normals)
			fixed (Color4* _c = colors)
							fixed (Vector2* _t = textureCoordinates)
							{
				DefaultVertex* vertex = _vertex + vertexStart;
				DefaultVertex* vertexEnd = _vertex + vertexStart + vertexCount;
				Vector3* p = _p != null ? _p + vertexStart : null;
				Vector3* n = _n != null ? _n + vertexStart : null;
				Color4* c = _c != null ? _c + vertexStart : null;
				Vector2* t = _t != null ? _t + vertexStart : null;
								while (vertex < vertexEnd)
								{
									vertex->Position.X = p->X;
									vertex->Position.Y = p->Y;
									vertex->Position.Z = p->Z;
									vertex->Position.W = 1f;
									++p;

									if (n != null)
									{
										vertex->Normal.X = n->X;
										vertex->Normal.Y = n->Y;
										vertex->Normal.Z = n->Z;
										++n;
									}
									if (c != null)
									{
										vertex->Color.Alpha = c->Alpha;
										vertex->Color.Red = c->Red;
										vertex->Color.Green = c->Green;
										vertex->Color.Blue = c->Blue;
										++c;
									}
									if (t != null)
									{
										vertex->TexCoord.X = t->X * texScale.X;
										vertex->TexCoord.Y = t->Y * texScale.Y;
										++t;
									}

									++vertex;
								}
							}
						}
		protected override void DetachCore()
		{
			//DetachGeometry();
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

			if (this.vertexBuffer == null || this.indexBuffer == null)
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
					UpdateBuffers(renderContext, arrays, TextureCoordScale);
			}
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			base.RenderCore(renderContext);

			renderContext.TechniqueContext.Variables.DisableBack.Set(CullMode == global::SharpDX.Direct3D11.CullMode.Back);
			renderContext.TechniqueContext.Variables.DisableBlandDif.Set(DisableBlandDiffuseColors);

			Draw(renderContext, PrimitiveTopology.TriangleList, this.indexBuffer, this.vertexBuffer, DefaultVertex.SizeInBytes);
		}
	}
}
