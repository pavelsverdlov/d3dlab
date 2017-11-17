using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using SysColor = System.Windows.Media.Color;
using DrwBitmap = System.Drawing.Bitmap;
using System.IO;

namespace HelixToolkit.Wpf.SharpDX
{
	public static class DebugFlags
	{
		public class Flags<TKey, TValue>
		{
			readonly Dictionary<TKey, TValue> keys = new Dictionary<TKey, TValue>();
			public TValue this[TKey key]
			{
				get
				{
					lock (keys)
					{
						TValue value;
						if (keys.TryGetValue(key, out value))
							return value;
						return default(TValue);
					}
				}
				set
				{
					lock (keys)
						keys[key] = value;
				}
			}

			public void RemoveAll(Func<TKey, bool> predicate)
			{
				lock (keys)
				{
					foreach (TKey key in keys.Keys.Where(predicate).ToArray())
						keys.Remove(key);
				}
			}
		}

		public static readonly Flags<object, int> FlagsInt = new Flags<object, int>();
		public static readonly Flags<object, bool> FlagsBool = new Flags<object, bool>();
		public static readonly Flags<object, object> FlagsObj = new Flags<object, object>();
	}

	public static class SDXCommonExtensions
	{
		private static PropertyInfo wpfCursorHandleProp;
		public static System.Windows.Forms.Cursor ToWFCursor(this System.Windows.Input.Cursor cursor)
		{
			if (wpfCursorHandleProp == null)
				wpfCursorHandleProp = typeof(System.Windows.Input.Cursor).GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic);

			var handle = (System.Runtime.InteropServices.SafeHandle)wpfCursorHandleProp.GetValue(cursor);
			if (handle != null && !handle.IsInvalid)
				return new System.Windows.Forms.Cursor(handle.DangerousGetHandle());

			return System.Windows.Forms.Cursors.Cross;
		}

		public static System.Windows.Input.Cursor ToWPFCursor(this System.Windows.Forms.Cursor cursor)
		{
			var wpfCursor = typeof(System.Windows.Input.Cursors).GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
				.Where(i => i.PropertyType == typeof(System.Windows.Input.Cursor))
				.Select(i => (System.Windows.Input.Cursor)i.GetValue(null))
				.Where(i => i.ToWFCursor() == cursor)
				.FirstOrDefault();

			if (wpfCursor != null)
				return wpfCursor;

			return null;
		}

		public static double ToRad(this double degrees)
		{
			return degrees * Math.PI / 180.0;
		}

		public static float ToRadF(this double degrees)
		{
			return (float)(degrees * Math.PI / 180.0);
		}

		public static float ToRad(this float degrees)
		{
			return degrees * MathUtil.Pi / 180f;
		}

		public static float ToDeg(this float radians)
		{
			return radians * 180f / MathUtil.Pi;
		}

		public static double ToDeg(this double radians)
		{
			return radians * 180.0 / Math.PI;
		}

		public static bool EqualsStrong<T>(this IList<T> array1, IList<T> array2)
		{
			if (array1 == null && array2 == null)
				return true;
			if (array1 != null && array2 == null)
				return false;
			if (array1 == null && array2 != null)
				return false;
			if (array1.Count != array2.Count)
				return false;

			for (int i = 0; i < array1.Count; i++)
			{
				if (!object.Equals(array1[i], array2[i]))
					return false;
			}
			return true;
		}

		static readonly Element3D[] emptyElements = new Element3D[0];

		public static IEnumerable<Element3D> Children(this Element3D elem)
		{
			var group = elem as GroupElement3D;
			if (group != null)
				return group.Children != null ? group.Children.Cast<Element3D>() : emptyElements;

			var compModel = elem as CompositeModel3D;
			if (compModel != null)
				return compModel.Children != null ? compModel.Children.Cast<Element3D>() : emptyElements;

			return emptyElements;
		}

		public static bool IsNaN(ref System.Windows.Media.Media3D.Matrix3D m)
		{
			return double.IsNaN(m.M11)
				|| double.IsNaN(m.M12)
				|| double.IsNaN(m.M13)
				|| double.IsNaN(m.M14)
				|| double.IsNaN(m.M21)
				|| double.IsNaN(m.M22)
				|| double.IsNaN(m.M23)
				|| double.IsNaN(m.M24)
				|| double.IsNaN(m.M31)
				|| double.IsNaN(m.M32)
				|| double.IsNaN(m.M33)
				|| double.IsNaN(m.M34)
				|| double.IsNaN(m.OffsetX)
				|| double.IsNaN(m.OffsetY)
				|| double.IsNaN(m.OffsetZ)
				|| double.IsNaN(m.M44);
		}

