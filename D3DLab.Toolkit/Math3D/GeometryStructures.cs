using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;

using g3;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Math3D {
    public readonly struct HitResultLocal {
        public readonly Vector3 Point;
        public readonly float Distance;
        public readonly bool IsHitted;

        public HitResultLocal(Vector3 point, float distance, bool isHitted) {
            Point = point;
            Distance = distance;
            IsHitted = isHitted;
        }
    }
    public class GeometryStructures<TFormat> : GeometryStructures
        where TFormat: IFileGeometry3D {

        public TFormat OriginGeometry { get; }

        public GeometryStructures(TFormat geo)  {
            OriginGeometry = geo;
            Positions = geo.Positions.ToImmutableArray();
            Indices = geo.Indices.ToImmutableArray();
            Normals = geo.Normals.ToImmutableArray();
            TexCoor = geo.TextureCoors.ToImmutableArray();
            Topology = geo.Topology;
        }
    }

    public abstract class GeometryStructures : IGeometryData {
        public ImmutableArray<Vector3> Positions { get; protected set; }
        public ImmutableArray<int> Indices { get; protected set; }
        public ImmutableArray<Vector3> Normals { get; protected set; }
        public ImmutableArray<Vector2> TexCoor { get; protected set; }
        public GeometryPrimitiveTopologies Topology { get; protected set; }

        public bool IsModified { get; set; }
        public bool IsDisposed { get; private set; }
        public bool IsBuilt { get; private set; }
        public AxisAlignedBox Bounds { get; private set; }
       

        DMeshAABBTree3 TreeLocal;
        DMesh3 DMeshLocal;
       
        public Task BuildTreeAsync() {
            return Task.Run(() => {
                try {
                    var norm = Normals.ConvertToVector3f();
                    DMeshLocal = DMesh3Builder.Build(Positions.ConvertToVector3f(), Indices, norm);

                    TreeLocal = new DMeshAABBTree3(DMeshLocal);
                    TreeLocal.Build();

                    Bounds = new AxisAlignedBox(DMeshLocal.GetBounds());

                    IsBuilt = true;
                }catch(Exception ex) {
                    Debug.WriteLine($"BuildTreeAsync {ex.Message}");
                }
                return this;
            });
        }
        public IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal) {
            HitResultLocal res = default;
            var ray = new Ray3d(rayLocal.Position.ToVector3f(), rayLocal.Direction.ToVector3f());
            try {
                int hit_tid = TreeLocal.FindNearestHitTriangle(ray);
                if (hit_tid == DMesh3.InvalidID) {
                    return Enumerable.Empty<HitResultLocal>();
                }

                var intr = MeshQueries.TriangleIntersection(DMeshLocal, hit_tid, ray);
                var distance = (float)ray.Origin.Distance(ray.PointAt(intr.RayParameter));
                var point = intr.Triangle.V1.ToVector3();
                res = new HitResultLocal(point, distance, true);
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }

            return new[] { res };
        }
        public void ReCalculateNormals() {
            Normals = Positions.ToList().CalculateNormals(Indices.ToList()).ToImmutableArray();
        }

        public virtual void Dispose() {
            IsDisposed = true;
            TreeLocal = null;
            DMeshLocal = null;
            IsBuilt = false;
            Bounds = AxisAlignedBox.Zero;
        }

    }
}
