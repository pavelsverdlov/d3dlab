// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrthographicCamera.cs" company="">
//   
// </copyright>
// <summary>
//   Represents an orthographic projection camera.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using HelixToolkit.Wpf.SharpDX.Extensions;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace HelixToolkit.Wpf.SharpDX {
    using System.Windows;

    using global::SharpDX;

    /// <summary>
    /// Represents an orthographic projection camera.
    /// </summary>
    public class OrthographicCamera : ProjectionCamera {
        /// <summary>
        /// The width property
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof(double), typeof(OrthographicCamera));

		private double width;
		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>
		/// The width.
		/// </value>
		public double Width
		{
			get { return width; }
			set
			{
				if (width == value)
					return;
				var oldValue = width;
				width = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(WidthProperty, oldValue, width));
			}
		}
		//{
  //          get { return (double)this.GetValue(WidthProperty); }
  //          set { this.SetValue(WidthProperty, value); }
  //      }

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.NewValue is double && (double)e.NewValue < 0.001)
				return;
			if (e.NewValue is Point3D && (Point3D)e.NewValue == new Point3D())
				return;
			if (e.NewValue is Vector3D && ((Vector3D)e.NewValue).Length < 0.001)
				return;

			if (e.Property == WidthProperty)
				Width = (double)e.NewValue;
			else if (e.Property == PositionProperty)
				Position = (Point3D)e.NewValue;
			else if (e.Property == LookDirectionProperty)
				LookDirection = (Vector3D)e.NewValue;
			else if (e.Property == UpDirectionProperty)
				UpDirection = (Vector3D)e.NewValue;
		}

        public OrthographicCamera() {
            // default values for near-far must be different for ortho:
            NearPlaneDistance = -10.0;
            FarPlaneDistance = 100.0;
        }

        /// <summary>
        /// Creates the projection matrix.
        /// </summary>
        /// <param name="aspectRatio">
        /// The aspect ratio.
        /// </param>
        /// <returns>
        /// A <see cref="Matrix"/>.
        /// </returns>
        public override Matrix CreateProjectionMatrix(double aspectRatio) {
            if(this.CreateLeftHandSystem) {
                return Matrix.OrthoLH(
                    (float)this.Width,
                    (float)(this.Width / aspectRatio),
                    (float)this.NearPlaneDistance,
                    (float)this.FarPlaneDistance);
            }
            float halfWidth = (float) (Width * 0.5f);
            float halfHeight = (float) ((this.Width / aspectRatio) * 0.5f);
            Matrix projection;
            OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, (float)this.NearPlaneDistance, (float)this.FarPlaneDistance,out projection);
            return projection;
        }

        //the right way to calculate orthographic projection matrix
        public static void OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result) {
            float zRange = -2.0f / (zfar - znear);

            result = Matrix.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
            result.M33 = zRange;
            result.M41 = ((left + right) / (left - right));
            result.M42 = ((top + bottom) / (bottom - top));
            result.M43 = -znear * zRange;
        }

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.
        /// </summary>
        /// <returns>
        /// The new instance.
        /// </returns>
        protected override Freezable CreateInstanceCore() {
            return new OrthographicCamera();
        }

		//private LineGeometryModel3D lineGeometryModel3D = new LineGeometryModel3D() { Color = Color.Red, Thickness = 5 };
		//public LineGeometryModel3D LineGeometryModel3D {
        //    get { return lineGeometryModel3D; }
        //    set { lineGeometryModel3D = value; }
        //}

        //public void DrawFrustrum(Viewport3DX viewport) {
        //    var bf = new BoundingFrustum(this.GetViewProjectionMatrix(viewport.ActualWidth / viewport.ActualHeight));
        //    var corners = bf.GetCorners().ToList();
        //    LineBuilder b = new LineBuilder();
        //    b.AddLine(corners[0], corners[1]);
        //    b.AddLine(corners[1], corners[2]);
        //    b.AddLine(corners[2], corners[3]);
        //    b.AddLine(corners[3], corners[0]);

        //    b.AddLine(corners[4], corners[5]);
        //    b.AddLine(corners[5], corners[6]);
        //    b.AddLine(corners[6], corners[7]);
        //    b.AddLine(corners[7], corners[4]);

        //    b.AddLine(corners[0], corners[4]);
        //    b.AddLine(corners[1], corners[5]);
        //    b.AddLine(corners[2], corners[6]);
        //    b.AddLine(corners[3], corners[7]);
        //    LineGeometryModel3D.Geometry = b.ToLineGeometry3D();

        //}

        public bool TestBlockInView(Viewport3DX viewport, BoundingBox blockBounds) {
            var bf = new BoundingFrustum(this.GetViewProjectionMatrix(viewport.ActualWidth / viewport.ActualHeight));
            return bf.Intersects(ref blockBounds);
        }

    }
}