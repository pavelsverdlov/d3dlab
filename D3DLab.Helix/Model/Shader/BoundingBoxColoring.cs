using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Model.Shader {
	public class BoundingBoxColoring {
		private float blockRadius;
		public BoundingBox Box { get; set; }
		public Color InvalidPartsColor { get; set; }
		public CilidricalColoringAxis CilidricalColoringAxis { get; set; }
		//block radius if round block;
		public float BlockRadius { get { return blockRadius; } set { blockRadius = value; } }
		public BoundingBoxColoring(BoundingBox box, Color color,float blockRadius,CilidricalColoringAxis cilidricalColoringAxis){
			this.blockRadius = blockRadius;
			Box = box;
			InvalidPartsColor = color;
			CilidricalColoringAxis = cilidricalColoringAxis;
		}
	}
	
// axis of coloring (when checking radius it will check in axis plane): 0 = None, 1 = X, 2 = Y, 3 = Z
	public enum CilidricalColoringAxis{
		None = 0,
		X = 1,
		Y = 2,
		Z = 3,
	}
}
