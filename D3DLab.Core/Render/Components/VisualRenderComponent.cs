using System;
using System.Diagnostics;
using D3DLab.Core.Components;
using D3DLab.Core.Components.Render;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace D3DLab.Core.Render.Components {
    public abstract class VisualRenderData : IDisposable {
        public global::SharpDX.Direct3D11.Buffer VertexBuffer;
        public global::SharpDX.Direct3D11.Buffer IndexBuffer;

        public virtual void Dispose() {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
    public sealed class RenderData : VisualRenderData {
        public readonly Buffer InstanceBuffer;
        public readonly RasterizerState RasterState;

        public DuplexMaterialRenderData MaterialData;

        public RenderData() {
            InstanceBuffer = null;
            RasterState = null;
        }

        public override void Dispose() {
            base.Dispose();
            MaterialData.Dispose();
        }
    }
    public class VisualRenderComponent : RenderComponent,IAttachTo<VisualEntity> {
        
      
        private RenderData renderData;
        private readonly object loker;
        public VisualRenderComponent() {
            this.loker = new object();
        }

        protected override void OnUpdate(Graphics graphics) {
          
            var updateData = new RenderData();
            var data = parent.Data;
            
            var gindeces = data.Geometry.Indices.ToArrayFast();
            var gpositions = data.Geometry.Positions.ToArrayFast();
            var gnormals = data.Geometry.Normals?.ToArrayFast();
            var gtextureCoordinates = data.Geometry.TextureCoordinates?.ToArrayFast();
            var gcolors = data.Geometry.Colors?.ToArrayFast();
            
            var texScale = data.TextureCoordScale;

            /// --- init vertex buffer
            var colors = gcolors != null && gcolors.Length >= gpositions.Length ? gcolors : null;
            var textureCoordinates = (gtextureCoordinates != null && gtextureCoordinates.Length >= gpositions.Length) ? gtextureCoordinates : null;
            var normals = gnormals;
            //var tangents = geometry.Tangents != null ? geometry.Tangents : null;
            //var bitangents = geometry.BiTangents != null ? geometry.BiTangents : null;
            var positions = gpositions;
            var vertexCount = positions.Length;
            var result = new DefaultVertex[vertexCount];

//            var sw = new Stopwatch();
//            sw.Start();
            CopyArrays(texScale, colors, textureCoordinates, normals, positions, 0, positions.Length, result);
//            sw.Stop();
//            Debug.WriteLine("Visual Update CopyArrays {0}ms", sw.ElapsedMilliseconds);
//            sw.Restart();
            
            updateData.IndexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), gindeces);
            updateData.VertexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, result);

            //            graphics.SharpDevice.Device.ImmediateContext.MapSubresource(,)
            //                graphics.SharpDevice.Device.ImmediateContext.UpdateSubresource();

            //write using MapSubresource
//            DataStream dataStream;
//            using (var stream = new DataStream(range.Length*sizeofT, true, true)) {
//                stream.WriteRange(range);
//                stream.Position = 0;
//                var vb = new SharpDX.Direct3D11.Buffer(graphics.SharpDevice.Device, stream,
//                    new BufferDescription {
//                        BindFlags = flags,
//                        SizeInBytes = (int) stream.Length,
//                        OptionFlags = ResourceOptionFlags.None,
//                        Usage = ResourceUsage.Default,
//                        CpuAccessFlags = CpuAccessFlags.None,
//                    });
//            }
//            graphics.SharpDevice.Device.ImmediateContext.MapSubresource(updateData.VertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
//            dataStream.Write();
//            var m = data.Transform;
//            graphics.SharpDevice.Device.ImmediateContext.UpdateSubresource(ref m, updateData.VertexBuffer);


//            sw.Stop();
//            Debug.WriteLine("Visual Update CreateBuffers {0}ms", sw.ElapsedMilliseconds);

            updateData.MaterialData = new DuplexMaterialRenderData(data.Material, data.BackMaterial);
            updateData.MaterialData.Update(graphics.SharpDevice);
            //
            renderData = updateData;
           
        }
        private static unsafe void CopyArrays(Vector2 texScale,
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
        
       protected override void OnRender(World world, Graphics graphics) {
//            var sw = new Stopwatch();
//            sw.Start();
            var data = parent.Data;
            var device = graphics.SharpDevice;
            using (renderData) {
                var variables = graphics.Variables(data.RenderTechnique);
                var vertexLayout = graphics.EffectsManager.GetLayout(data.RenderTechnique);
                var effectTechnique = graphics.EffectsManager.GetEffect(data.RenderTechnique)
                    .GetTechniqueByName(data.RenderTechnique.Name);
                renderData.MaterialData.Render(variables);
                
                //var variables = renderContext.TechniqueContext.Variables;
                var immediateContext = device.Device.ImmediateContext;
                // base.RenderCore(renderContext);
                // --- set constant paramerers             
                var worldMatrix = data.Transform*Matrix.Identity;
                
                // variables
                variables.World.SetMatrix(ref worldMatrix);
                if (variables.LineParams != null) {
                    // --- set effect per object const vars
                    var lineParams = new Vector4(data.Thickness, data.Smoothness, 0, 0);
                    variables.LineParams.Set(lineParams);
                }
                variables.DisableBack.Set(data.CullMaterial == global::SharpDX.Direct3D11.CullMode.Back);
                variables.DisableBlandDif.Set(false); //DisableBlandDiffuseColors
                if (variables.HasInstances != null)
                    variables.HasInstances.Set(renderData.InstanceBuffer != null);


                // --- set rasterstate            
                //UpdateRasterState(renderContext, DepthBias, this.CullMode);
                var rasterizerStateDescription = new RasterizerStateDescription() {
                    FillMode = FillMode.Solid,
                    CullMode = data.CullMaterial,
                    DepthBias = data.DepthBias,
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
                    immediateContext.InputAssembler.SetIndexBuffer(renderData.IndexBuffer, Format.R32_UInt, 0);

                    int indicesCount = renderData.IndexBuffer.Description.SizeInBytes/sizeof (int);
                    if (renderData.InstanceBuffer != null) {
                        int instancesCount = renderData.InstanceBuffer.Description.SizeInBytes/Matrix.SizeInBytes;
                        // -- INSTANCING: need to set 2 buffers
                        immediateContext.InputAssembler.SetVertexBuffers(0, new[] {
                            new VertexBufferBinding(renderData.VertexBuffer, DefaultVertex.SizeInBytes, 0),
                            new VertexBufferBinding(renderData.InstanceBuffer, Matrix.SizeInBytes, 0)
                        });
                        // --- render the geometry
                        for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                            ApplyPass(effectTechnique, device.Device, i);
                            immediateContext.DrawIndexedInstanced(indicesCount, instancesCount, 0, 0, 0);
                        }
                    } else {
                        // --- bind buffer
                        immediateContext.InputAssembler.SetVertexBuffers(0,
                            new VertexBufferBinding(renderData.VertexBuffer, DefaultVertex.SizeInBytes, 0));
                        // --- render the geometry
                        for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                            ApplyPass(effectTechnique, device.Device, i);
                            immediateContext.DrawIndexed(indicesCount, 0, 0);
                        }
                    }
                }
            }
//            sw.Stop();
//            Debug.WriteLine("Visual Render {0}ms", sw.ElapsedMilliseconds);
        }

        public void ApplyPass(EffectTechnique effectTechnique, global::SharpDX.Direct3D11.Device device, int i) {
            effectTechnique.GetPassByIndex(i).Apply(device.ImmediateContext);
        }

        private VisualEntity parent;

      

        public void OnAttach(VisualEntity parent) {
            this.parent = parent;
        }
        
    }
}