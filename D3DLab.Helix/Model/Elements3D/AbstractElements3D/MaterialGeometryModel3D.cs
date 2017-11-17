using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Extensions;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public abstract class MaterialGeometryModel3D : GeometryModel3D
	{
		public MaterialGeometryModel3D()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public bool RenderDiffuseMap
		{
			get { return (bool)this.GetValue(RenderDiffuseMapProperty); }
			set { this.SetValue(RenderDiffuseMapProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty RenderDiffuseMapProperty =
			DependencyProperty.Register("RenderDiffuseMap", typeof(bool), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(true));

		/// <summary>
		/// 
		/// </summary>
		public bool RenderNormalMap
		{
			get { return (bool)this.GetValue(RenderNormalMapProperty); }
			set { this.SetValue(RenderNormalMapProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty RenderNormalMapProperty =
			DependencyProperty.Register("RenderNormalMap", typeof(bool), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(false));

		/// <summary>
		/// 
		/// </summary>
		public bool RenderDisplacementMap
		{
			get { return (bool)this.GetValue(RenderDisplacementMapProperty); }
			set { this.SetValue(RenderDisplacementMapProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty RenderDisplacementMapProperty =
			DependencyProperty.Register("RenderDisplacementMap", typeof(bool), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(false));

		/// <summary>
		/// 
		/// </summary>
		public Material BackMaterial
		{
			get { return (Material)this.GetValue(BackMaterialProperty); }
			set { this.SetValue(BackMaterialProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty BackMaterialProperty =
			DependencyProperty.Register("BackMaterial", typeof(Material), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(BackMaterialChanged));

		/// <summary>
		/// 
		/// </summary>
		protected static void BackMaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public Material Material {
			get { return (Material)this.GetValue(MaterialProperty); }
			set { this.SetValue(MaterialProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty MaterialProperty =
			DependencyProperty.Register("Material", typeof(Material), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(MaterialChanged));

		/// <summary>
		/// 
		/// </summary>
		protected static void MaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		}



		/// <summary>
		/// 
		/// </summary>
		public Vector2 TextureCoodScale
		{
			get { return (Vector2)this.GetValue(TextureCoodScaleProperty); }
			set { this.SetValue(TextureCoodScaleProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty TextureCoodScaleProperty =
			DependencyProperty.Register("TextureCoodScale", typeof(Vector2), typeof(MaterialGeometryModel3D), new UIPropertyMetadata(new Vector2(1, 1)));

		/// <summary>
		/// 
		/// </summary>        
		public override bool HitTest(Ray rayWS, ref List<HitTestResult> hits)
		{
			if ((this.Instances != null) && (this.Instances.Any()))
			{
				bool hit = false;
				foreach (var modelMatrix in Instances)
				{
					this.PushMatrix(modelMatrix);
					if (base.HitTest(rayWS, ref hits))
					{
						hit = true;
						var lastHit = hits[hits.Count - 1];
						lastHit.Tag = modelMatrix;
						hits[hits.Count - 1] = lastHit;
					}
					this.PopMatrix();
				}

				return hit;
			}
			else
			{
				return base.HitTest(rayWS, ref hits);
			}
		}

		public override void Update(RenderContext renderContext, System.TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;
			var renderData = (MaterialGeometryRenderData)RenderData;

			renderData.TextureCoordScale = TextureCoodScale;

			if (GetType(renderData.Material) != GetType(Material))
				renderData.Material = Material.CloneMaterial();
			else if (Material != null)
				renderData.Material.Assign(Material);

			if(GetType(renderData.BackMaterial) != GetType(BackMaterial))
				renderData.BackMaterial = BackMaterial.CloneMaterial();
			else if(BackMaterial != null)
				renderData.BackMaterial.Assign(BackMaterial);
		}

		private static Type GetType(object m)
		{
			if (m == null)
				return null;
			return m.GetType();
		}
	}
}
