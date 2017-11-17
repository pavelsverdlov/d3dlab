// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectionCamera.cs" company="">
//   
// </copyright>
// <summary>
//   An abstract base class for perspective and orthographic projection cameras.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf.SharpDX
{
	using System;
using System.Windows;
using System.Windows.Media.Media3D;

	/// <summary>
	/// An abstract base class for perspective and orthographic projection cameras.
	/// </summary>
	public abstract class ProjectionCamera : Camera
	{
		/// <summary>
		/// The create left hand system property
		/// </summary>
		public static readonly DependencyProperty CreateLeftHandSystemProperty =
		    DependencyProperty.Register(
		        "CreateLeftHandSystem", typeof(bool), typeof(ProjectionCamera), new PropertyMetadata(false, CameraChangedStatic));

		/// <summary>
		/// The far plane distance property.
		/// </summary>
		public static readonly DependencyProperty FarPlaneDistanceProperty =
		    DependencyProperty.Register(
		        "FarPlaneDistance", typeof(double), typeof(ProjectionCamera), new PropertyMetadata(1e3, CameraChangedStatic));

		/// <summary>
		/// The look direction property
		/// </summary>
		public static readonly DependencyProperty LookDirectionProperty = DependencyProperty.Register(
		    "LookDirection", typeof(Vector3D), typeof(ProjectionCamera), new PropertyMetadata(new Vector3D(), CameraChangedStatic));

		/// <summary>
		/// The near plane distance property
		/// </summary>
		public static readonly DependencyProperty NearPlaneDistanceProperty =
		    DependencyProperty.Register(
		        "NearPlaneDistance", typeof(double), typeof(ProjectionCamera), new PropertyMetadata(1e-1, CameraChangedStatic));

		/// <summary>
		/// The position property
		/// </summary>
		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
		    "Position",
		    typeof(Point3D),
		    typeof(ProjectionCamera),
		    new PropertyMetadata(new Point3D(), CameraChangedStatic));

		/// <summary>
		/// Up direction property
		/// </summary>
		public static readonly DependencyProperty UpDirectionProperty = DependencyProperty.Register(
		    "UpDirection", typeof(Vector3D), typeof(ProjectionCamera), new PropertyMetadata(new Vector3D(), CameraChangedStatic));

		private bool createLeftHandSystem;
		/// <summary>
		/// Gets or sets a value indicating whether to create a left hand system.
		/// </summary>
		/// <value>
		/// <c>true</c> if creating a left hand system; otherwise, <c>false</c>.
		/// </value>
		public bool CreateLeftHandSystem
		{
			get { return createLeftHandSystem; }
			set
			{
				if (createLeftHandSystem == value)
					return;
				var oldValue = createLeftHandSystem;
				createLeftHandSystem = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(CreateLeftHandSystemProperty, oldValue, value));
			}
		}
		//{
		//    get { return (bool)this.GetValue(CreateLeftHandSystemProperty); }
		//    set { this.SetValue(CreateLeftHandSystemProperty, value); }
		//}

		private double farPlaneDistance;
		/// <summary>
		/// Gets or sets the far plane distance.
		/// </summary>
		/// <value>
		/// The far plane distance.
		/// </value>
		public double FarPlaneDistance
		{
			get { return farPlaneDistance; }
			set
			{
				if (farPlaneDistance == value)
					return;
				var oldValue = farPlaneDistance;
				farPlaneDistance = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(FarPlaneDistanceProperty, oldValue, value));
			}
		}
		//{
		//	get { return (double)this.GetValue(FarPlaneDistanceProperty); }
		//	set { this.SetValue(FarPlaneDistanceProperty, value); }
		//}

		private Vector3D lookDirection;
		/// <summary>
		/// Gets or sets the look direction.
		/// </summary>
		/// <value>
		/// The look direction.
		/// </value>
		public override Vector3D LookDirection
		{
			get { return lookDirection; }
			set
			{
				if (lookDirection == value)
					return;
				var oldValue = lookDirection;
				lookDirection = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(LookDirectionProperty, oldValue, value));
			}
		}
		//{
		//	get { return (Vector3D)this.GetValue(LookDirectionProperty); }
		//	set { this.SetValue(LookDirectionProperty, value); }
		//}

		private double nearPlaneDistance;
		/// <summary>
		/// Gets or sets the near plane distance.
		/// </summary>
		/// <value>
		/// The near plane distance.
		/// </value>
		public double NearPlaneDistance
		{
			get { return nearPlaneDistance; }
			set
			{
				if (nearPlaneDistance == value)
					return;
				var oldValue = nearPlaneDistance;
				nearPlaneDistance = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(NearPlaneDistanceProperty, oldValue, value));
			}
		}
		//{
		//	get { return (double)this.GetValue(NearPlaneDistanceProperty); }
		//	set { this.SetValue(NearPlaneDistanceProperty, value); }
		//}

		private Point3D position;
		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>
		/// The position.
		/// </value>
		public override Point3D Position
		{
			get { return position; }
			set
			{
				if (position == value)
					return;
				var oldValue = position;
				position = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(PositionProperty, oldValue, value));
			}
		}
		//{
		//	get { return (Point3D)this.GetValue(PositionProperty); }
		//	set { this.SetValue(PositionProperty, value); }
		//}

		/// <summary>
		/// Gets the target position.
		/// </summary>
		/// <value>
		/// The target.
		/// </value>
		public Point3D Target
		{
			get { return this.Position + this.LookDirection; }
		}

		private Vector3D upDirection;
		/// <summary>
		/// Gets or sets up direction.
		/// </summary>
		/// <value>
		/// Up direction.
		/// </value>
		public override Vector3D UpDirection
		{
			get { return upDirection; }
			set
			{
				if (upDirection == value)
					return;
				var oldValue = upDirection;
				upDirection = value;
				OnCameraChanged(new DependencyPropertyChangedEventArgs(UpDirectionProperty, oldValue, value));
			}
		}

		/// <summary>
		/// Creates the view matrix.
		/// </summary>
		/// <returns>
		/// A Matrix.
		/// </returns>
		public override global::SharpDX.Matrix CreateViewMatrix()
		{
			if (this.CreateLeftHandSystem)
			{
				return global::SharpDX.Matrix.LookAtLH(
					this.Position.ToVector3(),
					(this.Position + this.LookDirection).ToVector3(),
					this.UpDirection.ToVector3());
			}

			return global::SharpDX.Matrix.LookAtRH(
				this.Position.ToVector3(),
				(this.Position + this.LookDirection).ToVector3(),
				this.UpDirection.ToVector3());
		}

		/// <summary>
		/// Handles camera changes.
		/// </summary>
		/// <param name="obj">
		/// The sender.
		/// </param>
		/// <param name="args">
		/// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
		/// </param>
		protected static void CameraChangedStatic(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((ProjectionCamera)obj).OnCameraChanged(args);
		}

		/// <summary>
		/// The camera changed.
		/// </summary>
		/// <param name="args">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		protected void OnCameraChanged(DependencyPropertyChangedEventArgs args)
		{
			if (CameraChanged != null)
				CameraChanged(this, EventArgs.Empty);
		}

		public event EventHandler CameraChanged;
	}
}