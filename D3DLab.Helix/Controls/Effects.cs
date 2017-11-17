using System;
using System.Linq;
using System.Collections.Generic;
using global::SharpDX;
using global::SharpDX.DXGI;
using global::SharpDX.Direct3D11;
using global::SharpDX.D3DCompiler;
using global::SharpDX.Direct3D;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using HelixToolkit.Wpf.SharpDX.Render;

namespace HelixToolkit.Wpf.SharpDX
{
	public class RenderTechnique : IComparable
	{
		public RenderTechnique(string name)
		{
			this.Name = name;
		}

		public override string ToString()
		{
			return this.Name;
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this.Name.Equals(obj.ToString());
		}

		public int CompareTo(object obj)
		{
			return Name.CompareTo(obj.ToString());
		}

		public static bool operator ==(RenderTechnique a, RenderTechnique b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			// Return true if the fields match:
			return a.Name.Equals(b.Name);
		}

		public static bool operator !=(RenderTechnique a, RenderTechnique b)
		{
			return !(a == b);
		}

		public string Name { get; private set; }

        public virtual void UpdateVariables(Effect variables) {
            //TODO: set variables of this RenderTechnique
	    }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DefaultVertex
	{
		public Vector4 Position;
		public Color4 Color;
		public Vector2 TexCoord;
		public Vector3 Normal;
		//public Vector3 Tangent;
		//public Vector3 BiTangent;

		public const int SizeInBytes = 4 * (0
			+ 4 // Position
			+ 4 // Color
			+ 2 // TexCoord
			+ 3 // Normal
			//+ 3 // Tangent
			//+ 3   // BiTangent
			);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct LinesVertex
	{
		public Vector4 Position;
		public Color4 Color;
		public const int SizeInBytes = 4 * (4 + 4);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct CubeVertex
	{
		public Vector4 Position;
		public const int SizeInBytes = 4 * 4;
	}
}
