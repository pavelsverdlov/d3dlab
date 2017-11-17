using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Render;
using Point = System.Windows.Point;

namespace HelixToolkit.Wpf.SharpDX
{
	/// <summary>
	/// Provides a base class for a scene model which contains geometry
	/// </summary>
	public abstract class GeometryModel3D : Model3D, IHitable, IBoundable, IVisible, IThrowingShadow, ISelectable, IMouse3D
	{
		protected RasterizerState rasterState;

		/// <summary>
		/// Current geometry for thread safe access
		/// </summary>
		public Geometry3D ThreadSafe_Geometry { get; set; }

		public Geometry3D Geometry
		{
			get { return (Geometry3D)this.GetValue(GeometryProperty); }
			set
			{
				if (Geometry != null)
					Geometry.CollectionChanged -= Geometry_CollectionChanged;
				this.SetValue(GeometryProperty, value);
				ThreadSafe_Geometry = value;
				Geometry_CollectionChanged(GeometryCollectionType.Any);
				if (Geometry != null)
					Geometry.CollectionChanged += Geometry_CollectionChanged;
			}
		}

		public void Geometry_CollectionChanged(GeometryCollectionType type)
		{
			changedCollections |= type;
		    OnGeomertyChanged(type);
			if (GeometryChangedEx != null)
				GeometryChangedEx(type);
		}

	    protected virtual void OnGeomertyChanged(GeometryCollectionType type) { }

		public delegate void GeomertyChangedExDelegate(GeometryCollectionType type);
		public event GeomertyChangedExDelegate GeometryChangedEx;

        public GeometryCollectionType changedCollections;

		public override void Update(RenderContext renderContext, TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (GeometryRenderData)RenderData;

			renderData.DepthBias = DepthBias;
			renderData.Thickness = (float)Thickness;
			renderData.Smoothness = (float)Smoothness;
			renderData.Instances = (Instances != null && Instances.Any()) ? Instances.ToArray() : null;

			if (changedCollections == GeometryCollectionType.Nothing)
				return;

			foreach (var array in renderData.RenderSources.Arrays)
			{
				if (changedCollections.HasFlag(array.Type))
					array.Update(Geometry);
			}
			renderData.RenderSources.StartUpdateIfNeeded();
			changedCollections = GeometryCollectionType.Nothing;
			ReAttach();
		}

		public static readonly DependencyProperty GeometryProperty =
			DependencyProperty.Register("Geometry", typeof(Geometry3D), typeof(GeometryModel3D), new UIPropertyMetadata(GeometryChanged));

		protected static void GeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((GeometryModel3D)d).OnGeometryChanged(e);
		}

		protected virtual void OnGeometryChanged(DependencyPropertyChangedEventArgs e) {
		    OnGeometryChanged();
		}

	    public void OnGeometryChanged() {

            if (this.Geometry == null || this.Geometry.Positions == null) {
                this.Bounds = new BoundingBox();
                return;
            }

            this.Bounds = this.Geometry.Positions.ToBoundingBox();

            if (IsAttached)
                ReAttach();
        }