		public static bool IsNaN(ref Matrix m)
		{
			return float.IsNaN(m.M11)
				|| float.IsNaN(m.M12)
				|| float.IsNaN(m.M13)
				|| float.IsNaN(m.M14)
				|| float.IsNaN(m.M21)
				|| float.IsNaN(m.M22)
				|| float.IsNaN(m.M23)
				|| float.IsNaN(m.M24)
				|| float.IsNaN(m.M31)
				|| float.IsNaN(m.M32)
				|| float.IsNaN(m.M33)
				|| float.IsNaN(m.M34)
				|| float.IsNaN(m.M41)
				|| float.IsNaN(m.M42)
				|| float.IsNaN(m.M43)
				|| float.IsNaN(m.M44);
		}

		public static bool IsNaN(ref Vector3 v)
		{
			return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
		}

		public static bool IsNaN(ref Color4 v)
		{
			return float.IsNaN(v.Red) || float.IsNaN(v.Green) || float.IsNaN(v.Blue) || float.IsNaN(v.Alpha);
		}

		public static void CheckData(bool condition)
		{
			if (condition)
				Debug.WriteLine("Data isn't valid");
		}

		public static void CheckIsNaN(this System.Windows.Media.Media3D.Transform3D t)
		{
			var m = t.Value;
			CheckData(IsNaN(ref m));
		}

		public static void CheckIsNaN(ref System.Windows.Media.Media3D.Matrix3D m)
		{
			CheckData(IsNaN(ref m));
		}

		public static void CheckIsNaN(ref Matrix m)
		{
			CheckData(IsNaN(ref m));
		}

		public static void CheckIsNaN(ref Vector3 v)
		{
			CheckData(IsNaN(ref v));
		}

		public static void CheckIsNaN(ref Color4 v)
		{
			CheckData(IsNaN(ref v));
		}

		public static Color4 CheckIsNaN(this Color4 v)
		{
			CheckData(IsNaN(ref v));
			return v;
		}

		public static float CheckIsNaN(this float v)
		{
			CheckData(float.IsNaN(v));
			return v;
		}

		public static PhongMaterial ToMaterial(this Color4 color)
		{
			return new PhongMaterial()
			{
				DiffuseColor = color,
				SpecularColor = (Color4)Color.White,
				SpecularShininess = 300,
			};
		}

		public static Vector3 TransformToV3(this Vector3 vector, Matrix transform)
		{
			var res = default(Vector3);
			TransformToV3(ref vector, ref transform, ref res);
			return res;
		}

		public static Vector3 TransformToV3(this Vector3 vector, ref Matrix transform)
		{
			var res = default(Vector3);
			TransformToV3(ref vector, ref transform, ref res);
			return res;
		}

		public static void TransformToV3(ref Vector3 vector, ref Matrix transform, ref Vector3 result)
		{
			var w = vector.X * transform.M14 + vector.Y * transform.M24 + vector.Z * transform.M34 + transform.M44;

			result.X = (vector.X * transform.M11 + vector.Y * transform.M21 + vector.Z * transform.M31 + transform.M41) / w;
			result.Y = (vector.X * transform.M12 + vector.Y * transform.M22 + vector.Z * transform.M32 + transform.M42) / w;
			result.Z = (vector.X * transform.M13 + vector.Y * transform.M23 + vector.Z * transform.M33 + transform.M43) / w;
		}

		public static void Transform(ref Vector2 vector, ref Matrix transform, out Vector2 result)
		{
			var w = vector.X * transform.M14 + vector.Y * transform.M24 + transform.M44;

			result = new Vector2(
				(vector.X * transform.M11 + vector.Y * transform.M21 + transform.M41) / w,
				(vector.X * transform.M12 + vector.Y * transform.M22 + transform.M42) / w);
		}

		public static void Transform(Vector2[] vectors, ref Matrix transform, Vector2[] results)
		{
			var len = vectors.Length;
			for (int i = 0; i < len; i++)
			{
				var vector = vectors[i];
				var w = vector.X * transform.M14 + vector.Y * transform.M24 + transform.M44;
				results[i] = new Vector2(
					(vector.X * transform.M11 + vector.Y * transform.M21 + transform.M41) / w,
					(vector.X * transform.M12 + vector.Y * transform.M22 + transform.M42) / w);
			}
		}

		public static bool IsNaN(this double d)
		{
			return double.IsNaN(d);
		}

		public static bool IsNaN(this float d)
		{
			return float.IsNaN(d);
		}

