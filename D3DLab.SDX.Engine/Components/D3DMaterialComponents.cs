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
    public interface ID3DRenderable : IGraphicComponent {
        void Update(GraphicsDevice graphics);
        void Render(GraphicsDevice graphics);
    }

    internal interface ID3DMaterialComponent {

    }

    
    public class D3DTexturedMaterialComponent : TexturedMaterialComponent, ID3DMaterialComponent {
        
        /// <summary>
        /// TODO: make wrapper as for D3DRasterizerState to allow online debugging
        /// </summary>
        public SamplerStateDescription SampleDescription { get; }

        public D3DTexturedMaterialComponent(params FileInfo[] image) : base(image) {
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
    }
}
