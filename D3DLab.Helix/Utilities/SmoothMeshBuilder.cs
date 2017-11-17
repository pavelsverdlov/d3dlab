using System.Collections.Generic;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Utilities {
	public class SmoothMeshBuilder {
		protected Dictionary<Vector3, int> PositionIndices;
		protected Vector3Collection Positions;
		protected IntCollection Indices;

		public SmoothMeshBuilder() {
			PositionIndices = new Dictionary<Vector3, int>();
			Positions = new Vector3Collection();
			Indices = new IntCollection();
		}

		public void Add(IEnumerable<Vector3> positions, IEnumerable<int> indices) {
			var positionIndices = new Dictionary<int, int>();
			var newPositions = new Vector3Collection();
			var newIndex = 0;
			foreach (var position in positions) {
				int realIndex;
				if (!PositionIndices.TryGetValue(position, out realIndex)) {
					realIndex = Positions.Count + newPositions.Count;
					PositionIndices.Add(position, realIndex);
					newPositions.Add(position);
				}
				positionIndices[newIndex] = realIndex;
				newIndex++;
			}
			Positions.AddRange(newPositions);
			foreach (var index in indices)
				Indices.Add(positionIndices[index]);
		}

		public MeshGeometry3D ToMeshGeometry3D() {
			return new MeshGeometry3D { Positions = Positions, Indices = Indices };
		}
	}

}