        protected override void OnTransformChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnTransformChanged(e);
			if (this.Geometry != null)
			{
				//var b = BoundingBox.FromPoints(this.Geometry.Positions.Select(x => Vector3.TransformCoordinate(x, this.modelMatrix)).ToArray());
				//				var b = BoundingBox.FromPoints(this.Geometry.Positions.Array);
				//				this.Bounds = b;
				//this.BoundsDiameter = (b.Maximum - b.Minimum).Length();
			}
		}

        private BoundingBox transformedBoundingCache;
	    private Vector3Collection transformedBoundingPositionsCache;
        private Matrix matrixCache;
		public BoundingBox TransformedBounds
		{
			get {
                if (matrixCache != this.modelMatrix ||
                    (this.ThreadSafe_Geometry != null && transformedBoundingPositionsCache != this.ThreadSafe_Geometry.Positions)) {
			        matrixCache = this.modelMatrix;
			        var g = this.ThreadSafe_Geometry;
			        if (g == null || g.Positions == null || g.Positions.Count == 0) {
                        transformedBoundingCache = new BoundingBox();
			            transformedBoundingPositionsCache = null;
			        } else {
                        transformedBoundingCache = g.Positions.ToBoundingBox(ref matrixCache);
                        transformedBoundingPositionsCache = g.Positions;
			        }
			        //var b = FromPoints(g.Positions, this.modelMatrix);
			    }
                return transformedBoundingCache;
			}
		}

		public BoundingBox Bounds
		{
			get { return (BoundingBox)this.GetValue(BoundsProperty); }
			protected set
			{
				this.SetValue(BoundsPropertyKey, value);
				ThreadSafeBounds = value;
			}
		}

        /// <summary>
        /// This does not work - not thread safe
        /// </summary>
		public BoundingBox ThreadSafeBounds;

		private static readonly DependencyPropertyKey BoundsPropertyKey =
			DependencyProperty.RegisterReadOnly("Bounds", typeof(BoundingBox), typeof(GeometryModel3D), new UIPropertyMetadata(new BoundingBox()));

		public static readonly DependencyProperty BoundsProperty = BoundsPropertyKey.DependencyProperty;

		public int DepthBias
		{
			get { return (int)this.GetValue(DepthBiasProperty); }
			set { this.SetValue(DepthBiasProperty, value); }
		}

		public static readonly DependencyProperty DepthBiasProperty =
			DependencyProperty.Register("DepthBias", typeof(int), typeof(GeometryModel3D), new UIPropertyMetadata(0, RasterStateChanged));

		private static void RasterStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((GeometryModel3D)d).OnRasterStateChanged((int)e.NewValue);
		}

		protected virtual void OnRasterStateChanged(int depthBias) { }

		public double Thickness
		{
			get { return (double)this.GetValue(ThicknessProperty); }
			set { this.SetValue(ThicknessProperty, value); }
		}

		public static readonly DependencyProperty ThicknessProperty =
			DependencyProperty.Register("Thickness", typeof(double), typeof(GeometryModel3D), new UIPropertyMetadata(1.0));

		public double Smoothness
		{
			get { return (double)this.GetValue(SmoothnessProperty); }
			set { this.SetValue(SmoothnessProperty, value); }
		}

		public IEnumerable<Matrix> Instances
		{
			get { return (IEnumerable<Matrix>)this.GetValue(InstancesProperty); }
			set { this.SetValue(InstancesProperty, value); }
		}

		public static readonly DependencyProperty InstancesProperty =
			DependencyProperty.Register("Instances", typeof(IEnumerable<Matrix>), typeof(GeometryModel3D), new UIPropertyMetadata(null, InstancesChanged));

		protected static void InstancesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var model = (GeometryModel3D)d;

			Matrix[] array = null;
			if (e.NewValue != null)
				array = ((IEnumerable<Matrix>)e.NewValue).ToArray();

			model.InstancesChanged();
			//model.ReAttach();
			//model.UpdateRenderData<GeometryRenderData<IRenderableGeometry>>(i => i.UpdateInstances(array));
		}

		private void InstancesChanged()
		{
			if (this.RenderData == null)
				return;
			((GeometryRenderData)this.RenderData).Instances = (Instances != null && Instances.Any() ? Instances.ToArray() : null);
		}

		public override void Attach(IRenderHost host)
		{
			base.Attach(host);
			InstancesChanged();
		}

		public static readonly DependencyProperty SmoothnessProperty =
			DependencyProperty.Register("Smoothness", typeof(double), typeof(GeometryModel3D), new UIPropertyMetadata(0.0));

		public static readonly RoutedEvent MouseDown3DEvent =
			EventManager.RegisterRoutedEvent("MouseDown3D", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Model3D));

		public static readonly RoutedEvent MouseUp3DEvent =
			EventManager.RegisterRoutedEvent("MouseUp3D", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Model3D));

		public static readonly RoutedEvent MouseMove3DEvent =
			EventManager.RegisterRoutedEvent("MouseMove3D", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Model3D));

		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register("IsSelected", typeof(bool), typeof(DraggableGeometryModel3D), new UIPropertyMetadata(false));

		/// <summary>
		/// Provide CLR accessors for the event 
		/// </summary>
		public event RoutedEventHandler MouseDown3D
		{
			add { AddHandler(MouseDown3DEvent, value); }
			remove { RemoveHandler(MouseDown3DEvent, value); }
		}

		/// <summary>
		/// Provide CLR accessors for the event 
		/// </summary>
		public event RoutedEventHandler MouseUp3D
		{
			add { AddHandler(MouseUp3DEvent, value); }
			remove { RemoveHandler(MouseUp3DEvent, value); }
		}

		/// <summary>
		/// Provide CLR accessors for the event 
		/// </summary>
		public event RoutedEventHandler MouseMove3D
		{
			add { AddHandler(MouseMove3DEvent, value); }
			remove { RemoveHandler(MouseMove3DEvent, value); }
		}

		public GeometryModel3D()
		{
			this.MouseDown3D += OnMouse3DDown;
			this.MouseUp3D += OnMouse3DUp;
			this.MouseMove3D += OnMouse3DMove;
			this.IsThrowingShadow = true;
		}

		public virtual void OnMouse3DDown(object sender, RoutedEventArgs e) { }

		public virtual void OnMouse3DUp(object sender, RoutedEventArgs e) { }

		public virtual void OnMouse3DMove(object sender, RoutedEventArgs e) { }

		/// <summary>
		/// Checks if the ray hits the geometry of the model.
		/// If there a more than one hit, result returns the hit which is nearest to the ray origin.
		/// </summary>
		/// <param name="rayWS">Hitring ray from the camera.</param>
		/// <param name="result">results of the hit.</param>
		/// <returns>True if the ray hits one or more times.</returns>
		public virtual bool HitTest(Ray rayWS, ref List<HitTestResult> hits)
		{
			if (this.Visibility == Visibility.Collapsed)
			{
				return false;
			}
			if (this.IsHitTestVisible == false)
			{
				return false;
			}

			var g = this.Geometry as MeshGeometry3D;
			var result = new HitTestResult();
			result.Distance = double.MaxValue;

			if (g != null && g.Positions != null && g.Indices != null)
			{
				var gIndices = g.Indices.ToArrayFast();
				var gPositions = g.Positions.ToArrayFast();

				var m = this.modelMatrix;
				var mr = this.modelMatrix.Inverted();

				var b = g.Bounds;

				var localRay = new Ray(Vector3.Transform(rayWS.Position, mr).ToVector3(), Vector3.TransformNormal(rayWS.Direction, mr));

				// this all happens now in world space now:
				if (localRay.Intersects(ref b))
				{
					for (int i = 0; i < gIndices.Length - 2; i += 3)
					{
						var i0 = gIndices[i];
						var i1 = gIndices[i + 1];
						var i2 = gIndices[i + 2];

						var p0 = gPositions[i0];
						var p1 = gPositions[i1];
						var p2 = gPositions[i2];

						float d;
						if (Collision.RayIntersectsTriangle(ref localRay, ref p0, ref p1, ref p2, out d))
						{
							if (d < result.Distance) // If d is NaN, the condition is false.
							{
								result.IsValid = true;
								result.ModelHit = this;
								// transform hit-info to world space now:
								result.PointHit = (rayWS.Position + (rayWS.Direction * d)).ToPoint3D();
								result.Distance = d;
								result.Vertex1 = i0;
								result.Vertex2 = i1;
								result.Vertex3 = i2;
								var n = Vector3.Cross(p1 - p0, p2 - p0);
								n = Vector3.TransformNormal(n, mr);
								n.Normalize();
								// transform hit-info to world space now:
								result.NormalAtHit = n.ToVector3D();// Vector3.TransformNormal(n, m).ToVector3D();
							}
						}
					}
				}
			}

			if (result.IsValid)
				hits.Add(result);

			return result.IsValid;
		}

		private static BoundingBox FromPoints(IEnumerable<Vector3> points)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			Vector3 minimum = new Vector3(3.40282347E+38f);
			Vector3 maximum = new Vector3(-3.40282347E+38f);

			foreach (var p in points)
			{
				var p0 = p;
				Vector3.Min(ref minimum, ref p0, out minimum);
				Vector3.Max(ref maximum, ref p0, out maximum);
			}
			return new BoundingBox(minimum, maximum);
		}

		private static BoundingBox FromPoints(IEnumerable<Vector3> points, Matrix matrix)
		{
			if (points == null)
				throw new ArgumentNullException("points");

			Vector3 minimum = new Vector3(3.40282347E+38f);
			Vector3 maximum = new Vector3(-3.40282347E+38f);

			Vector4 p1;
			foreach (var p in points)
			{
				var p0 = p;
				Vector3.Transform(ref p0, ref matrix, out p1);
				p0 = p1.ToVector3();
				Vector3.Min(ref minimum, ref p0, out minimum);
				Vector3.Max(ref maximum, ref p0, out maximum);
			}
			return new BoundingBox(minimum, maximum);
		}

		public bool IsThrowingShadow { get; set; }

		public bool IsSelected
		{
			get { return (bool)this.GetValue(IsSelectedProperty); }
			set { this.SetValue(IsSelectedProperty, value); }
		}
	}
}
