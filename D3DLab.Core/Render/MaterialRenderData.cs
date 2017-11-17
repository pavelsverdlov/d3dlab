using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using SharpDX.Direct3D11;

namespace D3DLab.Core.Render {
    public sealed class MaterialRenderData : IDisposable {
        public PhongMaterial phongMaterial;
        public ShaderResourceView texDiffuseMapView;
        public ShaderResourceView texNormalMapView;
        public ShaderResourceView texDisplacementMapView;
        private readonly PhongMaterial material;

        public MaterialRenderData(PhongMaterial material) {
            if (material == null) { throw new NotSupportedException("Only PhongMaterial"); }
            this.material = material;
        }

        public void Update(SharpDevice device) {
            phongMaterial = material.Clone();
            // --- has texture
            if (phongMaterial.DiffuseMapBytes != null) {
                try {
                    this.texDiffuseMapView = ShaderResourceView.FromMemory(device.Device, phongMaterial.DiffuseMapBytes);
                } catch (SharpDXException /*se*/) {
                    if (Debugger.IsAttached) {
                        Debugger.Break();
                    }
                }
            }
            // --- has displacement map
            if (phongMaterial.DisplacementMap != null) {
                this.texDisplacementMapView = ShaderResourceView.FromMemory(device.Device, phongMaterial.DisplacementMap.ToByteArray());
            }
        }

        public void Dispose() {
            phongMaterial = null;
            Disposer.RemoveAndDispose(ref texDiffuseMapView);
            Disposer.RemoveAndDispose(ref texNormalMapView);
            Disposer.RemoveAndDispose(ref texDisplacementMapView);
        }
    }
}
