using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Utilities {
	public class FlatMeshBuilder {
		public HelixToolkit.Wpf.SharpDX.MeshBuilder MeshBuilder { get; private set; }

		public FlatMeshBuilder() {
			this.MeshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(false, false);
		}

		public void Clear() {
			this.MeshBuilder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(false, false);
		}

		public HelixToolkit.Wpf.SharpDX.MeshGeometry3D ToMeshGeometry3D() {
			return MeshBuilder.ToMeshGeometry3D();
		}

		public void AddSide(List<Vector2> points, float topZ, float bottomZ, bool reverse = false) {
			var count = points.Count;
			var points3D = new Vector3[2, count];
			for (var i = 0; i < count; ++i) {
				points3D[0, i] = points[i].ToVector3(reverse ? topZ : bottomZ);
				points3D[1, i] = points[i].ToVector3(reverse ? bottomZ : topZ);
			}
			this.MeshBuilder.AddRectangularMesh(points3D);
		}

		public void AddSide(List<Vector2> points, int begin, int end, float topZ, float bottomZ, bool reverse = false) {
			this.AddSide(points.Subsequence(begin, end), topZ, bottomZ, reverse);
		}

		public void AddSide(Vector2 begin, Vector2 end, float topZ, float bottomZ, bool reverse = false) {
			this.MeshBuilder.AddRectangularMesh(new List<Vector3> {
				begin.ToVector3(reverse ? topZ : bottomZ),
				begin.ToVector3(reverse ? bottomZ : topZ),
				end.ToVector3(reverse ? topZ : bottomZ),
				end.ToVector3(reverse ? bottomZ : topZ),
			}, 2);
		}

		public void AddBase(List<Vector2> points, List<int> indices, float z, bool reverse = false) {
			if (indices == null)
				return;
			if (reverse)
				indices = FlatMeshBuilder.InvertIndices(indices);
			this.MeshBuilder.Append(points.Select(point => point.ToVector3(z)).ToList(), indices);
		}

		public void AddBases(List<Vector2> points, List<int> indices, float topZ, float bottomZ, bool reverse = false) {
			if (indices == null)
				return;
			AddBase(points, indices, reverse ? bottomZ : topZ);
			AddBase(points, indices, reverse ? topZ : bottomZ, true);
		}

		public void AddBases(List<Vector2> points, float topZ, float bottomZ, bool reverse = false) {
			AddBases(points, CreateIndices(points), topZ, bottomZ, reverse);
		}

		public static List<int> CreateIndices(List<Vector2> points) {
			var polygon = new Polygon { Points = points.Select(point => point).ToList() };
			return polygon.Triangulate();
		}

		public static List<int> InvertIndices(List<int> indices) {
			var result = new List<int>(indices.Count);
			for (var i = 0; i < indices.Count; i += 3) {
				result.Add(indices[i]);
				result.Add(indices[i + 2]);
				result.Add(indices[i + 1]);
			}
			return result;
		}
	}
}
