using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Systems;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.SDX.Engine.Rendering.Strategies {
    internal interface IRenderStrategy {
        void Update(GraphicsDevice Graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer);
        void Render(GraphicsDevice Graphics);
    }

    internal class ColoredVertexesRenderStrategy : IRenderStrategy {
        readonly D3DColoredVertexesRenderComponent renderCom;
        readonly IGeometryComponent geometryCom;

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexPositionColor {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector4 Color;

            public VertexPositionColor(Vector3 position, Vector3 normal, Vector4 color) {
                Position = position;
                Normal = normal;
                Color = color;
            }
        }

        public ColoredVertexesRenderStrategy(Components.D3DColoredVertexesRenderComponent rcom, IGeometryComponent geocom) {
            this.renderCom = rcom;
            this.geometryCom = geocom;
        }

        public void Update(GraphicsDevice graphics, SharpDX.Direct3D11.Buffer gameDataBuffer, SharpDX.Direct3D11.Buffer lightDataBuffer) {
            if (renderCom.IsBuffersCreated) {
                return;
            }

            var device = graphics.Device;
            var context = graphics.ImmediateContext;

            var pass = renderCom.Pass;

            var indexes = geometryCom.Indices.ToArray();
            var vertices = new VertexPositionColor[indexes.Length];
            for (var i = 0; i < indexes.Length; i++) {
                var index = indexes[i];
                vertices[i] = new VertexPositionColor(geometryCom.Positions[index], geometryCom.Normals[index], geometryCom.Colors[index]);
            }

            var layconst = new VertexLayoutConstructor()
                .AddPositionElementAsVector3()
                .AddNormalElementAsVector3()
                .AddColorElementAsVector4();

            var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
            
            var triangleVertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertices);
            var indexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, indexes);

            var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShader = new PixelShader(device, pass.PixelShader.ReadCompiledBytes());

            context.VertexShader.Set(vertexShader);
            context.VertexShader.SetConstantBuffer(GameResourceBuffer.RegisterResourceSlot, gameDataBuffer);
            context.VertexShader.SetConstantBuffer(LightStructLayout.RegisterResourceSlot, lightDataBuffer);

            context.PixelShader.Set(pixelShader);

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = new InputLayout(device, inputSignature, layconst.ConstuctElements());

            context.InputAssembler.SetVertexBuffers(0,
                    new VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionColor>(), 0));
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_SInt, 0);

            renderCom.IsBuffersCreated = true;
        }
        public void Render(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;

            graphics.UpdateRasterizerState(renderCom.RasterizerState.GetDescription());
            
            graphics.ImmediateContext.Draw(geometryCom.Indices.Count, 0);
        }
    }
}
