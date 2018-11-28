using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.SDX.Engine.Components {
    public class D3DTexturedMaterialComponent : TexturedMaterialComponent {

        [IgnoreDebuging]
        public ShaderResourceView TextureResource { get; set; }
        [IgnoreDebuging]
        public SamplerState SampleState { get; set; }

        /// <summary>
        /// TODO: make wrapper as for D3DRasterizerState to allow online debugging
        /// </summary>
        public SamplerStateDescription SampleDescription { get; }

        public D3DTexturedMaterialComponent(FileInfo image) : base(image) {
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
            TextureResource?.Dispose();
            SampleState?.Dispose();
            base.Dispose();
        }
    }
}
