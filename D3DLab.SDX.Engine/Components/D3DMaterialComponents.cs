using D3DLab.SDX.Engine.D2;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Ext;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.SDX.Engine.Components {
    internal interface ID3DRenderable : IGraphicComponent {
        void Update(GraphicsDevice graphics);
        void Render(GraphicsDevice graphics);
    }

    internal interface ID3DMaterialComponent {

    }

    public class D3DGradientMaterialComponent : GradientMaterialComponent, ID3DRenderable, ID3DMaterialComponent {
        [StructLayout(LayoutKind.Sequential)]
        public struct GradientBuffer {
            public Vector4 apexColor;
            public Vector4 centerColor;
        }

        public D3DGradientMaterialComponent() {
            IsModified = true;
        }

        public override void Dispose() {
            base.Dispose();
            ConstantBuffer?.Dispose();
        }


        #region D3D

        internal SharpDX.Direct3D11.Buffer ConstantBuffer { get; private set; }

        void ID3DRenderable.Update(GraphicsDevice graphics) {
            var str = new GradientBuffer {
                apexColor = Apex,
                centerColor = Center
            };
            ConstantBuffer = graphics.CreateBuffer(BindFlags.ConstantBuffer, ref str);
            IsModified = false;
        }
        void ID3DRenderable.Render(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;
            context.PixelShader.SetConstantBuffer(0, ConstantBuffer);
        }

        #endregion


    }
    public class D3DTexturedMaterialComponent : TexturedMaterialComponent, ID3DRenderable, ID3DMaterialComponent {

        public D3DTexturedMaterialComponent(params FileInfo[] image) : base(image) {
            TextureResources = new ShaderResourceView[0];
            SampleDescription = new SamplerStateDescription() {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLodBias = 0.0f,
                MaximumAnisotropy = 1,
                ComparisonFunction = Comparison.Always,
                BorderColor = new SharpDX.Color4(0, 0, 0, 0),
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            };
            IsModified = true;
        }

        public override void Dispose() {
            TextureResources.ForEach(x => x.Dispose());
            SampleState?.Dispose();
            base.Dispose();
        }

        #region D3D

        /// <summary>
        /// TODO: make wrapper as for D3DRasterizerState to allow online debugging
        /// </summary>
        public SamplerStateDescription SampleDescription { get; }

        [IgnoreDebuging]
        internal ShaderResourceView[] TextureResources { get; set; }
        [IgnoreDebuging]
        internal SamplerState SampleState { get; set; }

        ShaderResourceView[] ConvertToResources(TexturedLoader loader) {
            var resources = new ShaderResourceView[Images.Length];
            for (var i = 0; i < Images.Length; i++) {
                var file = Images[i];
                resources[i] = loader.LoadShaderResource(file);
            }
            return resources;
        }

        void ID3DRenderable.Update(GraphicsDevice graphics) {
            TextureResources = ConvertToResources(graphics.TexturedLoader);
            SampleState = graphics.CreateSampler(SampleDescription);
            IsModified = false;
        }

        void ID3DRenderable.Render(GraphicsDevice graphics) {
            var context = graphics.ImmediateContext;
            context.PixelShader.SetShaderResources(0, TextureResources);
            context.PixelShader.SetSampler(0, SampleState);
        }

        #endregion
    }
}
