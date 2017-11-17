using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public class MeshGeometryModel3D : MaterialGeometryModel3D
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MeshGeometryModel3D"/> class.
		/// </summary>
		public MeshGeometryModel3D()
		{
		}

		private CullMode _cullMaterialMode = CullMode.Back;

		public CullMode CullMaterial
		{
			get { return _cullMaterialMode; }
			set { _cullMaterialMode = value; }
		}

		public bool DisableBlandDiffuseColors { get; set; }

		/// <summary>
		/// Gets or sets value for the shading model shading is used
		/// </summary>
		/// <value>
		/// <c>true</c> if deferred shading is enabled; otherwise, <c>false</c>.
		/// </value>
		public RenderTechnique RenderTechniqueUserDefined
		{
			get { return (RenderTechnique)this.GetValue(RenderTechniqueProperty); }
			set { this.SetValue(RenderTechniqueProperty, value); }
		}

		public static readonly DependencyProperty RenderTechniqueProperty = DependencyProperty.Register(
		 "RenderTechniqueUserDefined", typeof(RenderTechnique), typeof(MeshGeometryModel3D), new PropertyMetadata(null, (s, e) => ((MeshGeometryModel3D)s).RenderTechniqueUserDefindedPropertyChanged()));

		private void RenderTechniqueUserDefindedPropertyChanged()
		{
			//if (IsAttached)
			//	ReAttach();
            OnRenderTechniqueUserDefinedPropertyChanged();
		}

	    protected virtual void OnRenderTechniqueUserDefinedPropertyChanged() {
	    }

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new MeshGeometryRenderData(RenderTechniqueUserDefined);
		}

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (MeshGeometryRenderData)RenderData;
			renderData.CullMode = CullMaterial;
		    renderData.RenderTechniqueUserDefinded = RenderTechniqueUserDefined;//renderContext.GlobalRenderTechnique(this) ?? RenderTechniqueUserDefined;
			renderData.DisableBlandDiffuseColors = DisableBlandDiffuseColors;
		}
	}
}
