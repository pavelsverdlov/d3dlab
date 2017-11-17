using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;

namespace D3DLab.Core.Render {
    public sealed class DuplexMaterialRenderData : IDisposable{
        private readonly MaterialRenderData front;
        private readonly MaterialRenderData back;

        public DuplexMaterialRenderData(Material material, Material backMaterial) {
            front = new MaterialRenderData(material as PhongMaterial);
            back = new MaterialRenderData(backMaterial as PhongMaterial);
        }

        public void Update(SharpDevice device) {
            front.Update(device);
            back.Update(device);
        }

        public void Render(EffectVariables variables) {
            RenderFront(variables, front);
            RenderBack(variables, back);
        }

        private static void RenderBack(EffectVariables variables, MaterialRenderData data) {
            variables.MaterialBackDiffuseVariable.Set(data.phongMaterial.DiffuseColor.CheckIsNaN());
            variables.MaterialBackAmbientVariable.Set(data.phongMaterial.AmbientColor.CheckIsNaN());
            variables.MaterialBackEmissiveVariable.Set(data.phongMaterial.EmissiveColor.CheckIsNaN());
            variables.MaterialBackSpecularVariable.Set(data.phongMaterial.SpecularColor.CheckIsNaN());
            variables.MaterialBackReflectVariable.Set(data.phongMaterial.ReflectiveColor.CheckIsNaN());
            variables.MaterialBackShininessVariable.Set(data.phongMaterial.SpecularShininess.CheckIsNaN());
            variables.HasDiffuseMapVariableBack.Set(data.texDiffuseMapView != null);
            if (data.texDiffuseMapView != null)
                variables.TexDiffuseMapVariableBack.SetResource(data.texDiffuseMapView);
            variables.HasNormalMapVariableBack.Set(data.texNormalMapView != null);
            if (data.texNormalMapView != null)
                variables.TexNormalMapVariableBack.SetResource(data.texNormalMapView);
            variables.HasDisplacementMapVariableBack.Set(data.texDisplacementMapView != null);
            if (data.texDisplacementMapView != null)
                variables.TexDisplacementMapVariableBack.SetResource(data.texDisplacementMapView);
        }
        private static void RenderFront(EffectVariables variables, MaterialRenderData data) {
            var IsShadowMapEnabled = false;
            variables.HasShadowMapVariable.Set(IsShadowMapEnabled);
            variables.MaterialDiffuseVariable.Set(data.phongMaterial.DiffuseColor.CheckIsNaN());
            variables.MaterialAmbientVariable.Set(data.phongMaterial.AmbientColor.CheckIsNaN());
            variables.MaterialEmissiveVariable.Set(data.phongMaterial.EmissiveColor.CheckIsNaN());
            variables.MaterialSpecularVariable.Set(data.phongMaterial.SpecularColor.CheckIsNaN());
            variables.MaterialReflectVariable.Set(data.phongMaterial.ReflectiveColor.CheckIsNaN());
            variables.MaterialShininessVariable.Set(data.phongMaterial.SpecularShininess.CheckIsNaN());
            variables.HasDiffuseMapVariable.Set(data.texDiffuseMapView != null);
            if (data.texDiffuseMapView != null)
                variables.TexDiffuseMapVariable.SetResource(data.texDiffuseMapView);
            variables.HasNormalMapVariable.Set(data.texNormalMapView != null);
            if (data.texNormalMapView != null)
                variables.TexNormalMapVariable.SetResource(data.texNormalMapView);
            variables.HasDisplacementMapVariable.Set(data.texDisplacementMapView != null);
            if (data.texDisplacementMapView != null)
                variables.TexDisplacementMapVariable.SetResource(data.texDisplacementMapView);
        }

        public void Dispose() {
            front.Dispose();
            back.Dispose();
        }
    }
}
