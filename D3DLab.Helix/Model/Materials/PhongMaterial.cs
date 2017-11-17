namespace HelixToolkit.Wpf.SharpDX
{
	using global::SharpDX;
	using System.Windows;
	using System.Windows.Media.Imaging;
	using System;


	/// <summary>
	/// Implments a phong-material with its all properties
	/// Includes Diffuse, Normal, Displacement, Specular, etc. maps
	/// </summary>
	[Serializable]
	public partial class PhongMaterial : Material
	{
		
		/// <summary>
		/// Constructs a Shading Material which correspnds with 
		/// the Phong and BlinnPhong lighting models.
		/// </summary>
		public PhongMaterial()
		{
			DiffuseColor = (Color4)Color.Gray;
			AmbientColor = (Color4)Color.Gray;
			EmissiveColor = (Color4)Color.Black;
			SpecularColor = (Color4)Color.Black;
			SpecularShininess = 30f;
			ReflectiveColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
		}

		/// <summary>
		/// Gets or sets a color that represents how the material reflects System.Windows.Media.Media3D.AmbientLight.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
		/// </summary>
		public Color4 AmbientColor { get; set; }
		//{
		//    get { return (Color4)this.GetValue(AmbientColorProperty); }
		//    set { this.SetValue(AmbientColorProperty, value); }
		//}

		/// <summary>
		/// Gets or sets the diffuse color for the material.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
		/// </summary>
		public Color4 DiffuseColor { get; set; }
		//{
		//    get { return (Color4)this.GetValue(DiffuseColorProperty); }
		//    set { this.SetValue(DiffuseColorProperty, value); }
		//}

		/// <summary>
		/// Gets or sets the emissive color for the material.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
		/// </summary>
		public Color4 EmissiveColor { get; set; }
		//{
		//    get { return (Color4)this.GetValue(EmissiveColorProperty); }
		//    set { this.SetValue(EmissiveColorProperty, value); }
		//}

		/// <summary>
		/// A fake parameter for reflectivity of the environment map
		/// </summary>
		public Color4 ReflectiveColor { get; set; }
		//{
		//    get { return (Color4)this.GetValue(ReflectiveColorProperty); }
		//    set { this.SetValue(ReflectiveColorProperty, value); }
		//}

		/// <summary>
		/// Gets or sets the specular color for the material.
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
		/// </summary>
		public Color4 SpecularColor { get; set; }
		//{
		//    get { return (Color4)this.GetValue(SpecularColorProperty); }
		//    set { this.SetValue(SpecularColorProperty, value); }
		//}

		/// <summary>
		/// The power of specular reflections. 
		/// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
		/// </summary>
		public float SpecularShininess { get; set; }
		//{
		//    get { return (float)this.GetValue(SpecularShininessProperty); }
		//    set { this.SetValue(SpecularShininessProperty, value); }
		//}

		private BitmapSource diffuseMap;
		/// <summary>
		/// System.Windows.Media.Brush to be applied as a System.Windows.Media.Media3D.Material
		/// to a 3-D model.
		/// </summary>
		public BitmapSource DiffuseMap
		{
			get { return diffuseMap; }
			set
			{
				if (diffuseMap == value)
					return;
				diffuseMap = value;
				DiffuseMapBytes = value != null
					? value.ToByteArray()
					: null;
			}
		}

		public byte[] DiffuseMapBytes { get; private set; }
		//{
		//    get { return (BitmapSource)this.GetValue(DiffuseMapProperty); }
		//    set { this.SetValue(DiffuseMapProperty, value); }
		//}

		/// <summary>
		/// 
		/// </summary>
		public BitmapSource NormalMap { get; set; }
		//{
		//    get { return (BitmapSource)this.GetValue(NormalMapProperty); }
		//    set { this.SetValue(NormalMapProperty, value); }
		//}

		/// <summary>
		/// 
		/// </summary>
		public BitmapSource DisplacementMap { get; set; }
		//{
		//    get { return (BitmapSource)this.GetValue(DisplacementMapProperty); }
		//    set { this.SetValue(DisplacementMapProperty, value); }
		//}

		public override void Assign(Material other)
		{
			base.Assign(other);

			var otherPhong = other as PhongMaterial;
			if (otherPhong == null)
				return;

			AmbientColor = otherPhong.AmbientColor;
			DiffuseColor = otherPhong.DiffuseColor;
			DisplacementMap = otherPhong.DisplacementMap;
			EmissiveColor = otherPhong.EmissiveColor;
			NormalMap = otherPhong.NormalMap;
			ReflectiveColor = otherPhong.ReflectiveColor;
			SpecularColor = otherPhong.SpecularColor;
			SpecularShininess = otherPhong.SpecularShininess;
			diffuseMap = otherPhong.DiffuseMap;
			DiffuseMapBytes = otherPhong.DiffuseMapBytes;
		}

		public PhongMaterial Clone()
		{
			var result = new PhongMaterial();
			result.Assign(this);
			return result;
		}

	    public static PhongMaterial Empty() {
	        return new PhongMaterial {
	            DiffuseColor = new Color4(),
	            AmbientColor = new Color4(),
	            EmissiveColor = new Color4(),
	            ReflectiveColor = new Color4(),
	            SpecularColor = new Color4(),
	            SpecularShininess = 0
	        };
	    }
	}
}
