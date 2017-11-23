using D3DLab.Core.Render;
using D3DLab.Core.Render.Components;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public interface IComponentSystem {
        void Execute(IContext ctx);        
    }


    public class VisualRenderSystem : IComponentSystem {
        public void Execute(IContext ctx) {
            foreach (var entity in ctx.GetEntities()) {
                var render = entity.GetComponent<PhongTechniqueRenderComponent>();               
                var material = entity.GetComponent<MaterialComponent>();
                var geo = entity.GetComponent<GeometryComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (render == null || material == null || geo == null || transform == null ) {
                    continue;
                }

                using (var data = Update(ctx.Graphics, geo, material)) {
                    Render(ctx.World, ctx.Graphics, data, render, transform, material);
                }
            }
        }

        private RenderData Update(Graphics graphics, GeometryComponent geo, MaterialComponent maretial) {
            var updateData = new RenderData();

            var gindeces = geo.Geometry.Indices.ToArrayFast();
            var gpositions = geo.Geometry.Positions.ToArrayFast();
            var gnormals = geo.Geometry.Normals?.ToArrayFast();
            var gtextureCoordinates = geo.Geometry.TextureCoordinates?.ToArrayFast();
            var gcolors = geo.Geometry.Colors?.ToArrayFast();

            var texScale = new Vector2();// maretial.TextureCoordScale;

            /// --- init vertex buffer
            var colors = gcolors != null && gcolors.Length >= gpositions.Length ? gcolors : null;
            var textureCoordinates = (gtextureCoordinates != null && gtextureCoordinates.Length >= gpositions.Length) ? gtextureCoordinates : null;
            var normals = gnormals;

            var positions = gpositions;
            var vertexCount = positions.Length;
            var result = new DefaultVertex[vertexCount];

            CopyArrays(texScale, colors, textureCoordinates, normals, positions, 0, positions.Length, result);

            updateData.IndexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), gindeces);
            updateData.VertexBuffer = graphics.SharpDevice.Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, result);

            updateData.MaterialData = new DuplexMaterialRenderData(maretial.Material, maretial.BackMaterial);
            updateData.MaterialData.Update(graphics.SharpDevice);
            //
            return updateData;

        }
        private void Render(World world, Graphics graphics, RenderData renderData,
            RenderTechniqueComponent render, TransformComponent transform, MaterialComponent material) {
            //            var sw = new Stopwatch();
            //            sw.Start();
            var device = graphics.SharpDevice;

            var variables = graphics.Variables(render.RenderTechnique);
            var vertexLayout = graphics.EffectsManager.GetLayout(render.RenderTechnique);
            var effectTechnique = graphics.EffectsManager.GetEffect(render.RenderTechnique)
                .GetTechniqueByName(render.RenderTechnique.Name);
            renderData.MaterialData.Render(variables);

            //var variables = renderContext.TechniqueContext.Variables;
            var immediateContext = device.Device.ImmediateContext;
            // base.RenderCore(renderContext);
            // --- set constant paramerers             
            var worldMatrix = transform.Matrix * Matrix.Identity;

            // variables
            variables.World.SetMatrix(ref worldMatrix);
            /*if (variables.LineParams != null) {
                // --- set effect per object const vars
                var lineParams = new Vector4(data.Thickness, data.Smoothness, 0, 0);
                variables.LineParams.Set(lineParams);
            }*/
            variables.DisableBack.Set(material.CullMaterial == global::SharpDX.Direct3D11.CullMode.Back);
            variables.DisableBlandDif.Set(false); //DisableBlandDiffuseColors
            if (variables.HasInstances != null)
                variables.HasInstances.Set(renderData.InstanceBuffer != null);


            // --- set rasterstate            
            //UpdateRasterState(renderContext, DepthBias, this.CullMode);
            var rasterizerStateDescription = new RasterizerStateDescription() {
                FillMode = FillMode.Solid,
                CullMode = material.CullMaterial,
                DepthBias = 0,//material.DepthBias,
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
                immediateContext.InputAssembler.InputLayout = vertexLayout;
                // --- set context
                immediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                immediateContext.InputAssembler.SetIndexBuffer(renderData.IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

                int indicesCount = renderData.IndexBuffer.Description.SizeInBytes / sizeof(int);
                if (renderData.InstanceBuffer != null) {
                    int instancesCount = renderData.InstanceBuffer.Description.SizeInBytes / Matrix.SizeInBytes;
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

            //            sw.Stop();
            //            Debug.WriteLine("Visual Render {0}ms", sw.ElapsedMilliseconds);
        }

        public void ApplyPass(EffectTechnique effectTechnique, global::SharpDX.Direct3D11.Device device, int i) {
            effectTechnique.GetPassByIndex(i).Apply(device.ImmediateContext);
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
    }
    public class UpdateRenderTechniqueSystem : IComponentSystem {
        public void Execute(IContext ctx) {
            foreach (var entity in ctx.GetEntities()) {
                var tech = entity.GetComponent<RenderTechniqueComponent>();
                if(tech == null) {
                    continue;
                }
                switch (tech) {
                    case LightBuilder.LightTechniqueRenderComponent light:
                        var com = entity.GetComponent<LightBuilder.LightRenderComponent>();
                        light.Update(ctx.Graphics,ctx.World, com.Color);
                        break;
                    case CameraBuilder.CameraTechniqueRenderComponent camera:
                        var ccom = entity.GetComponent<CameraBuilder.CameraComponent>();
                        camera.Update(ctx.Graphics, ctx.World, ccom);
                        break;
                }
            }


        }

    }
}
