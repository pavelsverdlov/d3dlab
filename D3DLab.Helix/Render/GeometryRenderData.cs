using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public abstract class GeometryRenderData : RenderData
	{
		private int instancesHashCode;
		protected Buffer instanceBuffer;
		protected RasterizerState rasterState;
        public Vector2 TextureCoordScale { get; set; }

        protected GeometryRenderData(RenderTechnique _renderTechnique)
			: base()
		{
			RenderSources = new GeometryRenderSources();
			RenderSources.StartUpdate = DoStartUpdate;
			CullMode = CullMode.None;
		}

		public GeometryRenderSources RenderSources { get; private set; }

		public CullMode CullMode { get; set; }

		public int DepthBias { get; set; }

		public Matrix[] Instances { get; set; }

		public bool IsThrowingShadow { get; set; }

		public float Thickness { get; set; }

		public float Smoothness { get; set; }

		protected override void AttachCore(RenderContext renderContext)
		{
		}

		protected override bool CanRender(RenderContext renderContext)
		{
			if (!base.CanRender(renderContext))
				return false;
			return !renderContext.IsShadowPass || IsThrowingShadow;
		}

		protected virtual void DoStartUpdate(RenderArrays obj) { }

		protected virtual void UpdateRasterState(RenderContext renderContext, int depthBias, CullMode cullMaterialMode)
		{
			if (this.rasterState != null &&
				this.rasterState.Description.CullMode == cullMaterialMode &&
				this.rasterState.Description.DepthBias == depthBias)
			{
				return;
			}

			var rasterStateDesc = new RasterizerStateDescription()
			{
				FillMode = FillMode.Solid,
				CullMode = cullMaterialMode,
				DepthBias = depthBias,
				DepthBiasClamp = -1000,
				SlopeScaledDepthBias = +0,
				IsDepthClipEnabled = true,
				IsFrontCounterClockwise = true,

				//IsMultisampleEnabled = true,
				//IsAntialiasedLineEnabled = true,                    
				//IsScissorEnabled = true,
			};

			UpdateRasterState(renderContext, rasterStateDesc);
		}

		protected void UpdateRasterState(RenderContext renderContext, RasterizerStateDescription rasterStateDesc)
		{
			Disposer.RemoveAndDispose(ref this.rasterState);
			/// --- set up rasterizer states
			try
			{
				this.rasterState = new RasterizerState(renderContext.Device, rasterStateDesc);
			}
			catch { }
		}
		private static int GetCode(Matrix[] instances)
		{
			return instances != null ? instances.GetHashCode() : 0;
		}

		protected override void RenderCore(RenderContext renderContext)
		{
			/// --- set constant paramerers             
			var worldMatrix = Transform * renderContext.worldMatrix;

			SDXCommonExtensions.CheckIsNaN(ref worldMatrix);
			SDXCommonExtensions.CheckData(worldMatrix.TranslationVector.Length() > 500f);

			renderContext.TechniqueContext.Variables.World.SetMatrix(ref worldMatrix);

			if (renderContext.TechniqueContext.Variables.LineParams != null)
			{
				/// --- set effect per object const vars
				var lineParams = new Vector4(Thickness, Smoothness, 0, 0);
				renderContext.TechniqueContext.Variables.LineParams.Set(lineParams);
			}

			/// --- set rasterstate            
			UpdateRasterState(renderContext, DepthBias, this.CullMode);
			renderContext.Device.ImmediateContext.Rasterizer.State = this.rasterState;

			var code = GetCode(Instances);
			bool hasInstances = Instances != null && Instances.Length > 0;
			if (instancesHashCode != code && hasInstances)
			{
				Disposer.RemoveAndDispose(ref instanceBuffer);
				this.instanceBuffer = Buffer.Create(renderContext.Device, Instances,
					new BufferDescription(sizeInBytes: Matrix.SizeInBytes * Instances.Length,
						usage: ResourceUsage.Dynamic,
						bindFlags: BindFlags.VertexBuffer,
						cpuAccessFlags: CpuAccessFlags.Write,
						optionFlags: ResourceOptionFlags.None,
						structureByteStride: 0));
				instancesHashCode = code;

				DataStream stream;
				renderContext.Device.ImmediateContext.MapSubresource(this.instanceBuffer, MapMode.WriteDiscard,
					global::SharpDX.Direct3D11.MapFlags.None, out stream);
				try
				{
					stream.Position = 0;
					stream.WriteRange(Instances, 0, Instances.Length);
					renderContext.Device.ImmediateContext.UnmapSubresource(this.instanceBuffer, 0);
				}
				finally
				{
					stream.Dispose();
				}
			}
			else if (!hasInstances)
			{
				Disposer.RemoveAndDispose(ref instanceBuffer);
			}

			if (renderContext.TechniqueContext.Variables.HasInstances != null)
				renderContext.TechniqueContext.Variables.HasInstances.Set(this.instanceBuffer != null);

			renderContext.Device.ImmediateContext.InputAssembler.InputLayout = renderContext.TechniqueContext.VertexLayout;
		}

		protected override void DetachCore()
		{
			Disposer.RemoveAndDispose(ref this.rasterState);
			Disposer.RemoveAndDispose(ref this.instanceBuffer);
			Instances = null;
			instancesHashCode = 0;
		}

		protected void Draw(RenderContext renderContext, PrimitiveTopology primitiveTopology, Buffer indexBuffer, Buffer vertexBuffer, int vertexSizeInBytes)
		{
			int indicesCount = indexBuffer.Description.SizeInBytes / sizeof(int);

			/// --- set context
			renderContext.Device.ImmediateContext.InputAssembler.PrimitiveTopology = primitiveTopology;
			renderContext.Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

			if (this.instanceBuffer != null)
			{
				int instancesCount = this.instanceBuffer.Description.SizeInBytes / Matrix.SizeInBytes;

				/// --- INSTANCING: need to set 2 buffers
				renderContext.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new[]
				{
					new VertexBufferBinding(vertexBuffer, vertexSizeInBytes, 0),
					new VertexBufferBinding(this.instanceBuffer, Matrix.SizeInBytes, 0)
				});

				/// --- render the geometry
				for (int i = 0; i < renderContext.TechniqueContext.EffectTechnique.Description.PassCount; i++)
				{
					renderContext.TechniqueContext.ApplyPass(renderContext.Device, i);
					renderContext.Device.ImmediateContext.DrawIndexedInstanced(indicesCount, instancesCount, 0, 0, 0);
				}
			}
			else
			{
				/// --- bind buffer
				renderContext.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, vertexSizeInBytes, 0));

				/// --- render the geometry
				for (int i = 0; i < renderContext.TechniqueContext.EffectTechnique.Description.PassCount; i++)
				{
					renderContext.TechniqueContext.ApplyPass(renderContext.Device, i);
					renderContext.Device.ImmediateContext.DrawIndexed(indicesCount, 0, 0);
				}
			}
		}
	}
}
