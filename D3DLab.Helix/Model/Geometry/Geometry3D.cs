using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using global::SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;

namespace HelixToolkit.Wpf.SharpDX
{
	[Serializable]
	public abstract class Geometry3D
	{
		protected Geometry3D()
		{
		}

		protected Geometry3D(Vector3[] positions, int[] indices)
		{
			Positions = Vector3Collection.FromArray(positions);
			Indices = IntCollection.FromArray(indices);
		}

		protected Geometry3D(Vector3Collection positions, IntCollection indices)
		{
			Positions = positions;
			Indices = indices;
		}

		private IntCollection indices;
		public IntCollection Indices
		{
			get { return indices; }
			set { SetCollection(ref indices, value, GeometryCollectionType.Indices); }
		}

		private Vector3Collection positions;
		public Vector3Collection Positions
		{
			get { return positions; }
			set
			{
				if (SetCollection(ref positions, value, GeometryCollectionType.Positions))
					bounds = null;
			}
		}

		private Color4Collection colors;
		public Color4Collection Colors
		{
			get { return colors; }
			set { SetCollection(ref colors, value, GeometryCollectionType.Colors); }
		}

		readonly List<IReadOnlySupport> lists = new List<IReadOnlySupport>();
		protected bool SetCollection<T>(ref T field, T value, GeometryCollectionType type) where T : class, IReadOnlySupport
		{
			if (field == value)
				return false;

			if (field != null)
				lock (lists)
					lists.Remove(field);
			field = value;

			if (field != null)
			{
				if (!IsUpdating)
					LockCollection(field);
				lock (lists)
					lists.Add(field);
			}

			OnCollectionChanged(type);

			return true;
		}

		private static void LockCollection(IReadOnlySupport field)
		{
			field.SwitchToReadOnly();
			field.Truncate();
		}

		public bool IsUpdating
		{
			get { return updateCounter > 0; }
		}


		int updateCounter;
		public void BeginUpdate()
		{
			if (updateCounter++ == 0)
				lock (lists)
					lists.ForEach(i => i.SwitchToWritable());
		}
		public void EndUpdate()
		{
			if (--updateCounter == 0)
			{
				lock (lists)
					lists.ForEach(i => LockCollection(i));
				bounds = null;
				OnCollectionChanged(GeometryCollectionType.Any);
			}
		}

		public struct Triangle
		{
			public Vector3 P0, P1, P2;
		}

		public struct Line
		{
			public Vector3 P0, P1;
		}

		BoundingBox? bounds;
		public BoundingBox Bounds
		{
			get
			{
				if (bounds == null && Positions != null)
					bounds = Positions.ToBoundingBox();
				return bounds ?? default(BoundingBox);
			}
		}

		protected virtual void OnCollectionChanged(GeometryCollectionType obj)
		{
			if (obj == GeometryCollectionType.Any || obj == GeometryCollectionType.Positions)
				bounds = null;
			Action<GeometryCollectionType> handler = CollectionChanged;
			if (handler != null)
				handler(obj);
		}
		public event Action<GeometryCollectionType> CollectionChanged;
	}

	public abstract class GeometryRenderArray
	{
		protected GeometryRenderArray(GeometryRenderSources owner, GeometryCollectionType type)
		{
			if (owner == null)
				throw new ArgumentNullException("owner", "owner is null.");
			this.owner = owner;
			this.Type = type;
			this.sourceGetter = GetSourceGetter(type);
		}

		private static Func<Geometry3D, IList> GetSourceGetter(GeometryCollectionType type)
		{
			Func<Geometry3D, Func<MeshGeometry3D, IList>, IList> asMG = (g, f) =>
			{
				var mg = g as MeshGeometry3D;
				if (mg == null)
					return null;
				return f(mg);
			};
			switch (type)
			{
				case GeometryCollectionType.Positions:
					return i => i.Positions;
				case GeometryCollectionType.Indices:
					return i => i.Indices;
				case GeometryCollectionType.Colors:
					return i => i.Colors;
				case GeometryCollectionType.Normals:
					return i => asMG(i, g => g.Normals);
				case GeometryCollectionType.TextureCoordinates:
					return i => asMG(i, g => g.TextureCoordinates);
				case GeometryCollectionType.BiTangents:
					return i => asMG(i, g => g.BiTangents);
				case GeometryCollectionType.Tangents:
					return i => asMG(i, g => g.Tangents);
				default:
					throw new NotImplementedException(string.Format("No implementation for collection type '{0}'", type));
			}
		}

		public GeometryCollectionType Type { get; private set; }
		private readonly GeometryRenderSources owner;
		Func<Geometry3D, IList> sourceGetter;

