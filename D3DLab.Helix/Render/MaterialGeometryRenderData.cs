using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.WinForms;

namespace HelixToolkit.Wpf.SharpDX.Render {
   
    public abstract class MaterialGeometryRenderData : GeometryRenderData {
        private PhongMaterial phongMaterial;
        private ShaderResourceView texDiffuseMapView;
        private ShaderResourceView texNormalMapView;
        private ShaderResourceView texDisplacementMapView;

        private PhongMaterial phongBackMaterial;
        private ShaderResourceView texDiffuseMapViewBack;
        private ShaderResourceView texNormalMapViewBack;
        private ShaderResourceView texDisplacementMapViewBack;

        protected MaterialGeometryRenderData(RenderTechnique _renderTechnique)
            : base(_renderTechnique) {
        }

       

        public Material Material { get; set; }
        public Material BackMaterial { get; set; }

        protected override void AttachCore(RenderContext renderContext) {
            base.AttachCore(renderContext);
            AttachMaterial(renderContext, Material as PhongMaterial);
            AttachBackMaterial(renderContext, BackMaterial as PhongMaterial);
        }

        protected override void DetachCore() {
            DetachMaterial();
            DetachBackMaterial();
            base.DetachCore();
        }

        protected override void PreRenderCore(RenderContext renderContext) {
            base.PreRenderCore(renderContext);

            var curMaterial = Material as PhongMaterial;
            var curBackMaterial = BackMaterial as PhongMaterial;
            if (IsSameMaterial(curMaterial) && IsSameBackMaterial(curBackMaterial))
                return;

            DetachMaterial();
            AttachMaterial(renderContext, curMaterial);

            DetachBackMaterial();
            AttachBackMaterial(renderContext, curMaterial);

        }

        private bool IsSameMaterial(HelixToolkit.Wpf.SharpDX.PhongMaterial curMaterial) {
            //if (curMaterial != phongMaterialParam)
            //    return false;

            if (curMaterial == null && phongMaterial == null)
                return true;

            if ((curMaterial == null) != (phongMaterial == null))
                return false;

            if (curMaterial.DiffuseColor != phongMaterial.DiffuseColor)
                return false;
            if (curMaterial.DiffuseMap != phongMaterial.DiffuseMap)
                return false;
            if (curMaterial.AmbientColor != phongMaterial.AmbientColor)
                return false;
            if (curMaterial.EmissiveColor != phongMaterial.EmissiveColor)
                return false;
            if (curMaterial.SpecularColor != phongMaterial.SpecularColor)
                return false;
            if (curMaterial.SpecularShininess != phongMaterial.SpecularShininess)
                return false;
            if (curMaterial.DisplacementMap != phongMaterial.DisplacementMap)
                return false;
            if (curMaterial.NormalMap != phongMaterial.NormalMap)
                return false;
            return true;
        }


        private bool IsSameBackMaterial(HelixToolkit.Wpf.SharpDX.PhongMaterial curMaterial) {
            //if (curMaterial != phongMaterialParam)
            //    return false;

            if (curMaterial == null && phongBackMaterial == null)
                return true;

            if ((curMaterial == null) != (phongBackMaterial == null))
                return false;

            if (curMaterial.DiffuseColor != phongBackMaterial.DiffuseColor)
                return false;
            if (curMaterial.DiffuseMap != phongBackMaterial.DiffuseMap)
                return false;
            if (curMaterial.AmbientColor != phongBackMaterial.AmbientColor)
                return false;
            if (curMaterial.EmissiveColor != phongBackMaterial.EmissiveColor)
                return false;
            if (curMaterial.SpecularColor != phongBackMaterial.SpecularColor)
                return false;
            if (curMaterial.SpecularShininess != phongBackMaterial.SpecularShininess)
                return false;
            if (curMaterial.DisplacementMap != phongBackMaterial.DisplacementMap)
                return false;
            if (curMaterial.NormalMap != phongBackMaterial.NormalMap)
                return false;
            return true;
        }


        private void AttachMaterial(RenderContext renderContext, PhongMaterial phongMaterial) {
            this.phongMaterial = phongMaterial != null ? phongMaterial.Clone() : null;
            if (phongMaterial != null) {
                /// --- has texture
                if (phongMaterial.DiffuseMapBytes != null) {
                    try {
                        this.texDiffuseMapView = ShaderResourceView.FromMemory(renderContext.Device, phongMaterial.DiffuseMapBytes);
                    } catch (SharpDXException /*se*/) {
                        if (Debugger.IsAttached) {
                            Debugger.Break();
                        }
                    }
                }

                // --- has displacement map
                if (phongMaterial.DisplacementMap != null) {
                    this.texDisplacementMapView = ShaderResourceView.FromMemory(renderContext.Device, phongMaterial.DisplacementMap.ToByteArray());
                }
            }
        }

