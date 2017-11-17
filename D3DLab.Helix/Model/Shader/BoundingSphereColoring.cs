using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Model.Shader
{
	public class SphereColoring
	{
		public Vector3 Position { get; set; }
		public float SphereRadius { get; set; }
		public float SphereOffset { get; set; }
		public Color Color { get; set; }
		/// <summary>
		/// class that contains data for "brush" color draving VISUALIZATION used in shader;
		/// </summary>
		/// <param name="position">position of the sphere</param>
		/// <param name="sphereRadius">radius of the sphere (brush size)</param>
		/// <param name="sphereOffste">color offset - the distance from the border of brush where color start to blend with mesh color</param>
		/// <param name="color"></param>
		public SphereColoring(Vector3 position, float sphereRadius, float sphereOffste, Color color)
		{
			if (sphereOffste > sphereRadius)
			{
				throw new Exception("OFFSET CANNOT BE BIGGER THAN SPHERE RADIUS");
			}
			Position = position;
			SphereRadius = sphereRadius;
			SphereOffset = sphereOffste;
			Color = color;
		}
	}
}
