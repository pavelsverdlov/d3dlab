using System.Collections.Generic;
using System.Linq;
using System.Windows;
using global::SharpDX;
using global::SharpDX.Direct3D11;
using HelixToolkit.Wpf.SharpDX.Render;
using Color = global::SharpDX.Color;

namespace HelixToolkit.Wpf.SharpDX
{
	public class LineGeometryModel3D : GeometryModel3D
    {
        public Color Color
        {
            get { return (Color)this.GetValue(ColorProperty); }
            set { this.SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(LineGeometryModel3D), new UIPropertyMetadata(Color.Black, (o, e) => ((LineGeometryModel3D)o).OnColorChanged()));

        public double HitTestThickness
        {
            get { return (double)this.GetValue(HitTestThicknessProperty); }
            set { this.SetValue(HitTestThicknessProperty, value); }
        }

        public static readonly DependencyProperty HitTestThicknessProperty =
            DependencyProperty.Register("HitTestThickness", typeof(double), typeof(LineGeometryModel3D), new UIPropertyMetadata(1.0));

		public LineGeometryModel3D()
		{
			IsHitTestVisible = false;
		}

        public override bool HitTest(Ray rayWS, ref List<HitTestResult> hits)
        {
            LineGeometry3D lineGeometry3D;
           // Viewport3DX viewport = FindVisualAncestor<Viewport3DX>(this.renderHost as DependencyObject);

            if (this.Visibility == Visibility.Collapsed || !this.IsHitTestVisible ||
              //  viewport == null ||
                (lineGeometry3D = this.Geometry as LineGeometry3D) == null)
            {
                return false;
            }

            // revert unprojection; probably better: overloaded HitTest() for LineGeometryModel3D?
            var svpm = ViewportExtensions.GetScreenViewProjectionMatrix(renderHost.RenderContext.Camera,
                renderHost.RenderContext.RenderTarget.Height, renderHost.RenderContext.RenderTarget.Width);
            var smvpm = this.modelMatrix * svpm;
            var clickPoint4 = new Vector4(rayWS.Position + rayWS.Direction, 1);
            Vector4.Transform(ref clickPoint4, ref svpm, out clickPoint4);
            var clickPoint = clickPoint4.ToVector3();

            var result = new HitTestResult { IsValid = false, Distance = double.MaxValue };
            var maxDist = this.HitTestThickness;
            var lastDist = double.MaxValue;
            var index = 0;

			for (int i = 0; i < lineGeometry3D.Indices.Count - 1; i += 2)
			{
				var i0 = lineGeometry3D.Indices[i];
				var i1 = lineGeometry3D.Indices[i + 1];

				Vector3 p0, p1;
				var lp0 = lineGeometry3D.Positions[i0];
				Vector3.TransformCoordinate(ref lp0, ref this.modelMatrix, out p0);
				var lp1 = lineGeometry3D.Positions[i1];
				Vector3.TransformCoordinate(ref lp1, ref this.modelMatrix, out p1);

				float t;
                var dist = LineBuilder.GetPointToLineDistance2D(ref clickPoint, ref p0, ref p1, out t);
                if (dist < lastDist && dist <= maxDist)
                {
                    lastDist = dist;
                    Vector4 res;
                    Vector3.Transform(ref lp0, ref this.modelMatrix, out res);
                    lp0 = res.ToVector3();

                    Vector3.Transform(ref lp1, ref this.modelMatrix, out res);
                    lp1 = res.ToVector3();

                    var lv = lp1 - lp0;
                    var hitPointWS = lp0 + lv * t;
                    result.Distance = (rayWS.Position - hitPointWS).Length();
                    result.PointHit = hitPointWS.ToPoint3D();
                    result.ModelHit = this;
                    result.IsValid = true;
                    result.Tag = index; // ToDo: LineHitTag with additional info
                }

                index++;
            }

            if (result.IsValid)
            {
                hits.Add(result);
            }

            return result.IsValid;
        }

        protected override void OnRasterStateChanged(int depthBias)
        {
			//UpdateRenderData<GeometryRenderData<IRenderableGeometry>>(i => i.UpdateRasterState(DepthBias, CullMode.None));
        }

        private void OnColorChanged()
        {
			if (this.IsAttached)
				ReAttach();
        }

		internal override RenderData CreateRenderData(IRenderHost host)
		{
			return new LineGeometryRenderData();
		}

		public override void Update(RenderContext renderContext, System.TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);

			if (RenderData == null)
				return;

			var renderData = (LineGeometryRenderData)RenderData;
			renderData.Color = Color.ToColor4();
		}
	}
}