        private void AttachBackMaterial(RenderContext renderContext, PhongMaterial phongMaterialParam) {
            this.phongBackMaterial = phongMaterialParam != null ? phongMaterialParam.Clone() : null;
            if (phongMaterialParam != null) {
                /// --- has texture
                if (phongMaterialParam.DiffuseMapBytes != null) {
                    try {
                        this.texDiffuseMapViewBack = ShaderResourceView.FromMemory(renderContext.Device, phongMaterialParam.DiffuseMapBytes);
                    } catch (SharpDXException /*se*/) {
                        if (Debugger.IsAttached) {
                            Debugger.Break();
                        }
                    }
                }

                // --- has displacement map
                if (phongMaterialParam.DisplacementMap != null) {
                    this.texDisplacementMapViewBack = ShaderResourceView.FromMemory(renderContext.Device, phongMaterialParam.DisplacementMap.ToByteArray());
                }
            }
        }



        private void DetachMaterial() {
            phongMaterial = null;
            Disposer.RemoveAndDispose(ref texDiffuseMapView);
            Disposer.RemoveAndDispose(ref texNormalMapView);
            Disposer.RemoveAndDispose(ref texDisplacementMapView);
        }

        private void DetachBackMaterial() {
            phongBackMaterial = null;
            Disposer.RemoveAndDispose(ref texDiffuseMapViewBack);
            Disposer.RemoveAndDispose(ref texNormalMapViewBack);
            Disposer.RemoveAndDispose(ref texDisplacementMapViewBack);
        }

        protected override void RenderCore(RenderContext renderContext) {
            base.RenderCore(renderContext);
            /// --- set material params
            if (phongMaterial != null) {
                var IsShadowMapEnabled = false;

                renderContext.TechniqueContext.Variables.HasShadowMapVariable.Set(IsShadowMapEnabled);
                renderContext.TechniqueContext.Variables.MaterialDiffuseVariable.Set(phongMaterial.DiffuseColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialAmbientVariable.Set(phongMaterial.AmbientColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialEmissiveVariable.Set(phongMaterial.EmissiveColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialSpecularVariable.Set(phongMaterial.SpecularColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialReflectVariable.Set(phongMaterial.ReflectiveColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialShininessVariable.Set(phongMaterial.SpecularShininess.CheckIsNaN());
                renderContext.TechniqueContext.Variables.HasDiffuseMapVariable.Set(this.texDiffuseMapView != null);
                if (this.texDiffuseMapView != null)
                    renderContext.TechniqueContext.Variables.TexDiffuseMapVariable.SetResource(this.texDiffuseMapView);
                renderContext.TechniqueContext.Variables.HasNormalMapVariable.Set(this.texNormalMapView != null);
                if (this.texNormalMapView != null)
                    renderContext.TechniqueContext.Variables.TexNormalMapVariable.SetResource(this.texNormalMapView);
                renderContext.TechniqueContext.Variables.HasDisplacementMapVariable.Set(this.texDisplacementMapView != null);
                if (this.texDisplacementMapView != null)
                    renderContext.TechniqueContext.Variables.TexDisplacementMapVariable.SetResource(this.texDisplacementMapView);
            }
            if (phongBackMaterial != null) {
                renderContext.TechniqueContext.Variables.MaterialBackDiffuseVariable.Set(phongBackMaterial.DiffuseColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialBackAmbientVariable.Set(phongBackMaterial.AmbientColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialBackEmissiveVariable.Set(phongBackMaterial.EmissiveColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialBackSpecularVariable.Set(phongBackMaterial.SpecularColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialBackReflectVariable.Set(phongBackMaterial.ReflectiveColor.CheckIsNaN());
                renderContext.TechniqueContext.Variables.MaterialBackShininessVariable.Set(phongBackMaterial.SpecularShininess.CheckIsNaN());
                renderContext.TechniqueContext.Variables.HasDiffuseMapVariableBack.Set(this.texDiffuseMapViewBack != null);
                if (this.texDiffuseMapViewBack != null)
                    renderContext.TechniqueContext.Variables.TexDiffuseMapVariableBack.SetResource(this.texDiffuseMapViewBack);
                renderContext.TechniqueContext.Variables.HasNormalMapVariableBack.Set(this.texNormalMapViewBack != null);
                if (this.texNormalMapViewBack != null)
                    renderContext.TechniqueContext.Variables.TexNormalMapVariableBack.SetResource(this.texNormalMapViewBack);
                renderContext.TechniqueContext.Variables.HasDisplacementMapVariableBack.Set(this.texDisplacementMapViewBack != null);
                if (this.texDisplacementMapViewBack != null)
                    renderContext.TechniqueContext.Variables.TexDisplacementMapVariableBack.SetResource(this.texDisplacementMapViewBack);
            }
        }
    }
}
