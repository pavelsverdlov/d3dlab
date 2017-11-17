
namespace HelixToolkit.Wpf.SharpDX
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::SharpDX;

    using HelixToolkit.Wpf.SharpDX.Core;

    [Serializable]
    public class MeshGeometry3D : Geometry3D
    {
		public MeshGeometry3D() : base()
		{
		}

		public MeshGeometry3D(Vector3[] positions, int[] indices, Vector3[] normals = null)
			: base(positions, indices)
		{
			if (normals != null)
				Normals = Vector3Collection.FromArray(normals);
		}

		public MeshGeometry3D(Vector3Collection positions, IntCollection indices, Vector3Collection normals = null)
			: base(positions, indices)
		{
			if (normals != null)
				Normals = normals;
		}

		private Vector3Collection normals;
		public Vector3Collection Normals
		{
			get { return normals; }
			set { SetCollection(ref normals, value, GeometryCollectionType.Normals); }
		}

		private Vector2Collection textureCoordinates;
		public Vector2Collection TextureCoordinates
		{
			get { return textureCoordinates; }
			set { SetCollection(ref textureCoordinates, value, GeometryCollectionType.TextureCoordinates); }
		}

		private Vector3Collection tangents;
		public Vector3Collection Tangents
		{
			get { return tangents; }
			set { SetCollection(ref tangents, value, GeometryCollectionType.Tangents); }
		}

		private Vector3Collection biTangents;
		public Vector3Collection BiTangents
		{
			get { return biTangents; }
			set { SetCollection(ref biTangents, value, GeometryCollectionType.BiTangents); }
		}

        public IEnumerable<Geometry3D.Triangle> Triangles
        {
            get
            {
                for (int i = 0; i < Indices.Count; i += 3)
                {
                    yield return new Triangle() { P0 = Positions[Indices[i]], P1 = Positions[Indices[i + 1]], P2 = Positions[Indices[i + 2]], };
                }
            }
        }

        public MeshGeometry3D Clone() {
            return new MeshGeometry3D {
                 Normals = Normals == null ? null : new Vector3Collection(Normals),
                 Indices = Indices == null ? null : new IntCollection(Indices),
                 Positions = Positions == null ? null : new Vector3Collection(Positions),
                 Colors = Colors == null ? null : new Color4Collection(Colors),
                 TextureCoordinates = TextureCoordinates == null ? null : new Vector2Collection(TextureCoordinates),
                 Tangents = Tangents == null ? null : new Vector3Collection(Tangents),
                 BiTangents = BiTangents == null ? null : new Vector3Collection(BiTangents)
            };
        }

        public MeshGeometry3D ClonePositionAndIndices() {
            return new MeshGeometry3D {
                Indices = new IntCollection(Indices.Copy()),
                Positions=new Vector3Collection(Positions.Copy())
            };
        }

        public static MeshGeometry3D Merge(params MeshGeometry3D[] meshes)
        {
            var positions = new Vector3Collection();
            var indices = new IntCollection();

            var normals = meshes.All(x => x.Normals != null) ? new Vector3Collection() : null;
            var colors = meshes.All(x => x.Colors != null) ? new Color4Collection() : null;
            var textureCoods = meshes.All(x => x.TextureCoordinates != null) ? new Vector2Collection() : null;
            var tangents = meshes.All(x => x.Tangents != null) ? new Vector3Collection() : null;
            var bitangents = meshes.All(x => x.BiTangents != null) ? new Vector3Collection() : null;

            int index = 0;
            foreach (var part in meshes)
            {
                for (int i = 0; i < part.Positions.Count; i++)
                {
                    positions.Add(part.Positions[i]);
                }

                for (int i = 0; i < part.Indices.Count; i++)
                {
                    indices.Add(index + part.Indices[i]);
                }

                index += part.Indices.Count;
            }

            if (normals != null)
            {
                normals = new Vector3Collection(meshes.SelectMany(x => x.Normals));
            }

            if (colors != null)
            {
                colors = new Color4Collection(meshes.SelectMany(x => x.Colors));
            }

            if (textureCoods != null)
            {
                textureCoods = new Vector2Collection(meshes.SelectMany(x => x.TextureCoordinates));
            }

            if (tangents != null)
            {
                tangents = new Vector3Collection(meshes.SelectMany(x => x.Tangents));
            }

            if (bitangents != null)
            {
                bitangents = new Vector3Collection(meshes.SelectMany(x => x.BiTangents));
            }

            var mesh = new MeshGeometry3D()
            {
                Positions = positions,
                Indices = indices,
            };

            mesh.Normals = normals;
            mesh.Colors = colors;
            mesh.TextureCoordinates = textureCoods;
            mesh.Tangents = tangents;
            mesh.BiTangents = bitangents;

            return mesh;
        }
    }
}