		public void Update(Geometry3D g)
		{
			if (g == null) { return; }
			UpdateCore(sourceGetter(g));
			owner.SetUpdated();
		}
		protected abstract void UpdateCore(IList source);
	}

	public class GeometryRenderArray<T> : GeometryRenderArray
	{
		public GeometryRenderArray(GeometryRenderSources owner, GeometryCollectionType type)
			: base(owner, type) { }

		public T[] Array { get; private set; }

		public void Update(IList data)
		{
			UpdateCore(data);
		}

		protected override void UpdateCore(IList source)
		{
			if (source == null)
				Array = null;
			else
				Array = ((ObservableList<T>)source).ToArrayFast();
		}
	}

	public class GeometryRenderSources
	{
		internal const int StepsForUpdate = 0;

		public GeometryRenderSources()
		{
			var _arrays = new List<GeometryRenderArray>();
			_arrays.Add(Positions = new GeometryRenderArray<Vector3>(this, GeometryCollectionType.Positions));
			_arrays.Add(Indices = new GeometryRenderArray<int>(this, GeometryCollectionType.Indices));
			_arrays.Add(Colors = new GeometryRenderArray<Color4>(this, GeometryCollectionType.Colors));
			_arrays.Add(Normals = new GeometryRenderArray<Vector3>(this, GeometryCollectionType.Normals));
			_arrays.Add(TextureCoordinates = new GeometryRenderArray<Vector2>(this, GeometryCollectionType.TextureCoordinates));
			_arrays.Add(BiTangents = new GeometryRenderArray<Vector3>(this, GeometryCollectionType.BiTangents));
			_arrays.Add(Tangents = new GeometryRenderArray<Vector3>(this, GeometryCollectionType.Tangents));
			Arrays = _arrays.ToArray();
		}

		public GeometryRenderArray[] Arrays { get; private set; }

		public GeometryRenderArray<Vector3> Positions { get; private set; }
		public GeometryRenderArray<int> Indices { get; private set; }
		public GeometryRenderArray<Color4> Colors { get; private set; }
		public GeometryRenderArray<Vector3> Normals { get; private set; }
		public GeometryRenderArray<Vector2> TextureCoordinates { get; private set; }
		public GeometryRenderArray<Vector3> BiTangents { get; private set; }
		public GeometryRenderArray<Vector3> Tangents { get; private set; }

		/// updatedCounter > 0  - need wait to update render data
		/// updatedCounter == 0 - need update render data
		/// updatedCounter < 0  - no update
		private volatile int updatedCounter = -1;

		public bool IsUpdated
		{
			get { return updatedCounter == 0; }
		}


		public void SetUpdated()
		{
			updatedCounter = StepsForUpdate;
		}

		public void Update(Action<GeometryRenderSources> updater)
		{
			updater(this);
			updatedCounter = StepsForUpdate;
		}

		public bool CheckForUpdate(out RenderArrays _arrays)
		{
			_arrays = null;
			var isUpdated = IsUpdated;
			if (isUpdated)
				_arrays = ToArrays();
			if (updatedCounter >= 0)
				updatedCounter--;
			return isUpdated;
		}

		private static T[] ToArray<T>(GeometryRenderArray<T> list)
		{
			var array = list.Array;
			if (array == null)
				return null;

			return array;
		}

		public RenderArrays ToArrays()
		{
			var _arrays = new RenderArrays()
			{
				Positions = ToArray(Positions),
				Indices = ToArray(Indices),
				Normals = ToArray(Normals),
				Colors = ToArray(Colors),
				TextureCoordinates = ToArray(TextureCoordinates),
				Tangents = ToArray(Tangents),
				BiTangents = ToArray(BiTangents)
			};
			return _arrays;
		}

		public void StartUpdateIfNeeded()
		{
			RenderArrays arrays;
			if (StartUpdate != null && CheckForUpdate(out arrays))
				StartUpdate(arrays);
		}

		public Action<RenderArrays> StartUpdate;
	}

	public class RenderArrays
	{
		public Vector3[] Positions;
		public int[] Indices;
		public Color4[] Colors;
		public Vector3[] Normals;
		public Vector2[] TextureCoordinates;
		public Vector3[] BiTangents;
		public Vector3[] Tangents;
	}

	[Flags]
	public enum GeometryCollectionType
	{
		Nothing = 0x0,
		Positions = 0x1,
		Indices = 0x2,
		Colors = 0x4,
		Normals = 0x8,
		TextureCoordinates = 0x10,
		BiTangents = 0x20,
		Tangents = 0x40,
		Any = Positions | Indices | Colors | Normals | TextureCoordinates | BiTangents | Tangents,
	}
}