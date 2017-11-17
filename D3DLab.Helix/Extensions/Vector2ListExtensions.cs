using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Extensions {
	public static class Vector2ListExtensions {
		public static Vector2 GetItemSafe(this List<Vector2> points, int index) {
			var count = points.Count;
			if (count == 0)
				return Vector2Extensions.NaN;
			while (index < 0)
				index += count;
			while (index >= count)
				index -= count;
			return points[index];
		}

		public static List<Vector2> Reversed(this List<Vector2> points) {
			var result = points.ToList();
			result.Reverse();
			//var result = new List<Vector2>(points.Count);
			//for (var i = points.Count - 1; i >= 0; --i)
			//	result.Add(points[i]);
			return result;
		}

		public static List<Vector2> Subsequence(this List<Vector2> points, int begin, int end) {
			var result = new List<Vector2>();
			if (end >= begin) {
				for (var i = begin; i <= end; ++i)
					result.Add(points[i]);
			} else {
				var count = points.Count;
				for (var i = begin; i < count; ++i)
					result.Add(points[i]);
				for (var i = 0; i <= end; ++i)
					result.Add(points[i]);
			}
			return result;
		}

		public static List<Vector2> ReversedSubsequence(this List<Vector2> points, int begin, int end) {
			var result = new List<Vector2>();
			if (end <= begin) {
				for (var i = begin; i >= end; --i)
					result.Add(points[i]);
			} else {
				for (var i = begin; i >= 0; --i)
					result.Add(points[i]);
				for (var i = points.Count - 1; i >= end; --i)
					result.Add(points[i]);
			}
			return result;
		}

		public static float FindClosestPointDistance(this List<Vector2> points, Vector2 p0) {
			if ((points == null) || (points.Count == 0))
				return float.NaN;
			return p0.DistanceTo(points[points.FindClosestPointIndex(p0)]);
		}

		public static int FindClosestPointIndex(this List<Vector2> points, Vector2 p0) {
			if ((points == null) || (points.Count == 0))
				return -1;

			var result = 0;
			var distance2 = p0.DistanceSq(points[0]);

			var count = points.Count;
			for (var i = 1; i < count; i++) {
				var d2 = p0.DistanceSq(points[i]);
				if (d2 >= distance2)
					continue;
				distance2 = d2;
				result = i;
			}

			return result;
		}

		public static bool Intersects(this List<Vector2> points, int begin, int end, Vector2 p0, Vector2 p1) {
			if (begin < 0)
				begin = 0;
			if (end >= points.Count)
				end = points.Count - 1;
			for (var i = begin; i < end; ++i) {
				if (!p0.Intersection(p1, points[i], points[i + 1]).IsNaN())
					return true;
			}
			return false;
		}

		public static bool Intersects(this List<Vector2> points, Vector2 p0, Vector2 p1) {
			return Intersects(points, 0, points.Count - 1, p0, p1);
		}

		/// <summary>
		/// Search for a closest point index i such as p[i] is left from a ray but p[i+1] is right from it
		/// </summary>
		/// <param name="points">Ring or linestring points</param>
		/// <param name="source">Ray begin point</param>
		/// <param name="target">Ray end point</param>
		/// <param name="inside">Hit from inside the ring (invert angle sign)</param>
		/// <param name="closed">Linestring-is-a-ring flag</param>
		/// <returns></returns>
		public static int FindRayHit(this List<Vector2> points, Vector2 source, Vector2 target, bool inside = false, bool closed = false) {
			var count = points.Count;
			var closestHitIndex = -1;
			if (count > 1) {
				var sign = inside ? -1 : 1;
				var closestHitDistanceSq = float.NaN;
				var angle = source.AngleBetween(target, points[0]) * sign;
				for (var i = 1; i < count; ++i) {
					var nextAngle = source.AngleBetween(target, points[i]) * sign;
					if ((angle >= 0) && (nextAngle < 0)) {
						var distanceSq = source.DistanceSq(points[i]);
						if ((closestHitIndex == -1) || (distanceSq < closestHitDistanceSq)) {
							closestHitDistanceSq = distanceSq;
							closestHitIndex = i - 1;
						}
					}
					angle = nextAngle;
				}
				if (closed) {
					var nextAngle = source.AngleBetween(target, points[0]) * sign;
					if ((angle >= 0) && (nextAngle < 0)) {
						var distanceSq = source.DistanceSq(points[0]);
						if ((closestHitIndex == -1) || (distanceSq < closestHitDistanceSq)) {
							closestHitIndex = count - 1;
						}
					}
				}
			}
			return closestHitIndex;
		}

		public static Vector2 GetClosestPoint(this List<Vector2> points, Vector2 inputPoint) {
			var result = points[0];
			var distance = inputPoint.DistanceTo(result);

			var count = points.Count;
			for (var i = 1; i < count; i++) {
				var closest = inputPoint.GetClosestPointOfSegment(points[i - 1], points[i]);
				if (closest.Item1 >= distance)
					continue;
				distance = closest.Item1;
				result = closest.Item2;
			}

			return result;
		}

		public static List<Vector2> GetClosestPoints(this List<Vector2> points, List<Vector2> inputPoints) {
			var count = inputPoints.Count;
			var result = new List<Vector2>(count);

			for (var i = 0; i < count; i++)
				result.Add(GetClosestPoint(points, inputPoints[i]));

			return result;
		}

		public static int GetPointIndex(this List<Vector2> points, Vector2 point) {
			var count = points.Count;
			for (var i = 0; i < count; i++) {
				if (points[i] == point)
					return i;
			}
			return -1;
		}

		public static bool Contains(this List<Vector2> points, Vector2 point) {
			return points.GetPointIndex(point) >= 0;
		}

		public class ClosestPointData {
			public int Index;
			public Vector2 Closest;
			public float Distance;
		}

		public static ClosestPointData FindClosestPointData(this List<Vector2> points, Vector2 inputPoint) {
			var result = new ClosestPointData() {
				Index = 0,
				Closest = points[0],
				Distance = inputPoint.DistanceTo(points[0])
			};

			var count = points.Count;
			for (var i = 1; i < count; i++) {
				var distance = inputPoint.DistanceTo(points[i]);
				if (distance >= result.Distance)
					continue;
				result.Index = i;
				result.Closest = points[i];
				result.Distance = distance;
			}

			return result;
		}

		private static bool GetNextIndex(ref int index, int count, bool closed) {
			var result = true;
			if (index < count - 1)
				index++;
			else if (closed)
				index = 0;
			else
				result = false;
			return result;
		}

		public static bool GetNextIndex(this List<Vector2> points, ref int index, bool closed) {
			return GetNextIndex(ref index, points.Count, closed);
		}

		public static int GetNextIndex(this List<Vector2> points, int index) {
			int result = index;
			points.GetNextIndex(ref result, true);
			return result;
		}

		private static bool GetPrevIndex(ref int index, int count, bool closed) {
			var result = true;
			if (index > 0)
				index--;
			else if (closed)
				index = count - 1;
			else
				result = false;
			return result;
		}

		public static int GetPrevIndex(this List<Vector2> points, int index) {
			int result = index;
			points.GetPrevIndex(ref result, true);
			return result;
		}

		public static bool GetPrevIndex(this List<Vector2> points, ref int index, bool closed) {
			return GetPrevIndex(ref index, points.Count, closed);
		}

		/// <summary>
		/// Add extra points to the list to make the linestring more smooth
		/// </summary>
		/// <param name="points">Source point list</param>
		/// <param name="begin">Start point index</param>
		/// <param name="end">Stop point index</param>
		/// <param name="minSegment">Minimum segment length</param>
		/// <param name="closed">Linestring-is-a-ring flag</param>
		/// <returns></returns>
		public static List<Vector2> Decorate(this List<Vector2> points, int begin, int end, float minSegment, bool closed) {
			if (points == null)
				return null;
			var result = points.ToList();
			var count = result.Count;
			if (count > 1) {
				var minSegment2 = minSegment * minSegment;
				// Correct begin value
				if (begin < 0)
					begin = 0;
				else if (begin >= count)
					begin = count - 1;
				// Correct end value
				if (end < 0)
					end = 0;
				else if (end >= count)
					end = count - 1;
				// Split the longest segment while it is longer then minSegment
				if (closed && (begin != end) || !closed && (begin < end)) {
					while (true) {
						// Search for a longest segment
						int i0 = begin, i1 = begin;
						if (!GetNextIndex(ref i1, count, closed))
							break;
						var maxFound = false;
						var maxDistance2 = 0f;//result[i0].DistanceSq(result[i1]);
						int j0 = i0, j1 = i0;
						while (j0 != end) {
							if (!GetNextIndex(ref j1, count, closed))
								break;
							var distance2 = result[j0].DistanceSq(result[j1]);
							if (!maxFound || (distance2 > maxDistance2)) {
								int k0 = j0, k1 = j1;
								var l0 = GetPrevIndex(ref k0, count, closed);
								var l1 = GetNextIndex(ref k1, count, closed);
								if (!l0 || (Math.Abs(result[j0].AngleBetween(result[j1], result[k0])) > 120)) {
									if (!l1 || (Math.Abs(result[j1].AngleBetween(result[j0], result[k1])) > 120)) {
										i0 = j0;
										i1 = j1;
										maxDistance2 = distance2;
										maxFound = true;
									}
								}
							}
							j0 = j1;
						}
						if (maxDistance2 <= minSegment2) // No long enough segments found
							break;
						j0 = (i0 > 0) ? i0 - 1 : closed ? count - 1 : i0;
						j1 = (i1 < count - 1) ? i1 + 1 : closed ? 0 : i1;
						// Get points of the splitting segment
						var p0 = result[i0];
						var p1 = result[i1];
						// Get helper points
						var q0 = new Vector2(p0.X * 2 - result[j0].X, p0.Y * 2 - result[j0].Y);
						var q1 = new Vector2(p1.X * 2 - result[j1].X, p1.Y * 2 - result[j1].Y);
						// Get middle points
						var m0 = p0.MiddlePoint(p1);
						var m1 = q0.MiddlePoint(q1);
						// Get inserting point
						var m = m0.MiddlePoint(m1);
						result.Insert(i1, m0.MiddlePoint(m));
						count++;
						if (begin >= i1)
							begin++;
						if (end >= i1)
							end++;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Add extra points to the list to make the linestring more smooth
		/// </summary>
		/// <param name="points">Source point list</param>
		/// <param name="minSegment">Minimum segment length</param>
		/// <param name="closed">Linestring-is-a-ring flag</param>
		/// <returns></returns>
		public static List<Vector2> Decorate(this List<Vector2> points, float minSegment, bool closed) {
			return Decorate(points, 0, points.Count - 1, minSegment, closed);
		}

		public static void AddPoint(this List<Vector2> points, Vector2 point) {
			points.Add(point);
		}

		public static void AddSegment(this List<Vector2> points, Vector2 segment) {
			points.Add(points.Last() + segment);
		}

		/// <summary>
		/// Draw an arc from new points beginning from the las point in the list
		/// </summary>
		/// <param name="points"></param>
		/// <param name="center"></param>
		/// <param name="angle"></param>
		/// <param name="segments"></param>
		public static void AddArc(this List<Vector2> points, Vector2 center, float angle, int segments = 0) {
			if (angle.Equals(0))
				return;
			var start = points.Last();
			var radius = start - center;
			if (segments < 1)
				segments = (int)Math.Round(Math.Abs(angle) * radius.Length() / 30);
			if (segments == 0)
				return;
			var step = Math.PI * angle / 180 / segments;
			var cos = (float)Math.Cos(step);
			var sin = (float)Math.Sin(step);
			for (var i = 1; i < segments; ++i) {
				radius = new Vector2(radius.X * cos - radius.Y * sin, radius.X * sin + radius.Y * cos);
				points.Add(center + radius);
			}
			// Last point is calculated just with angle value
			step = Math.PI * angle / 180;
			cos = (float)Math.Cos(step);
			sin = (float)Math.Sin(step);
			radius = start - center;
			radius = new Vector2(radius.X * cos - radius.Y * sin, radius.X * sin + radius.Y * cos);
			points.Add(center + radius);
		}

		/// <summary>
		/// Draw the quadrant
		/// </summary>
		/// <param name="points"></param>
		/// <param name="center"></param>
		/// <param name="left"></param>
		/// <param name="segments"></param>
		public static void AddQuadrant(this List<Vector2> points, Vector2 center, bool left, int segments = 0) {
			AddArc(points, center, left ? 90f : -90f, segments);
		}

	}
}
