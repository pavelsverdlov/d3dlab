using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using global::SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using Point = System.Windows.Point;

namespace HelixToolkit.Wpf.SharpDX
{
	public static class VectorExtensions
	{
		public static Point3D Multiply(this Point3D p, double d)
		{
			return new Point3D(p.X * d, p.Y * d, p.Z * d);
		}

		public static Vector3D ToVector3D(this Vector3 vector)
		{
			return new Vector3D(vector.X, vector.Y, vector.Z);
		}

		public static Vector3D ToVector3D(this Transform3D trafo)
		{
			var matrix = trafo.Value;
			var w = 1.0 / matrix.M44;
			return new Vector3D(w * matrix.OffsetX, w * matrix.OffsetY, w * matrix.OffsetZ);
		}

		public static Point3D ToPoint3D(this Vector3 vector)
		{
			return new Point3D(vector.X, vector.Y, vector.Z);
		}

		public static Size3D ToSize3D(this Vector3 vector)
		{
			return new Size3D(vector.X, vector.Y, vector.Z);
		}

		public static Matrix3D? ToMatrix3D(this Matrix? m)
		{
			if (m == null)
				return null;
			return m.Value.ToMatrix3D();
		}

		public static Matrix3D ToMatrix3D(this Matrix m)
		{
			return new Matrix3D(
				(float)m.M11,
				(float)m.M12,
				(float)m.M13,
				(float)m.M14,
				(float)m.M21,
				(float)m.M22,
				(float)m.M23,
				(float)m.M24,
				(float)m.M31,
				(float)m.M32,
				(float)m.M33,
				(float)m.M34,
				(float)m.M41,
				(float)m.M42,
				(float)m.M43,
				(float)m.M44);
		}

		public static System.Windows.Point ToPoint(this System.Drawing.Point value)
		{
			return new System.Windows.Point(value.X, value.Y);
		}

		public static global::SharpDX.Vector2 ToVector2(this System.Drawing.Point value)
		{
			return new global::SharpDX.Vector2(value.X, value.Y);
		}

		public static global::SharpDX.Vector2 ToVector2(this Point vector)
		{
			return new global::SharpDX.Vector2((float)vector.X, (float)vector.Y);
		}

		// ATTENTION! This extension is replaced by extension with the same name
		// in class HelixToolkit.Wpf.SharpDX.Extensions.Vector2Extensions
		//public static global::SharpDX.Vector3 ToVector3(this Vector2 vector, float z = 1.0f)
		//{
		//    return new global::SharpDX.Vector3(vector.X, vector.Y, z);
		//}

		public static global::SharpDX.Vector3 ToVector3(this Point3D point)
		{
			return new global::SharpDX.Vector3((float)point.X, (float)point.Y, (float)point.Z);
		}

		public static global::SharpDX.Vector3[] ToVector3(this IEnumerable<Point3D> points)
		{
			return points.Select(ToVector3).ToArray();
		}

		public static global::SharpDX.Vector3 ToVector3(this Vector3D vector)
		{
			return new global::SharpDX.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
		}

		public static global::SharpDX.Vector3 ToVector3(this Vector4 vector)
		{
			return new global::SharpDX.Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
		}