	    public static bool IsNaN(this Plane p) {
	        var normal = p.Normal;
            return float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z) || float.IsNaN(p.D);
	    }

		public static bool IsNaN(this Vector3 v)
		{
			return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
		}

		public static float AngleBetween(this Vector3 vector1, Vector3 vector2)
		{
			vector1.Normalize();
			vector2.Normalize();
			float num;
			Vector3.Dot(ref vector1, ref vector2, out num);
			double radians;
			if (num < 0.0)
				radians = Math.PI - 2.0 * Math.Asin((-vector1 - vector2).Length() / 2.0);
			else
				radians = 2.0 * Math.Asin((vector1 - vector2).Length() / 2.0);
			return MathUtil.RadiansToDegrees((float)radians);
		}

		public static float AngleBetweenLine(this Vector3 vector1, Vector3 vector2)
		{
			vector1.Normalize();
			vector2.Normalize();
			float num;
			Vector3.Dot(ref vector1, ref vector2, out num);
			double radians;
			if (num < 0.0)
				radians = 2.0 * Math.Asin((-vector1 - vector2).Length() / 2.0);
			else
				radians = 2.0 * Math.Asin((vector1 - vector2).Length() / 2.0);
			return MathUtil.RadiansToDegrees((float)radians);
		}

        public static Matrix RotateAroundAxis(this Vector3 axis, float angleInDeg, Vector3 center) {
            return axis.RotationAround(MathUtil.DegreesToRadians(angleInDeg), center);
        }

        public static Matrix RotateAroundAxis(this Vector3 axis, float angleInDeg) {
            return axis.RotationAround(MathUtil.DegreesToRadians(angleInDeg));
        }

        public static Matrix RotationAround(this Vector3 axis, float angle, Vector3 center) {
            var m1 = Matrix.Translation(center * -1f);
            var m2 = axis.RotationAround(angle);
            var m3 = Matrix.Translation(center);
            var m = m1 * m2 * m3;
            return m;
		}

        public static Matrix RotationAround(this Vector3 axis, float angle) {
            return Matrix.RotationAxis(axis, angle);
        }

        // http://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
        public static Matrix RotationTo(this Vector3 source, Vector3 target) {
            Quaternion quaternion;
            if (source == target) {
                // No rotation
                quaternion = new Quaternion(Vector3.Zero, 1);
            } else if (source == -target) {
                // 180 degree rotation around any orthogonal vector
                quaternion = new Quaternion(Vector3.UnitZ, 0);
            } else {
                var cross = Vector3.Cross(source, target);
                var dot = Vector3.Dot(source, target);
                var area = (float)Math.Sqrt(source.LengthSquared() * target.LengthSquared());
                quaternion = new Quaternion(cross, dot + area);
                quaternion.Normalize();
            }
            return Matrix.RotationQuaternion(quaternion);
		}

        public static Matrix RotationTo(this Vector3 source, Vector3 target, Vector3 center) {
            var m1 = Matrix.Translation(center * -1f);
            var m2 = RotationTo(source, target);
            var m3 = Matrix.Translation(center);
            var m = m1 * m2 * m3;
            return m;
        }

        public static Matrix ScaleRelativeToCenter(this Vector3 center, float scale)
		{
			return ScaleRelativeToCenter(center, new Vector3(scale));
		}

		public static Matrix ScaleRelativeToCenter(this Vector3 center, Vector3 scale)
		{
			var m1 = Matrix.Translation(center * -1f);
			var m2 = Matrix.Scaling(scale);
			var m3 = Matrix.Translation(center);
			var m = m1 * m2 * m3;
			return m;
		}

		public static Vector3 Middle(this Vector3 p1, Vector3 p2, float k = 0.5f)
		{
			return new Vector3(
					p1.X * (1f - k) + p2.X * k,
					p1.Y * (1f - k) + p2.Y * k,
					p1.Z * (1f - k) + p2.Z * k);
		}

		public static global::SharpDX.Vector3 GetCenter(this global::SharpDX.BoundingBox bounds)
		{
			var centerX = (bounds.Minimum.X + bounds.Maximum.X) / 2;
			var centerY = (bounds.Minimum.Y + bounds.Maximum.Y) / 2;
			var centerZ = (bounds.Minimum.Z + bounds.Maximum.Z) / 2;
			return new global::SharpDX.Vector3(centerX, centerY, centerZ);
		}

		public static MeshGeometry3D Transform(this MeshGeometry3D mesh, Matrix matrix, bool clone = true)
		{
			if (mesh == null)
				return null;
			if (clone)
				mesh = mesh.Clone();

			if (mesh.Positions != null)
				mesh.Positions = mesh.Positions.Transform(matrix);
			if (mesh.Normals != null)
				mesh.Normals = mesh.Normals.TransformNormals(matrix);

			return mesh;
		}

		public static Vector3Collection Transform(this Vector3Collection positions, Matrix matrix)
		{
			var newPositions = positions.ToArrayFast().Transform(matrix);
			return Vector3Collection.FromArray<Vector3Collection>(newPositions);
		}

		public static Vector3Collection TransformNormals(this Vector3Collection normals, Matrix matrix)
		{
			var newNormals = normals.ToArrayFast().Transform(matrix);
			return Vector3Collection.FromArray<Vector3Collection>(newNormals);
		}

		public static Vector3[] Transform(this Vector3[] positions, Matrix matrix)
		{
			return Transform(positions, ref matrix);
		}

		public static Vector3[] TransformNormals(this Vector3[] normals, Matrix matrix)
		{
			return TransformNormals(normals, ref matrix);
		}

		unsafe public static Vector3[] Transform(this Vector3[] positions, ref Matrix matrix)
		{
			if (positions == null || positions.Length == 0)
				return null;

			var result = new Vector3[positions.Length];
			fixed (Vector3* _pSrc = positions)
			{
				fixed (Vector3* _pDst = result)
				{
					Vector3* pSrc = _pSrc, pDst = _pDst;
					var end = pSrc + positions.Length;
					for (; pSrc < end; ++pSrc, ++pDst)
						Vector3.TransformCoordinate(ref *pSrc, ref matrix, out *pDst);
				}
			}
			return result;
		}

		unsafe public static Vector3[] TransformNormals(this Vector3[] normals, ref Matrix matrix)
		{
			if (normals == null || normals.Length == 0)
				return null;

			var result = new Vector3[normals.Length];
			fixed (Vector3* _pSrc = normals)
			{
				fixed (Vector3* _pDst = result)
				{
					Vector3* pSrc = _pSrc, pDst = _pDst;
					var end = pSrc + normals.Length;
					for (; pSrc < end; ++pSrc, ++pDst)
						Vector3.TransformNormal(ref *pSrc, ref matrix, out *pDst);
				}
			}
			return result;
		}

		unsafe public static Vector2[] ToVector2(this Vector3[] positions)
		{
			if (positions == null || positions.Length == 0)
				return null;

			var result = new Vector2[positions.Length];
			fixed (Vector3* _pSrc = positions)
			{
				fixed (Vector2* _pDst = result)
				{
					Vector3* pSrc = _pSrc;
					Vector2* pDst = _pDst;
					var end = pSrc + positions.Length;
					for (; pSrc < end; ++pSrc, ++pDst)
						*((long*)pDst) = *((long*)pSrc);
				}
			}
			return result;
		}

		unsafe public static System.Drawing.PointF[] ToPointF(this Vector2[] positions)
		{
			if (positions == null || positions.Length == 0)
				return null;

			var result = new System.Drawing.PointF[positions.Length];
			fixed (Vector2* _pSrc = positions)
			{
				fixed (System.Drawing.PointF* _pDst = result)
				{
					Vector2* pSrc = _pSrc;
					System.Drawing.PointF* pDst = _pDst;
					var end = pSrc + positions.Length;
					for (; pSrc < end; ++pSrc, ++pDst)
					{
						pDst->X = pSrc->X;
						pDst->Y = pSrc->Y;
					}
				}
			}
			return result;
		}

		public static bool IsGeometryModelValid(this GeometryModel3D model)
		{
			if (model == null)
				return false;
			return model.ThreadSafe_Geometry.IsGeometryValid();
		}

		public static bool IsGeometryValid(this Geometry3D g)
		{
			if (g == null)
				return false;
			if (g.Positions == null)
				return false;
			if (g.Positions.Count == 0)
				return false;
			if (g.Indices == null)
				return false;
			if (g.Indices.Count == 0)
				return false;

			return true;
		}

		public static bool TryLock(object lockObject, Action handler, int timeout = 50)
		{
			if (lockObject == null)
				throw new ArgumentNullException("lockObject", "lockObject is null.");
			if (handler == null)
				throw new ArgumentNullException("handler", "handler is null.");

			if (!Monitor.TryEnter(lockObject, timeout))
				return false;
			try
			{
				handler();
			}
			finally
			{
				Monitor.Exit(lockObject);
			}
			return true;
		}

		public static bool IsEmpty(this BoundingBox b, float deviation = 0.001f)
		{
			return b.Minimum.Length() < deviation && b.Maximum.Length() < deviation;
		}

		public static BoundingBox Combine(this IEnumerable<BoundingBox> bounds)
		{
			var bArray = bounds.ToList();
			if (bArray.Count == 0)
				return default(BoundingBox );

			if (bArray.Count == 1)
				return bArray[0];

			var r = bArray[0];
			for (int i = 1; i < bArray.Count; i++)
				r = BoundingBox.Merge(r, bArray[i]);
			return r;
		}

		public static Vector3 Move(this Vector3 p1, Vector3 offset)
		{
			return p1 + offset;
		}

		public static Vector3 Move(this Vector3 p1, float dx, float dy, float dz)
		{
			return p1 + new Vector3(dx, dy, dz);
		}

		public static global::SharpDX.BoundingSphere ToBoundingSphere(this BoundingBox bounds, Matrix? matrix)
		{
			var center = bounds.Center();
			if (matrix != null)
				center = center.TransformToV3(matrix.Value);
			return new global::SharpDX.BoundingSphere(center, bounds.Minimum.DistanceTo(bounds.Maximum));
		}

		public static Matrix GlobalTransform(this Model3D model, bool onlyParent = false)
		{
			if (onlyParent && model != null)
				model = model.Parent as Model3D;

			var result = Matrix.Identity;
			while (model != null)
			{
				if (model.LocalMatrix != Matrix.Zero)
					result = model.LocalMatrix * result;
				model = model.Parent as Model3D;
			}
			return result;
		}

		#region Material

		public static PhongMaterial ToPhongMaterial(this global::SharpDX.Color color, float? opacity = null)
		{
			return ToPhongMaterial(color.ToColor4(), opacity);
		}

		public static PhongMaterial ToPhongMaterial(this System.Windows.Media.Color color, float? opacity = null)
		{
			return ToPhongMaterial(color.ToColor4(), opacity);
		}

		public static PhongMaterial ToPhongMaterial(this Color4 color, float? opacity = null)
		{
			if (opacity != null)
				color.Alpha = opacity.Value;
			return new PhongMaterial
			{
				AmbientColor = new Color4(),
				DiffuseColor = color,
				SpecularColor = color,
				EmissiveColor = new Color4(),
				ReflectiveColor = new Color4(),
				SpecularShininess = 100f
			};
		}

		public static PhongMaterial ToPhongMaterial(this BitmapSource image, float specularShininess = 1000000000000f)
		{
			var color = image.GetAverageColorFromImage();
			return new PhongMaterial
			{
				DiffuseMap = image,
				DiffuseColor = Color.White.ToColor4(),
				SpecularColor = color.ToColor4(),
				EmissiveColor = new Color4(),
				ReflectiveColor = new Color4(),
				AmbientColor = new Color4(),
				SpecularShininess = specularShininess,
			};
		}

		public static SysColor GetAverageColorFromImage(this BitmapSource image)
		{
			if (image == null)
			{
				return System.Windows.Media.Colors.White;
			}

			var bitmap = BitmapImage2Bitmap(image);

			var pixel = bitmap.GetPixel(bitmap.Width / 3 * 2, bitmap.Height / 3 * 2);
			return SysColor.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
		}

		public static DrwBitmap BitmapImage2Bitmap(BitmapSource bitmapImage)
		{
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapImage));
				enc.Save(outStream);
				DrwBitmap bitmap = new DrwBitmap(outStream);

				return new DrwBitmap(bitmap);
			}
		}

		#endregion
	}

	//public class DispatcherHelper
	//{
	//	private static DispatcherOperationCallback exitFrameCallback = new
	//		 DispatcherOperationCallback(ExitFrame);

	//	/// <summary>
	//	/// Processes all UI messages currently in the message queue.
	//	/// </summary>
	//	public static void DoEvents()
	//	{
	//		// Create new nested message pump.
	//		var nestedFrame = new DispatcherFrame();

	//		// Dispatch a callback to the current message queue, when getting called,
	//		// this callback will end the nested message loop.
	//		// note that the priority of this callback should be lower than that of UI event messages.
	//		var exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(
	//						DispatcherPriority.Background, exitFrameCallback, nestedFrame);

	//		// pump the nested message loop, the nested message loop will immediately
	//		// process the messages left inside the message queue.
	//		Dispatcher.PushFrame(nestedFrame);

	//		// If the "exitFrame" callback is not finished, abort it.
	//		if (exitOperation.Status != DispatcherOperationStatus.Completed)
	//			exitOperation.Abort();
	//	}

	//	private static object ExitFrame(object state)
	//	{
	//		DispatcherFrame frame = state as DispatcherFrame;

	//		// Exit the nested message loop.
	//		frame.Continue = false;
	//		return null;
	//	}
	//}
}
