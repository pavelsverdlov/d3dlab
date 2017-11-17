using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Extensions {
    public static class LineGeometryModel3DExtenions {
		public static LineGeometryModel3D SetGeometryByPoints(this LineGeometryModel3D lineGeometryModel3D, List<Point3D> points, bool closed = true)
		{
          
            lineGeometryModel3D.Geometry = GetLineGeometryByPoints(points, closed);
			return lineGeometryModel3D;
		}

        public static LineGeometryModel3D SetGeometryByPoints(this LineGeometryModel3D lineGeometryModel3D, IList<Vector3> points, bool closed = true)
        {
            var g = new LineBuilder();
			g.Add(points, closed);
            //var vFirst = points[0];
            //var v1 = vFirst;
            //for (int i = 1; i < points.Count; i++) {
            //    var v2 = points[i];
            //    g.AddLine(v1, v2);
            //    v1 = v2;
            //}
            //if (closed) {
            //    g.AddLine(v1, vFirst);
            //}
            lineGeometryModel3D.Geometry = g.ToLineGeometry3D();
			return lineGeometryModel3D;
		}

        public static LineGeometry3D GetLineGeometryByPoints(List<Point3D> points, bool closed = true) {
            var g = new LineBuilder();
            var p1 = points[0];
            var vFirst = new Vector3((float)p1.X, (float)p1.Y, (float)p1.Z);
            var v1 = vFirst;
            for (int i = 1; i < points.Count; i++) {
                var p2 = points[i];
                var v2 = new Vector3((float)p2.X, (float)p2.Y, (float)p2.Z);
                g.AddLine(v1, v2);
                v1 = v2;
            }

            if (closed) {
                g.AddLine(v1, vFirst);
            }

            return g.ToLineGeometry3D();
        }

        public static void SetGeometryToPoints(this LineGeometryModel3D lineGeometryModel3D, List<Point3D> points) {
            var g = new LineBuilder();
            var p1 = points[0];
            var vFirst = new Vector3((float)p1.X, (float)p1.Y, (float)p1.Z);
            var v1 = vFirst;
            for (int i = 1; i < points.Count; i++) {
                var p2 = points[i];
                var v2 = new Vector3((float)p2.X, (float)p2.Y, (float)p2.Z);
                g.AddLine(v1, v1+new Vector3(0.1f));
                v1 = v2;
            }

            //   g.AddLine(v1, vFirst);

            lineGeometryModel3D.Geometry = g.ToLineGeometry3D();
        }
        
    }
}