		public static Point3D ToPoint3D(this Vector4 vector)
		{
			return new Point3D(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
		}

		public static global::SharpDX.Vector3 ToXYZ(this Vector4 vector)
		{
			return new global::SharpDX.Vector3(vector.X, vector.Y, vector.Z);
		}

		public static global::SharpDX.Vector4 ToVector4(this Vector3D vector, float w = 1f)
		{
			return new global::SharpDX.Vector4((float)vector.X, (float)vector.Y, (float)vector.Z, w);
		}

		public static global::SharpDX.Vector4 ToVector4(this Point3D point, float w = 1f)
		{
			return new global::SharpDX.Vector4((float)point.X, (float)point.Y, (float)point.Z, w);
		}

		public static global::SharpDX.Vector4 ToVector4(this Transform3D trafo)
		{
			var matrix = trafo.Value;
			return new global::SharpDX.Vector4((float)matrix.OffsetX, (float)matrix.OffsetY, (float)matrix.OffsetZ, (float)matrix.M44);
		}

		public static global::SharpDX.Vector3 ToVector3(this Transform3D trafo)
		{
			var matrix = trafo.Value;
			return new global::SharpDX.Vector3((float)matrix.OffsetX, (float)matrix.OffsetY, (float)matrix.OffsetZ);
		}

		public static Matrix3D ToMatrix3D(this Transform3D trafo)
		{
			if (trafo == null)
				return Matrix3D.Identity;

			return trafo.Value;
		}

		public static global::SharpDX.Matrix ToMatrix(this Transform3D trafo)
		{
			if (trafo == null)
				return global::SharpDX.Matrix.Identity;

			var m = trafo.Value;
			return new global::SharpDX.Matrix(
				(float)m.M11,
				(float)m.M12,
				(float)m.M13,
				(float)m.M14,
				(float)m.M21,
				(float)m.M22,
				(float)m.M23,
				(float)m.M24,
				(float)m.M31,
				(float)m.M32,
				(float)m.M33,
				(float)m.M34,
				(float)m.OffsetX,
				(float)m.OffsetY,
				(float)m.OffsetZ,
				(float)m.M44);
		}

		public static Vector ToVectorIgnoreZ(this global::SharpDX.Vector3 vector)
		{
			return new Vector(vector.X, vector.Y);
		}

		public static global::SharpDX.Vector2 ToVector2IgnoreZ(this global::SharpDX.Vector3 vector)
		{
			return new global::SharpDX.Vector2(vector.X, vector.Y);
		}

        public static global::SharpDX.Vector2 ToVector2IgnoreZ(this global::SharpDX.Vector4 vector) {
            return new global::SharpDX.Vector2(vector.X, vector.Y);
        }

		public static global::SharpDX.Vector4 ToVector4(this global::SharpDX.Vector3 vector, float w = 1f)
		{
			return new global::SharpDX.Vector4((float)vector.X, (float)vector.Y, (float)vector.Z, w);
		}

		public static global::SharpDX.Color4 ToColor4(this global::SharpDX.Vector4 vector, float w = 1f)
		{
			return new global::SharpDX.Color4((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);
		}

		public static global::SharpDX.Color4 ToColor4(this global::SharpDX.Vector3 vector, float w = 1f)
		{
			return new global::SharpDX.Color4((float)vector.X, (float)vector.Y, (float)vector.Z, w);
		}

		public static global::SharpDX.Color4 ToColor4(this global::SharpDX.Color3 vector, float alpha = 1f)
		{
			return new global::SharpDX.Color4(vector.Red, vector.Green, vector.Blue, alpha);
		}

		public static global::SharpDX.Color4 ToColor4(this global::SharpDX.Vector2 vector, float z = 1f, float w = 1f)
		{
			return new global::SharpDX.Color4((float)vector.X, (float)vector.Y, z, w);
		}

		public static System.Drawing.PointF ToPointF(this global::SharpDX.Vector2 vector)
		{
			return new System.Drawing.PointF(vector.X, vector.Y);
		}

		public static global::SharpDX.Matrix? ToMatrixNullable(this Transform3D transform, bool nullIfIdentity = true)
		{
			if (transform == null)
				return null;
			return transform.Value.ToMatrixNullable(nullIfIdentity);
		}

		public static global::SharpDX.Matrix? ToMatrixNullable(this Matrix3D m, bool nullIfIdentity = true)
		{
			if (m.IsIdentity && nullIfIdentity)
				return null;

			return m.ToMatrix();
		}

		public static global::SharpDX.Matrix ToMatrix(this Matrix3D m)
		{
			return new global::SharpDX.Matrix(
				(float)m.M11,
				(float)m.M12,
				(float)m.M13,
				(float)m.M14,
				(float)m.M21,
				(float)m.M22,
				(float)m.M23,
				(float)m.M24,
				(float)m.M31,
				(float)m.M32,
				(float)m.M33,
				(float)m.M34,
				(float)m.OffsetX,
				(float)m.OffsetY,
				(float)m.OffsetZ,
				(float)m.M44);
		}

		public static global::SharpDX.Vector3 Normalized(this global::SharpDX.Vector3 vector)
		{
			vector.Normalize();
			return vector;
		}

		public static global::SharpDX.Vector4 Normalized(this global::SharpDX.Vector4 vector)
		{
			vector.Normalize();
			return vector;
		}

		public static global::SharpDX.Color4 Normalized(this global::SharpDX.Color4 vector)
		{
			var v = vector.ToVector3();
			v.Normalize();
			return v.ToColor4();
		}

		public static global::SharpDX.Matrix Inverted(this global::SharpDX.Matrix m)
		{
			m.Invert();
			return m;
		}

		/// <summary>
		/// Find a <see cref="Vector3D"/> that is perpendicular to the given <see cref="Vector3D"/>.
		/// </summary>
		/// <param name="n">
		/// The input vector.
		/// </param>
		/// <returns>
		/// A perpendicular vector.
		/// </returns>
		public static global::SharpDX.Vector3 FindAnyPerpendicular(this Vector3 n)
		{
			n.Normalize();
			Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
			if (u.LengthSquared() < 1e-3)
			{
				u = Vector3.Cross(new Vector3(1, 0, 0), n);
			}

			return u;
		}

		/// <summary>
		/// Determines whether the specified vector is undefined (NaN,NaN,NaN).
		/// </summary>
		/// <param name="v">The vector.</param>
		/// <returns>
		/// <c>true</c> if the specified vector is undefined; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsUndefined(this Vector3 v)
		{
			return float.IsNaN(v.X) && float.IsNaN(v.Y) && float.IsNaN(v.Z);
		}

		public static bool IsUndefined(this Vector3D v)
		{
			return double.IsNaN(v.X) && double.IsNaN(v.Y) && double.IsNaN(v.Z);
		}

		public static Vector3D FindAnyPerpendicular(this Vector3D n)
		{
			n.Normalize();
			Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), n);
			if (u.LengthSquared < 1e-3)
			{
				u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), n);
			}

