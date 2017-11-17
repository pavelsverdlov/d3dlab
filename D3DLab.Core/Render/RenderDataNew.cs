using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace D3DLab.Core.Render {
    public sealed class RenderDataNew : IDisposable {//
                                                                                       //       GeometryRenderData

        public GeometryRenderSources RenderSources { get; private set; }
        public CullMode CullMode { get; set; }
        public Matrix Transform { get; set; }
        public Vector2 TextureCoordScale { get; set; }
        public float Thickness { get; set; }
        public float Smoothness { get; set; }
        public int DepthBias { get; set; }
        public Matrix[] Instances { get; set; }
        public RenderDataNew() : base() {
            CullMode = CullMode.None;
            RenderSources = new GeometryRenderSources();
        }

        private global::SharpDX.Direct3D11.Buffer vertexBuffer;
        private global::SharpDX.Direct3D11.Buffer indexBuffer;

        private Buffer instanceBuffer;
        private RasterizerState rasterState;

        public void Update(SharpDevice device) {
            RenderArrays geometry = RenderSources.ToArrays();
            Vector2 texScale = TextureCoordScale;
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
        
        private unsafe static void CopyArrays(Vector2 texScale,
            Color4[] colors,
            Vector2[] textureCoordinates,
            Vector3[] normals,
            Vector3[] positions,
            int vertexStart,
            int vertexCount,
            DefaultVertex[] result) {
            fixed (DefaultVertex* _vertex = result)
            fixed (Vector3* _p = positions)
            fixed (Vector3* _n = normals)
            fixed (Color4* _c = colors)
            fixed (Vector2* _t = textureCoordinates) {
                DefaultVertex* vertex = _vertex + vertexStart;
                DefaultVertex* vertexEnd = _vertex + vertexStart + vertexCount;
                Vector3* p = _p != null ? _p + vertexStart : null;
                Vector3* n = _n != null ? _n + vertexStart : null;
                Color4* c = _c != null ? _c + vertexStart : null;
                Vector2* t = _t != null ? _t + vertexStart : null;
                while (vertex < vertexEnd) {
                    vertex->Position.X = p->X;
                    vertex->Position.Y = p->Y;
                    vertex->Position.Z = p->Z;
                    vertex->Position.W = 1f;
                    ++p;

                    if (n != null) {
                        vertex->Normal.X = n->X;
                        vertex->Normal.Y = n->Y;
                        vertex->Normal.Z = n->Z;
                        ++n;
                    }
                    if (c != null) {
                        vertex->Color.Alpha = c->Alpha;
                        vertex->Color.Red = c->Red;
                        vertex->Color.Green = c->Green;
                        vertex->Color.Blue = c->Blue;
                        ++c;
                    }
                    if (t != null) {
                        vertex->TexCoord.X = t->X * texScale.X;
                        vertex->TexCoord.Y = t->Y * texScale.Y;
                        ++t;
                    }

                    ++vertex;
                }
            }
        }
        public void Dispose() {
            Disposer.RemoveAndDispose(ref vertexBuffer);
            Disposer.RemoveAndDispose(ref indexBuffer);
        }

        public void Render(SharpDevice device,EffectVariables variables, InputLayout vertexLayout, EffectTechnique effectTechnique) {//, RenderContext renderContext
            //var variables = renderContext.TechniqueContext.Variables;
            var immediateContext = device.Device.ImmediateContext;
            // base.RenderCore(renderContext);
            // --- set constant paramerers             
            var worldMatrix = Transform * Matrix.Identity;


            // variables
            variables.World.SetMatrix(ref worldMatrix);
            if (variables.LineParams != null) {
                // --- set effect per object const vars
                var lineParams = new Vector4(Thickness, Smoothness, 0, 0);
                variables.LineParams.Set(lineParams);
            }
            variables.DisableBack.Set(CullMode == global::SharpDX.Direct3D11.CullMode.Back);
            variables.DisableBlandDif.Set(false); //DisableBlandDiffuseColors
            if (variables.HasInstances != null)
                variables.HasInstances.Set(this.instanceBuffer != null);


            // --- set rasterstate            
            //UpdateRasterState(renderContext, DepthBias, this.CullMode);
            var rasterizerStateDescription = new RasterizerStateDescription() {
                FillMode = FillMode.Solid,
                CullMode = this.CullMode,
                DepthBias = DepthBias,
                DepthBiasClamp = -1000,
                SlopeScaledDepthBias = +0,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = true,

                //IsMultisampleEnabled = true,
                //IsAntialiasedLineEnabled = true,                    
                //IsScissorEnabled = true,
            };
            using (var rasterState = new RasterizerState(device.Device, rasterizerStateDescription)) {
                immediateContext.Rasterizer.State = rasterState;
                /*
                 Disposer.RemoveAndDispose(ref instanceBuffer);
                 this.instanceBuffer = Buffer.Create(renderContext.Device, Instances,
                     new BufferDescription(sizeInBytes: Matrix.SizeInBytes * Instances.Length,
                         usage: ResourceUsage.Dynamic,
                         bindFlags: BindFlags.VertexBuffer,
                         cpuAccessFlags: CpuAccessFlags.Write,
                         optionFlags: ResourceOptionFlags.None,
                         structureByteStride: 0));

                 DataStream stream;
                 renderContext.Device.ImmediateContext.MapSubresource(this.instanceBuffer, MapMode.WriteDiscard,
                     global::SharpDX.Direct3D11.MapFlags.None, out stream);
                 try {
                     stream.Position = 0;
                     stream.WriteRange(Instances, 0, Instances.Length);
                     renderContext.Device.ImmediateContext.UnmapSubresource(this.instanceBuffer, 0);
                 } finally {
                     stream.Dispose();
                 }
                */
                immediateContext.InputAssembler.InputLayout = vertexLayout;
                // --- set context
                immediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                immediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

                int indicesCount = indexBuffer.Description.SizeInBytes / sizeof(int);
                if (this.instanceBuffer != null) {
                    int instancesCount = this.instanceBuffer.Description.SizeInBytes/Matrix.SizeInBytes;
                    // -- INSTANCING: need to set 2 buffers
                    immediateContext.InputAssembler.SetVertexBuffers(0, new[] {
                        new VertexBufferBinding(vertexBuffer, DefaultVertex.SizeInBytes, 0),
                        new VertexBufferBinding(this.instanceBuffer, Matrix.SizeInBytes, 0)
                    });
                    // --- render the geometry
                    for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                        ApplyPass(effectTechnique,device.Device, i);
                        immediateContext.DrawIndexedInstanced(indicesCount, instancesCount, 0, 0, 0);
                    }
                } else {
                    // --- bind buffer
                    immediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, DefaultVertex.SizeInBytes, 0));
                    // --- render the geometry
                    for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                        ApplyPass(effectTechnique,device.Device, i);
                        immediateContext.DrawIndexed(indicesCount, 0, 0);
                    }
                }
            }
        }

        public void ApplyPass(EffectTechnique effectTechnique, global::SharpDX.Direct3D11.Device device, int i) {
            effectTechnique.GetPassByIndex(i).Apply(device.ImmediateContext);
        }

        private void UpdateRasterState(RenderContext renderContext, int depthBias, CullMode cullMaterialMode) {
            if (this.rasterState != null &&
                this.rasterState.Description.CullMode == cullMaterialMode &&
                this.rasterState.Description.DepthBias == depthBias) {
                return;
            }

            var rasterStateDesc = new RasterizerStateDescription() {
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
        private void UpdateRasterState(RenderContext renderContext, RasterizerStateDescription rasterStateDesc) {
            Disposer.RemoveAndDispose(ref this.rasterState);
            /// --- set up rasterizer states
            try {
                this.rasterState = new RasterizerState(renderContext.Device, rasterStateDesc);
            } catch { }
        }

    }
}
