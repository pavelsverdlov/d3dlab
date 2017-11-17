using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Extensions
{
	public static class Vector3Extensions
	{
		public static BoundingBox GetBounds(this IList<Vector3> points, Matrix matrix)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			Vector3 p;
			Vector3 minimum = new Vector3(3.40282347E+38f);
			Vector3 maximum = new Vector3(-3.40282347E+38f);
			for (int i = 0; i < points.Count; i++)
			{
				p = points[i];
				Transform(ref p, ref matrix, out p);
				Vector3.Min(ref minimum, ref p, out minimum);
				Vector3.Max(ref maximum, ref p, out maximum);
			}
			return new BoundingBox(minimum, maximum);
		}

		public static void Transform(ref Vector3 vector, ref Matrix transform, out Vector3 result)
		{
			var w = vector.X * transform.M14 + vector.Y * transform.M24 + vector.Z * transform.M34 + transform.M44;
			result = new Vector3(
				(vector.X * transform.M11 + vector.Y * transform.M21 + vector.Z * transform.M31 + transform.M41) / w,
				(vector.X * transform.M12 + vector.Y * transform.M22 + vector.Z * transform.M32 + transform.M42) / w,
				(vector.X * transform.M13 + vector.Y * transform.M23 + vector.Z * transform.M33 + transform.M43) / w);
		}

		public static float AngleBetween(this Vector3 vector1, Vector3 vector2)
		{
			vector1.Normalize();
			vector2.Normalize();
			float num;
			Vector3.Dot(ref vector1, ref vector2, out num);
			float radians;
			if (num < 0.0)
				radians = (float)(Math.PI - 2.0 * Math.Asin((-vector1 - vector2).Length() / 2.0));
			else
				radians = (float)(2.0 * Math.Asin((vector1 - vector2).Length() / 2.0));
			return MathUtil.RadiansToDegrees(radians);
		}

		public static float AngleBetween(this Vector2 vector1, Vector2 vector2)
		{
			double y = vector1.X * vector2.Y - vector2.Y * vector1.Y;
			double x = vector1.X * vector2.X + vector1.Y * vector2.Y;
			return (float)(Math.Atan2(y, x) * 57.295779513082323);
		}
	}
}
