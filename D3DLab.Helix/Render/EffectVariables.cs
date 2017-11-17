using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Render
{
	public class EffectVariables
	{
		private readonly Effect effect;
		public EffectVariables(Effect effect)
		{
			this.effect = effect;
            OutOfBoxParams = new OutOfBoxParameters(this);
		}

		private T GetVariable<T>(ref T field, string name) where T : class
		{
			if (field != null)
				return field;

			var effectVar = effect.GetVariableByName(name);
			Debug.Assert(effectVar != null);

			EffectVariable effectVarRes;
			if (typeof(T) == typeof(EffectMatrixVariable))
				effectVarRes = effectVar.AsMatrix();
			else if (typeof(T) == typeof(EffectVectorVariable))
				effectVarRes = effectVar.AsVector();
			else if (typeof(T) == typeof(EffectScalarVariable))
				effectVarRes = effectVar.AsScalar();
			else if (typeof(T) == typeof(EffectShaderResourceVariable))
				effectVarRes = effectVar.AsShaderResource();
			else
				throw new NotImplementedException("Not implement factory for ");

			return field = (T)(object)effectVarRes;
		}

		//private EffectVectorVariable fieldName;
		//public EffectVectorVariable FieldName
		//{
		//	get { return GetVariable(ref fieldName, "var_name", i => i.AsVector()); }
		//}

		#region Camera

		private EffectMatrixVariable view;
		public EffectMatrixVariable View
		{
			get { return GetVariable(ref view, "mView"); }
		}

		private EffectMatrixVariable projection;
		public EffectMatrixVariable Projection
		{
			get { return GetVariable(ref projection, "mProjection"); }
		}

		private EffectVectorVariable viewport;
		public EffectVectorVariable Viewport
		{
			get { return GetVariable(ref viewport, "vViewport"); }
		}

		private EffectVectorVariable frustum;
		public EffectVectorVariable Frustum
		{
			get { return GetVariable(ref frustum, "vFrustum"); }
		}

		private EffectVectorVariable eyePos;
		public EffectVectorVariable EyePos
		{
			get { return GetVariable(ref eyePos, "vEyePos"); }
		}

		private EffectVectorVariable eyeLook;
		public EffectVectorVariable EyeLook
		{
			get { return GetVariable(ref eyeLook, "vEyeLook"); }
		}

		private EffectMatrixVariable world;
		public EffectMatrixVariable World
		{
			get { return GetVariable(ref world, "mWorld"); }
		}

		#endregion

		#region Render options

		private EffectScalarVariable hasInstances;
		public EffectScalarVariable HasInstances
		{
			get { return GetVariable(ref hasInstances, "bHasInstances"); }
		}

		private EffectVectorVariable tessellationVariables;
		public EffectVectorVariable TessellationVariables
		{
			get { return GetVariable(ref tessellationVariables, "vTessellation"); }
		}

		private EffectShaderResourceVariable texCubeMap;
		public EffectShaderResourceVariable TexCubeMap
		{
			get { return GetVariable(ref texCubeMap, "texCubeMap"); }
		}

		private EffectScalarVariable hasCubeMap;
		public EffectScalarVariable HasCubeMap
		{
			get { return GetVariable(ref hasCubeMap, "bHasCubeMap"); }
		}

		private EffectVectorVariable lineParams;
		public EffectVectorVariable LineParams
		{
			get { return GetVariable(ref lineParams, "vLineParams"); }
		}

		private EffectScalarVariable bDisableBack;
		public EffectScalarVariable DisableBack
		{
			get { return GetVariable(ref bDisableBack, "bDisableBack"); }
		}

		private EffectScalarVariable bDisableBlandDif;
		public EffectScalarVariable DisableBlandDif
		{
			get { return GetVariable(ref bDisableBlandDif, "bDisableBlandDif"); }
		}

		#endregion

		#region Material

		private EffectVectorVariable materialAmbientVariable;
		public EffectVectorVariable MaterialAmbientVariable
		{
			get { return GetVariable(ref materialAmbientVariable, "vMaterialAmbient"); }
		}

		private EffectVectorVariable materialDiffuseVariable;
		public EffectVectorVariable MaterialDiffuseVariable
		{
			get { return GetVariable(ref materialDiffuseVariable, "vMaterialDiffuse"); }
		}

		private EffectVectorVariable materialEmissiveVariable;
		public EffectVectorVariable MaterialEmissiveVariable
		{
			get { return GetVariable(ref materialEmissiveVariable, "vMaterialEmissive"); }
		}

		private EffectVectorVariable materialSpecularVariable;
		public EffectVectorVariable MaterialSpecularVariable
		{
			get { return GetVariable(ref materialSpecularVariable, "vMaterialSpecular"); }
		}

		private EffectVectorVariable materialReflectVariable;
		public EffectVectorVariable MaterialReflectVariable
		{
			get { return GetVariable(ref materialReflectVariable, "vMaterialReflect"); }
		}

		private EffectScalarVariable materialShininessVariable;
		public EffectScalarVariable MaterialShininessVariable
		{
			get { return GetVariable(ref materialShininessVariable, "sMaterialShininess"); }
		}

		private EffectScalarVariable hasDiffuseMapVariable;
		public EffectScalarVariable HasDiffuseMapVariable
		{
			get { return GetVariable(ref hasDiffuseMapVariable, "bHasDiffuseMap"); }
		}

		private EffectScalarVariable hasNormalMapVariable;
		public EffectScalarVariable HasNormalMapVariable
		{
			get { return GetVariable(ref hasNormalMapVariable, "bHasNormalMap"); }
		}

		private EffectScalarVariable hasDisplacementMapVariable;
		public EffectScalarVariable HasDisplacementMapVariable
		{
			get { return GetVariable(ref hasDisplacementMapVariable, "bHasDisplacementMap"); }
		}

		private EffectShaderResourceVariable texDiffuseMapVariable;
		public EffectShaderResourceVariable TexDiffuseMapVariable
		{
			get { return GetVariable(ref texDiffuseMapVariable, "texDiffuseMap"); }
		}

		private EffectShaderResourceVariable texNormalMapVariable;
		public EffectShaderResourceVariable TexNormalMapVariable
		{
			get { return GetVariable(ref texNormalMapVariable, "texNormalMap"); }
		}

		private EffectShaderResourceVariable texDisplacementMapVariable;
		public EffectShaderResourceVariable TexDisplacementMapVariable
		{
			get { return GetVariable(ref texDisplacementMapVariable, "texDisplacementMap"); }
		}

		#endregion

		#region BackMaterial

		private EffectVectorVariable materialBackAmbientVariable;
		public EffectVectorVariable MaterialBackAmbientVariable {
			get { return GetVariable(ref materialBackAmbientVariable, "vMaterialAmbientBack"); }
		}

		private EffectVectorVariable materialBackDiffuseVariable;
		public EffectVectorVariable MaterialBackDiffuseVariable {
			get { return GetVariable(ref materialBackDiffuseVariable, "vMaterialDiffuseBack"); }
		}

		private EffectVectorVariable materialBackEmissiveVariable;
		public EffectVectorVariable MaterialBackEmissiveVariable {
			get { return GetVariable(ref materialBackEmissiveVariable, "vMaterialEmissiveBack"); }
		}

		private EffectVectorVariable materialBackSpecularVariable;
		public EffectVectorVariable MaterialBackSpecularVariable {
			get { return GetVariable(ref materialBackSpecularVariable, "vMaterialSpecularBack"); }
		}

		private EffectVectorVariable materialBackReflectVariable;
		public EffectVectorVariable MaterialBackReflectVariable {
			get { return GetVariable(ref materialBackReflectVariable, "vMaterialReflectBack"); }
		}

		private EffectScalarVariable materialBackShininessVariable;
		public EffectScalarVariable MaterialBackShininessVariable {
			get { return GetVariable(ref materialBackShininessVariable, "sMaterialShininessBack"); }
		}

		private EffectScalarVariable hasDiffuseMapVariableBack;
		public EffectScalarVariable HasDiffuseMapVariableBack {
			get { return GetVariable(ref hasDiffuseMapVariableBack, "bHasDiffuseMapBack"); }
		}

		private EffectScalarVariable hasNormalMapVariableBack;
		public EffectScalarVariable HasNormalMapVariableBack {
			get { return GetVariable(ref hasNormalMapVariableBack, "bHasNormalMapBack"); }
		}

		private EffectScalarVariable hasDisplacementMapVariableBack;
		public EffectScalarVariable HasDisplacementMapVariableBack {
			get { return GetVariable(ref hasDisplacementMapVariableBack, "bHasDisplacementMapBack"); }
		}

		private EffectShaderResourceVariable texDiffuseMapVariableBack;
		public EffectShaderResourceVariable TexDiffuseMapVariableBack {
			get { return GetVariable(ref texDiffuseMapVariableBack, "texDiffuseMapBack"); }
		}

		private EffectShaderResourceVariable texNormalMapVariableBack;
		public EffectShaderResourceVariable TexNormalMapVariableBack {
			get { return GetVariable(ref texNormalMapVariableBack, "texNormalMapBack"); }
		}

		private EffectShaderResourceVariable texDisplacementMapVariableBack;
		public EffectShaderResourceVariable TexDisplacementMapVariableBack {
			get { return GetVariable(ref texDisplacementMapVariableBack, "texDisplacementMapBack"); }
		}

		#endregion


		#region Lights

		private EffectScalarVariable lightCount;
		public EffectScalarVariable LightCount
		{
			get { return GetVariable(ref lightCount, "iLightCount"); }
		}

		private EffectVectorVariable lightAmbient;
		public EffectVectorVariable LightAmbient
		{
			get { return GetVariable(ref lightAmbient, "vLightAmbient"); }
		}

		private EffectVectorVariable lightDir;
		public EffectVectorVariable LightDir
		{
			get { return GetVariable(ref lightDir, "vLightDir"); }
		}

		private EffectVectorVariable lightColor;
		public EffectVectorVariable LightColor
		{
			get { return GetVariable(ref lightColor, "vLightColor"); }
		}

		private EffectScalarVariable lightType;
		public EffectScalarVariable LightType
		{
			get { return GetVariable(ref lightType, "iLightType"); }
		}

		private EffectScalarVariable hasShadowMapVariable;
		public EffectScalarVariable HasShadowMapVariable
		{
			get { return GetVariable(ref hasShadowMapVariable, "bHasShadowMap"); }
		}

		private EffectMatrixVariable lightView;
		public EffectMatrixVariable LightView
		{
			get { return GetVariable(ref lightView, "mLightView"); }
		}

		private EffectMatrixVariable lightProj;
		public EffectMatrixVariable LightProj
		{
			get { return GetVariable(ref lightProj, "mLightProj"); }
		}

		private EffectShaderResourceVariable texShadowMapVariable;
		public EffectShaderResourceVariable TexShadowMapVariable
		{
			get { return GetVariable(ref texShadowMapVariable, "texShadowMap"); }
		}

		private EffectVectorVariable lightPos;
		public EffectVectorVariable LightPos
		{
			get { return GetVariable(ref lightPos, "vLightPos"); }
		}

		private EffectVectorVariable lightAtt;
		public EffectVectorVariable LightAtt
		{
			get { return GetVariable(ref lightAtt, "vLightAtt"); }
		}

		private EffectVectorVariable shadowMapInfoVariable;
		public EffectVectorVariable ShadowMapInfoVariable
		{
			get { return GetVariable(ref shadowMapInfoVariable, "vShadowMapInfo"); }
		}

		private EffectVectorVariable shadowMapSizeVariable;
		public EffectVectorVariable ShadowMapSizeVariable
		{
			get { return GetVariable(ref shadowMapSizeVariable, "vShadowMapSize"); }
		}

		private EffectVectorVariable lightSpot;
		public EffectVectorVariable LightSpot
		{
			get { return GetVariable(ref lightSpot, "vLightSpot"); }
		}

		#endregion

		#region Planes

		private EffectScalarVariable nPlaneCount;
		public EffectScalarVariable PlaneCount
		{
			get { return GetVariable(ref nPlaneCount, "nPlaneCount"); }
		}

		private EffectScalarVariable vPlaneDs;
		public EffectScalarVariable PlaneDs
		{
			get { return GetVariable(ref vPlaneDs, "vPlaneDs"); }
		}

		private EffectVectorVariable vPlaneNormals;
		public EffectVectorVariable PlaneNormals
		{
			get { return GetVariable(ref vPlaneNormals, "vPlaneNormals"); }
		}

		private EffectVectorVariable vPlaneColors;
		public EffectVectorVariable PlaneColors
		{
			get { return GetVariable(ref vPlaneColors, "vPlaneColors"); }
		}

		private EffectScalarVariable fPlaneLineWidth;
		public EffectScalarVariable PlaneLineWidth
		{
			get { return GetVariable(ref fPlaneLineWidth, "fPlaneLineWidth"); }
		}

		#endregion

        #region detecting object in bounding box

        private EffectScalarVariable nBoxCount;
		public EffectScalarVariable BoxCount
		{
            get { return GetVariable(ref nBoxCount, "nBoxCount"); }
        }

        private EffectVectorVariable vBoxesMinimums;
		public EffectVectorVariable BoxesMinimums
		{
            get { return GetVariable(ref vBoxesMinimums, "vBoxesMinimums"); }
        }

        private EffectVectorVariable vBoxesMaximums;
		public EffectVectorVariable BoxesMaximums
		{
            get { return GetVariable(ref vBoxesMaximums, "vBoxesMaximums"); }
        }

        private EffectVectorVariable vBoxesColors;
		public EffectVectorVariable BoxesColors
		{
            get { return GetVariable(ref vBoxesColors, "vBoxesColors"); }
        }

        private EffectScalarVariable vBoxBlockRadius;
		public EffectScalarVariable BoxBlockRadius
		{
            get { return GetVariable(ref vBoxBlockRadius, "vBoxBlockRadius"); }
        }

		private EffectScalarVariable iCilidricalColoringAxis;
		public EffectScalarVariable CilidricalColoringAxis {
			get { return GetVariable(ref iCilidricalColoringAxis, "iCilidricalColoringAxis"); }
		}

        #endregion

        #region detecting object in bounding box
        public OutOfBoxParameters OutOfBoxParams { get; private set; }

	    public sealed class OutOfBoxParameters {
            //common.fx
            //        float4 vOutOfBoxesMinimums;
            //        float4 vOutOfBoxesMaximums;
            //        float4 vOutOfBoxesColors;
		    private EffectScalarVariable radius;
		    public EffectScalarVariable Radius {
				get { return effect.GetVariable(ref radius, "vOutOfBoxesRadius"); }
		    }

            private EffectVectorVariable min;
            public EffectVectorVariable Min {
                get { return effect.GetVariable(ref min, "vOutOfBoxesMinimums"); }
            }
            private EffectVectorVariable max;
            public EffectVectorVariable Max {
                get { return effect.GetVariable(ref max, "vOutOfBoxesMaximums"); }
            }
            private EffectVectorVariable color;
            public EffectVectorVariable Color {
                get { return effect.GetVariable(ref color, "vOutOfBoxesColors"); }
            }

	        private readonly EffectVariables effect;

	        public OutOfBoxParameters(EffectVariables effect) { this.effect = effect; }
	    }



        #endregion

        #region Illumination settings

        private EffectVectorVariable illumDiffuse;
		public EffectVectorVariable IllumDiffuse
		{
			get { return GetVariable(ref illumDiffuse, "illumDiffuse"); }
		}

		private EffectScalarVariable illumShine;
		public EffectScalarVariable IllumShine
		{
			get { return GetVariable(ref illumShine, "illumShine"); }
		}

		private EffectVectorVariable illumSpecular;
		public EffectVectorVariable IllumSpecular
		{
			get { return GetVariable(ref illumSpecular, "illumSpecular"); }
		}

		#endregion
	}
}
