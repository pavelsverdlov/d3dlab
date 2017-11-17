using System;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX.Render;
using global::SharpDX;

namespace HelixToolkit.Wpf.SharpDX
{
	public abstract class Light3D : Model3D
	{
		public static readonly DependencyProperty DirectionProperty =
			DependencyProperty.Register("Direction", typeof(Vector3), typeof(Light3D), new UIPropertyMetadata(new Vector3()));

		public static readonly DependencyProperty DirectionTransformProperty =
			DependencyProperty.Register("DirectionTransform", typeof(Transform3D), typeof(Light3D), new UIPropertyMetadata(Transform3D.Identity, DirectionTransformPropertyChanged));

		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register("Color", typeof(Color4), typeof(Light3D), new UIPropertyMetadata(new Color4(0.2f, 0.2f, 0.2f, 1.0f), ColorChanged));

		public LightType LightType { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Light3D"/> class.
		/// </summary>
		public Light3D()
		{
		}

		private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Light3D)d).OnColorChanged(e);
		}

		public void OnColorChanged(DependencyPropertyChangedEventArgs e)
		{
		}

		/// <summary>
		/// Direction of the light.
		/// It applies to Directional Light and to Spot Light,
		/// for all other lights it is ignored.
		/// </summary>
		public Vector3 Direction
		{
			get { return (Vector3)this.GetValue(DirectionProperty); }
			set { this.SetValue(DirectionProperty, value); }
		}

		/// <summary>
		/// Transforms the Direction Vector of the Light.
		/// </summary>
		public Transform3D DirectionTransform
		{
			get { return (Transform3D)this.GetValue(DirectionTransformProperty); }
			set { this.SetValue(DirectionTransformProperty, value); }
		}

		/// <summary>
		/// Color of the light.
		/// For simplicity, this color applies to the diffuse and specular properties of the light.
		/// </summary>
		public Color4 Color
		{
			get { return (Color4)this.GetValue(ColorProperty); }
			set { this.SetValue(ColorProperty, value); }
		}

		/// <summary>
		/// 
		/// </summary>
		private static void DirectionTransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				var m = ((Transform3D)e.NewValue).Value;
				((Light3D)d).Direction = new Vector3((float)m.OffsetX, (float)m.OffsetY, (float)m.OffsetZ);
			}
		}

		/// <summary>
		/// Light Type.
		/// </summary>
		public enum Type : int
		{
			Ambient = 0,
			Directional = 1,
			Point = 2,
			Spot = 3,
		}

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (LightRenderData)RenderData;
			renderData.Color = Color;
			renderData.Direction = Direction;
		}
	}
}