			return u;
		}

		public static bool IsUndefindedOrNull(this Vector3D v)
		{
			return v.IsUndefined() || (v.X == 0 && v.Y == 0 && v.Z == 0);
		}

		public static double DistanceTo(this Point3D p1, Point3D p2)
		{
			return (p2 - p1).Length;
		}

		public static double DistanceToSquared(this Point3D p1, Point3D p2)
		{
			return (p2 - p1).LengthSquared;
		}

		public static float DistanceTo(this Vector3 p1, Vector3 p2)
		{
			return (p2 - p1).Length();
		}

		public static float DistanceToSquared(this Vector3 p1, Vector3 p2) {
			return (p2 - p1).LengthSquared();
		}

		public static float DistanceToLine(this Vector3 point, Vector3 pointOnLine, Vector3 lineVector)
		{
			return DistanceToLine(ref point, ref pointOnLine, ref lineVector);
		}

		public static float DistanceToLine(this Vector3 point, ref Vector3 pointOnLine, ref Vector3 lineVector)
		{
			return DistanceToLine(ref point, ref pointOnLine, ref lineVector);
		}

		public static float DistanceToLine(ref Vector3 point, ref Vector3 pointOnLine, ref Vector3 lineVector)
		{
			var v = pointOnLine - point;
			var v2 = Vector3.Cross(v, lineVector);
			var distance = v2.LengthSquared() / lineVector.LengthSquared();
			return (float)Math.Sqrt(distance);
		}

		public static System.Windows.Media.Color ChangeAlpha(this System.Windows.Media.Color c, byte alpha)
		{
			return System.Windows.Media.Color.FromArgb(alpha, c.R, c.G, c.B);
		}

		public static Color ChangeAlpha(this Color c, byte alpha)
		{
			return new Color(c.R, c.G, c.B, alpha);
		}

		public static Color4 ChangeAlpha(this Color4 c, float alpha)
		{
			return new Color4(c.Red, c.Green, c.Blue, alpha);
		}

		public static Color4 ToColor4(this System.Windows.Media.Color color, float? alpha = null)
		{
			color.Clamp();
			return new global::SharpDX.Color4(color.R / 255f, color.G / 255f, color.B / 255f, alpha ?? (color.A / 255f));
		}

		public static Color ToColor(this System.Windows.Media.Color color)
		{
			color.Clamp();
			return new global::SharpDX.Color(color.R, color.G, color.B, color.A);
		}

		public static System.Windows.Media.Color? ToColor(this Color? color)
		{
			if (color == null)
				return null;
			return System.Windows.Media.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
		}

		public static System.Windows.Media.Color ToColor(this Color color)
		{
			return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static System.Windows.Media.Color ToColor(this Color4 color)
		{
			return System.Windows.Media.Color.FromArgb((byte)(color.Alpha * 255f), (byte)(color.Red * 255f), (byte)(color.Green * 255f), (byte)(color.Blue * 255f));
		}

		public static Color ToColorDX(this Color4 color)
		{
			return new Color(color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static Color4 ToColor4(this Color color, float? alpha = null)
		{
			return new Color4(color.R / 255f, color.G / 255f, color.B / 255f, alpha ?? (color.A / 255f));
		}

		public static Transform3D AppendTransform(this Transform3D t1, Transform3D t2)
		{
			var g = new System.Windows.Media.Media3D.Transform3DGroup();
			g.Children.Add(t1);
			g.Children.Add(t2);
			return g;
		}

		public static Transform3D PrependTransform(this Transform3D t1, Transform3D t2)
		{
			var g = new System.Windows.Media.Media3D.Transform3DGroup();
			g.Children.Add(t2);
			g.Children.Add(t1);
			return g;
		}

        public static Vector3 Center(this BoundingBox box) {
            var x = box.Minimum.X + (box.Maximum.X - box.Minimum.X) / 2;
            var y = box.Minimum.Y + (box.Maximum.Y - box.Minimum.Y) / 2;
            var z = box.Minimum.Z + (box.Maximum.Z - box.Minimum.Z) / 2;
            return new Vector3(x, y, z);
        }

	    public static BoundingBox ToBoundingBoxFromPoints(this IEnumerable<Vector3> points) {
	        return BoundingBox.FromPoints(points.ToArray());
	    }

        public static BoundingBox ToBoundingBoxFromPoints(this Vector3[] points) {
            return BoundingBox.FromPoints(points);
        }

		public static BoundingBox Inflate(this BoundingBox box, float dx, float dy, float dz)
		{
			return new BoundingBox(box.Minimum.Move(-dx, -dy, -dz), box.Maximum.Move(dx, dy, dz));
        }

		//public static IEnumerable<Vector3> ApplyTransform(this IEnumerable<Vector3> points, Matrix matrix)
		//{
		//	return points.Select(i =>
		//	{
		//		Vector4 result;
		//		Vector3.Transform(ref i, ref matrix, out result);
		//		return result.ToVector3();
		//	});
		//}

		//public static IEnumerable<Point3D> ApplyTransformToPoint3D(this IEnumerable<Vector3> points, Matrix matrix)
		//{
		//	return points.Select(i =>
		//	{
		//		Vector4 result;
		//		Vector3.Transform(ref i, ref matrix, out result);
		//		return new Point3D(result.X / result.W, result.Y / result.W, result.Z / result.W);
		//	});
		//}
	}

	public static class VectorComparisonExtensions
	{
		/// <summary>
		/// Returns whether ALL elements of this are SmallerOrEqual the corresponding element of v.
		/// ATTENTION: For example (a.AllSmaller(b)) is not the same as !(a.AllGreaterOrEqual(b)) but !(a.AnyGreaterOrEqual(b)).
		/// </summary>
		public static bool AllSmallerOrEqual(this Vector2 v1, Vector2 v2)
		{
			return (v1.X <= v2.X && v1.Y <= v2.Y);
		}

		/// <summary>
		/// Returns whether ALL elements of this are SmallerOrEqual the corresponding element of v.
		/// ATTENTION: For example (a.AllSmaller(b)) is not the same as !(a.AllGreaterOrEqual(b)) but !(a.AnyGreaterOrEqual(b)).
		/// </summary>
		public static bool AllSmallerOrEqual(this Vector3 v1, Vector3 v2)
		{
			return (v1.X <= v2.X && v1.Y <= v2.Y && v1.Z <= v2.Z);
		}

		/// <summary>
		/// Returns whether ALL elements of v are SmallerOrEqual s.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AllSmallerOrEqual(this Vector3 v, float s)
		{
			return (v.X <= s && v.Y <= s && v.Z <= s);
		}

		/// <summary>
		/// Returns whether ALL elements of this are SmallerOrEqual the corresponding element of v.
		/// ATTENTION: For example (a.AllSmaller(b)) is not the same as !(a.AllGreaterOrEqual(b)) but !(a.AnyGreaterOrEqual(b)).
		/// </summary>
		public static bool AllSmaller(this Vector2 v1, Vector2 v2)
		{
			return (v1.X < v2.X && v1.Y < v2.Y);
		}

		/// <summary>
		/// Returns whether ALL elements of this are SmallerOrEqual the corresponding element of v.
		/// ATTENTION: For example (a.AllSmaller(b)) is not the same as !(a.AllGreaterOrEqual(b)) but !(a.AnyGreaterOrEqual(b)).
		/// </summary>
		public static bool AllSmaller(this Vector3 v1, Vector3 v2)
		{
			return (v1.X < v2.X && v1.Y < v2.Y && v1.Z < v2.Z);
		}

		/// <summary>
		/// Returns whether ALL elements of v are SmallerOrEqual s.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AllSmaller(this Vector3 v, float s)
		{
			return (v.X < s && v.Y < s && v.Z < s);
		}

		/// <summary>
		/// Returns whether AT LEAST ONE element of a is SmallerOrEqual the corresponding element of b.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AnySmallerOrEqual(this Vector3 a, Vector3 b)
		{
			return (a.X <= b.X || a.Y <= b.Y || a.Z <= b.Z);
		}

		/// <summary>
		/// Returns whether AT LEAST ONE element of v is SmallerOrEqual s.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AnySmallerOrEqual(this Vector3 v, float s)
		{
			return (v.X <= s || v.Y <= s || v.Z <= s);
		}

		/// <summary>
		/// Returns whether ALL elements of a are GreaterOrEqual the corresponding element of b.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AllGreaterOrEqual(this Vector3 a, Vector3 b)
		{
			return (a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z);
		}

		/// <summary>
		/// Returns whether ALL elements of v are GreaterOrEqual s.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AllGreaterOrEqual(this Vector3 v, float s)
		{
			return (v.X >= s && v.Y >= s && v.Z >= s);
		}

		/// <summary>
		/// Returns whether AT LEAST ONE element of a is GreaterOrEqual the corresponding element of b.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AnyGreaterOrEqual(this Vector3 a, Vector3 b)
		{
			return (a.X >= b.X || a.Y >= b.Y || a.Z >= b.Z);
		}

		/// <summary>
		/// Returns whether AT LEAST ONE element of v is GreaterOrEqual s.
		/// ATTENTION: For example (AllSmaller(a,b)) is not the same as !(AllGreaterOrEqual(a,b)) but !(AnyGreaterOrEqual(a,b)).
		/// </summary>
		public static bool AnyGreaterOrEqual(this Vector3 v, float s)
		{
			return (v.X >= s || v.Y >= s || v.Z >= s);
		}

		/// <summary>
		/// Component-wise min vec
		/// </summary>
		public static Vector3 ComponentMin(this Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
		}

		/// <summary>
		/// Component-wise max vec
		/// </summary>
		public static Vector3 ComponentMax(this Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
		}

        /// <summary>
        /// get closest point on line
        /// </summary>
        /// <param name="point"></param>
        /// <param name="linePoint1"> first point on line</param>
        /// <param name="linePoint2"> second point on line</param>
        /// <returns></returns>
        public static Vector3 GetClosestPointOnLine(this Vector3 point,  Vector3 linePoint1, Vector3 linePoint2) {
            var x1 = linePoint1.X;
            var y1 = linePoint1.Y;
            var z1 = linePoint1.Z;
            var x2 = linePoint2.X;
            var y2 = linePoint2.Y;
            var z2 = linePoint2.Z;
            var x3 = point.X;
            var y3 = point.Y;
            var z3 = point.Z;
            var alpha = ((x3 - x1) * (x2 - x1) + (y3 - y1) * (y2 - y1) + (z3 - z1) * (z2 - z1)) /
                        ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
            var closestPoint = new Vector3(x1 + alpha * (x2 - x1), y1 + alpha * (y2 - y1), z1 + alpha * (z2 - z1));
            return closestPoint;
        }
	}
}