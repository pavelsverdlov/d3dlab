using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Components;
using D3DLab.Core.Components.Render;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace D3DLab.Core.Render.Components {
    public sealed class LineRenderComponent : RenderComponent, IAttachTo<LineEntity> {
        private sealed class Data : VisualRenderData {

            public override void Dispose() {
                base.Dispose();
            }

            public DuplexMaterialRenderData MaterialData { get; set; }
        }

        private Data renderData;
        private LineEntity parent;
        public LineRenderComponent() {
            
        }

        protected override void OnUpdate(Graphics graphics) {
            var points = new List<Vector3> { parent.Data.Start, parent.Data.End, parent.Data.End * Vector3.UnitX * 30 };
            var g = new LineBuilder();
            //            var p1 = points[0];
            //            var vFirst = new Vector3((float)p1.X, (float)p1.Y, (float)p1.Z);
            //            var v1 = vFirst;
            //            for (int i = 1; i < points.Count; i++) {
            //                var p2 = points[i];
            //                var v2 = new Vector3((float)p2.X, (float)p2.Y, (float)p2.Z);
            //                g.AddLine(v1, v2);
            //                v1 = v2;
            //            }
            //            g.AddLine(v1, vFirst);
            g.Add(points);
            var geometry = g.ToLineGeometry3D();

            var color = parent.Data.Color.ToColor4();
            var positions = geometry.Positions;
            var vertexCount = positions.Count;
                        var result = new LinesVertex[vertexCount];
            
                        for (var i = 0; i < vertexCount; i++) {
                            result[i] = new LinesVertex {
                                Position = new Vector4(positions[i], 1f),
                                Color = color
                            };
                        }

//            var result = new DefaultVertex[vertexCount];
//            CopyArrays(new Vector2(0,0), null, null, null, positions.ToArray(), 0, positions.Count, result);

            var ndata = new Data();

//            ndata.IndexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices.ToArray());
//            ndata.VertexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, result);
                        ndata.VertexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.VertexBuffer, LinesVertex.SizeInBytes, result);
                        ndata.IndexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices.ToArray());

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Red,
                SpecularColor = SharpDX.Color.Red,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };

            ndata.MaterialData = new DuplexMaterialRenderData(mat, mat);
            ndata.MaterialData.Update(graphics.SharpDevice);

            renderData = ndata;
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
        protected void OnRender2(World world, Graphics graphics) {
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
                var worldMatrix = data.Transform * Matrix.Identity;

                // variables
                variables.World.SetMatrix(ref worldMatrix);
                if (variables.LineParams != null) {
                    // --- set effect per object const vars
                    var lineParams = new Vector4(data.Thickness,0, 0, 0);
                    variables.LineParams.Set(lineParams);
                }
                variables.DisableBack.Set(true);
                variables.DisableBlandDif.Set(false); //DisableBlandDiffuseColors
             


                // --- set rasterstate            
                //UpdateRasterState(renderContext, DepthBias, this.CullMode);
                var rasterizerStateDescription = new RasterizerStateDescription() {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.Back,
                    DepthBias =0,
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

                    int indicesCount = renderData.IndexBuffer.Description.SizeInBytes / sizeof(int);
//                    if (renderData.InstanceBuffer != null) {
//                        int instancesCount = renderData.InstanceBuffer.Description.SizeInBytes / Matrix.SizeInBytes;
//                        // -- INSTANCING: need to set 2 buffers
//                        immediateContext.InputAssembler.SetVertexBuffers(0, new[] {
//                            new VertexBufferBinding(renderData.VertexBuffer, DefaultVertex.SizeInBytes, 0),
//                            new VertexBufferBinding(renderData.InstanceBuffer, Matrix.SizeInBytes, 0)
//                        });
//                        // --- render the geometry
//                        for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
//                            ApplyPass(effectTechnique, device.Device, i);
//                            immediateContext.DrawIndexedInstanced(indicesCount, instancesCount, 0, 0, 0);
//                        }
//                    } else {
                        // --- bind buffer
                        immediateContext.InputAssembler.SetVertexBuffers(0,
                            new VertexBufferBinding(renderData.VertexBuffer, DefaultVertex.SizeInBytes, 0));
                        // --- render the geometry
                        for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                            ApplyPass(effectTechnique, device.Device, i);
                            immediateContext.DrawIndexed(indicesCount, 0, 0);
                        }
//                    }
                }
            }
            //            sw.Stop();
            //            Debug.WriteLine("Visual Render {0}ms", sw.ElapsedMilliseconds);
        }
        protected override void OnRender(World world, Graphics graphics) {
            var data = parent.Data;
            var device = graphics.SharpDevice;
            var technique = Techniques.RenderLines;// data.RenderTechnique;
            using (renderData) {
                var variables = graphics.Variables(technique);
                var vertexLayout = graphics.EffectsManager.GetLayout(technique);
                var effectTechnique = graphics.EffectsManager.GetEffect(technique)
                    .GetTechniqueByName(technique.Name);

                renderData.MaterialData.Render(variables);

                var immediateContext = device.Device.ImmediateContext;
                var worldMatrix = data.Transform * Matrix.Identity;

                variables.DisableBack.Set(true);
                variables.DisableBlandDif.Set(false);

                variables.World.SetMatrix(ref worldMatrix);
                var lineParams = new Vector4(data.Thickness, 0 /*Smoothness*/, 0, 0);
                variables.LineParams.Set(lineParams);

                var rasterizerStateDescription = new RasterizerStateDescription() {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.None,
                    DepthBias = -10,
                    DepthBiasClamp = 0,
                    SlopeScaledDepthBias = -1,
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = true,
                    IsMultisampleEnabled = true,
                    IsAntialiasedLineEnabled = true, // Intel HD 3000 doesn't like this (#10051) and it's not needed
                    //IsScissorEnabled = true,
                };

                using (var rasterState = new RasterizerState(device.Device, rasterizerStateDescription)) {
                    immediateContext.Rasterizer.State = rasterState;
                    immediateContext.InputAssembler.InputLayout = vertexLayout;
                    // --- set context
                    immediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                    immediateContext.InputAssembler.SetIndexBuffer(renderData.IndexBuffer, Format.R32_UInt, 0);

                    int indicesCount = renderData.IndexBuffer.Description.SizeInBytes / sizeof(int);
                    // --- bind buffer
                    immediateContext.InputAssembler.SetVertexBuffers(0,
                        new VertexBufferBinding(renderData.VertexBuffer, LinesVertex.SizeInBytes, 0));
                    // --- render the geometry
                    for (int i = 0; i < effectTechnique.Description.PassCount; i++) {
                        ApplyPass(effectTechnique, device.Device, i);
                        immediateContext.DrawIndexed(indicesCount, 0, 0);
                    }

                }
            }
        }
        public void ApplyPass(EffectTechnique effectTechnique, global::SharpDX.Direct3D11.Device device, int i) {
            effectTechnique.GetPassByIndex(i).Apply(device.ImmediateContext);
        }

        public void OnAttach(LineEntity parent) {
            this.parent = parent;
        }
    }
}
