using System;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Extensions
{
	public static class Vector2Extensions
	{
		// Default invalid Vector2 value
		public static readonly Vector2 NaN = new Vector2(float.NaN, float.NaN);

		// Maximum distance between points to be interpreted as equal
		public const double Tolerance = .000001;
		public const double ToleranceSq = Tolerance * Tolerance;


		// Conversions from Vector2 to different types
		public static System.Windows.Point ToPoint(this Vector2 v)
		{
			return new System.Windows.Point(v.X, v.Y);
		}

		public static System.Windows.Vector ToVector(this Vector2 v)
		{
			return new System.Windows.Vector(v.X, v.Y);
		}

		// Resolved conflict with ToVector3 defined in HelixToolkit.Wpf.SharpDX.VectorExtensions 
		public static global::SharpDX.Vector3 ToVector3(this Vector2 v, float z = 0)
		{
			return new global::SharpDX.Vector3(v.X, v.Y, z);
		}

		public static System.Windows.Media.Media3D.Vector3D ToVector3D(this Vector2 v, float z = 0)
		{
			return new System.Windows.Media.Media3D.Vector3D(v.X, v.Y, z);
		}

		// Analysis
		public static bool IsNaN(this Vector2 v)
		{
			return float.IsNaN(v.X) || float.IsNaN(v.Y);
		}

		public static float AngleBetween(this Vector2 v0, Vector2 v1)
		{
			return (float)System.Windows.Vector.AngleBetween(v0.ToVector(), v1.ToVector());
		}

		public static float AngleBetween(this Vector2 v, Vector2 v0, Vector2 v1)
		{
			return (v0 - v).AngleBetween(v1 - v);
		}

		public static float DistanceSq(this Vector2 v0, Vector2 v1)
		{
			var dx = v1.X - v0.X;
			var dy = v1.Y - v0.Y;
			return dx * dx + dy * dy;
		}

		public static float DistanceTo(this Vector2 v0, Vector2 v1)
		{
			return (float)Math.Sqrt(DistanceSq(v0, v1));
		}

		/// <summary>
		/// Returns the distance from point v to a line, v0 and v1 lie on the line
		/// </summary>
		/// <param name="v">point for test</param>
		/// <param name="v0">first point on line</param>
		/// <param name="v1">second point on line</param>
		/// <returns>distance from point to a line, if 0 - than point lies on line, distance sign indicates which side of the line lies the point </returns>
		public static float DistanceToLine(this Vector2 v, Vector2 v0, Vector2 v1)
		{
			var dx = v1.X - v0.X;
			var dy = v1.Y - v0.Y;
			var c = v0.X * v1.Y - v1.X * v0.Y;
			return (dx * v.Y - dy * v.X + c) / (float)Math.Sqrt(dy * dy + dx * dx);
		}

		public static Vector2 Intersection(this Vector2 v0, Vector2 v1, Vector2 a, Vector2 b)
		{
			if (a == v0 || b == v0 || a == v1 || b == v1)
				return NaN;

			var s1_x = v1.X - v0.X;
			var s1_y = v1.Y - v0.Y;
			var s2_x = b.X - a.X;
			var s2_y = b.Y - a.Y;

			var s = (s1_x * (v0.Y - a.Y) - s1_y * (v0.X - a.X)) / (s1_x * s2_y - s2_x * s1_y);
			var t = (s2_x * (v0.Y - a.Y) - s2_y * (v0.X - a.X)) / (s1_x * s2_y - s2_x * s1_y);

			if (s < 0 || s > 1 || t < 0 || t > 1)
				return NaN;

			return new Vector2(v0.X + (t * s1_x), v0.Y + (t * s1_y));
		}

		public static Tuple<float, Vector2> GetClosestPointOfLine(this Vector2 v, Vector2 v0, Vector2 v1)
		{
			// Main formula: P = P1 + u(P2 - P1)
			// Solution:
			// u = ((X0 - X1)(X2 - X1) + (Y0 - Y1)(Y2 - Y1)) / ((X2 - X1)(X2 - X1) + (Y2 - Y1)(Y2 - Y1))
			// x = x1 + u * (x2 - x1)
			// y = y1 + u * (y2 - y1)
			var u = ((v.X - v0.X) * (v1.X - v0.X) + (v.Y - v0.Y) * (v1.Y - v0.Y)) / v0.DistanceSq(v1);
			var p = new Vector2(v0.X + u * (v1.X - v0.X), v0.Y + u * (v1.Y - v0.Y));
			return new Tuple<float, Vector2>(v.DistanceTo(p), p);
		}

		/// <summary>
		/// Function returns the closest point from v to any point of the segment v0..v1,
		/// including all points along the segment (between vertices), and distance
		/// </summary>
		/// <param name="v"></param>
		/// <param name="v0">Begin point of the segment</param>
		/// <param name="v1">End point of the segment</param>
		/// <returns>The closest point coordinates and a distance to it</returns>
		public static Tuple<float, Vector2> GetClosestPointOfSegment(this Vector2 v, Vector2 v0, Vector2 v1)
		{
			var closest = GetClosestPointOfLine(v, v0, v1);
			var p = closest.Item2;
			if (((p.X >= v0.X) && (p.X <= v1.X)) || ((p.X >= v1.X) && (p.X <= v0.X)))
				return closest;
			var distance2v0 = v.DistanceSq(v0);
			var distance2v1 = v.DistanceSq(v1);
			if (distance2v0 < distance2v1)
				return new Tuple<float, Vector2>((float)Math.Sqrt(distance2v0), v0);
			return new Tuple<float, Vector2>((float)Math.Sqrt(distance2v1), v1);
		}

		// Transformations
		public static Vector2 Normalized(this Vector2 v)
		{
			return new Vector2(v.X, v.Y) / v.Length();
		}

		public static Vector2 Inverted(this Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		public static Vector2 RotatedLeft90(this Vector2 v)
		{
			return new Vector2(-v.Y, v.X);
		}

		public static Vector2 RotatedRight90(this Vector2 v)
		{
			return new Vector2(v.Y, -v.X);
		}

		/// <summary>
		/// Rotate vector on angle defined in grad
		/// </summary>
		/// <param name="v">Source vector</param>
		/// <param name="angle">Angle in grad</param>
		/// <returns></returns>
		public static Vector2 Rotated(this Vector2 v, double angle)
		{
			return RotatedInRad(v, angle * Math.PI / 180);
		}

		/// <summary>
		/// Rotate vector on angle defined in radians
		/// </summary>
		/// <param name="v">Source vector</param>
		/// <param name="angle">Angle in radians</param>
		/// <returns></returns>
		public static Vector2 RotatedInRad(this Vector2 v, double angle)
		{
			var cos = Math.Cos(angle);
			var sin = Math.Sin(angle);
			return new Vector2((float)(v.X * cos - v.Y * sin), (float)(v.X * sin + v.Y * cos));
		}

		public static Vector2 MiddlePoint(this Vector2 source, Vector2 target)
		{
			return new Vector2((source.X + target.X) / 2, (source.Y + target.Y) / 2);
		}

		public static Vector2 GetIntersectionPointOfTwoLines(Vector2 p1_1, Vector2 p1_2, Vector2 p2_1, Vector2 p2_2, out LineIntersectionStyle state)
		{
			return GetIntersectionPointOfTwoLines(ref p1_1, ref p1_2, ref p2_1, ref p2_2, out state);
		}

		/// <summary>
		/// Return intersection's point of two lines
		/// </summary>
		/// <param name="p1_1">First point of first line</param>
		/// <param name="p1_2">Second point of first line</param>
		/// <param name="p2_1">First point of second line</param>
		/// <param name="p2_2">Second point of second line</param>
		/// <param name="state">State of returned point</param>
		/// <returns>Intersection's point</returns>
		public static Vector2 GetIntersectionPointOfTwoLines(ref Vector2 p1_1, ref Vector2 p1_2, ref Vector2 p2_1, ref Vector2 p2_2, out LineIntersectionStyle state)
		{
			float m = ((p2_2.X - p2_1.X) * (p1_1.Y - p2_1.Y) - (p2_2.Y - p2_1.Y) * (p1_1.X - p2_1.X));
			float w = ((p1_2.X - p1_1.X) * (p1_1.Y - p2_1.Y) - (p1_2.Y - p1_1.Y) * (p1_1.X - p2_1.X)); //Можно обойтись и без этого
			float n = ((p2_2.Y - p2_1.Y) * (p1_2.X - p1_1.X) - (p2_2.X - p2_1.X) * (p1_2.Y - p1_1.Y));

			float Ua = m / n;
			float Ub = w / n;

			if ((n == 0) && (m != 0))
			{
				state = LineIntersectionStyle.Parallel;
				return default(Vector2);
			}

			if ((m == 0) && (n == 0))
			{
				state = LineIntersectionStyle.Сoincide;
				return default(Vector2);
			}

			var r = new Vector2(
				p1_1.X + Ua * (p1_2.X - p1_1.X),
				p1_1.Y + Ua * (p1_2.Y - p1_1.Y));

			// Check intersection bounds
			var min_x_1 = Math.Min(p1_1.X, p1_2.X);
			var max_x_1 = Math.Max(p1_1.X, p1_2.X);
			var min_y_1 = Math.Min(p1_1.Y, p1_2.Y);
			var max_y_1 = Math.Max(p1_1.Y, p1_2.Y);
			var min_x_2 = Math.Min(p2_1.X, p2_2.X);
			var max_x_2 = Math.Max(p2_1.X, p2_2.X);
			var min_y_2 = Math.Min(p2_1.Y, p2_2.Y);
			var max_y_2 = Math.Max(p2_1.Y, p2_2.Y);

			if (r.X >= min_x_1 && r.X <= max_x_1 &&
				r.Y >= min_y_1 && r.Y <= max_y_1 &&
				r.X >= min_x_2 && r.X <= max_x_2 &&
				r.Y >= min_y_2 && r.Y <= max_y_2)
			{
				state = LineIntersectionStyle.Intersect;
			}
			else
			{
				state = LineIntersectionStyle.IntersectOutBounds;
			}
			return r;
		}
	}

	public enum LineIntersectionStyle
	{
		Parallel,
		Сoincide,
		Intersect,
		IntersectOutBounds,
	}
}
