using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using g3;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace D3DLab.Std.Engine.Core.Components {
    public class HittableGeometryComponent : GeometryComponent, IGeometryComponent {
        public bool IsBuilt { get; private set; }

        #region geometry3Sharp 

        internal DMeshAABBTree3 Tree { get; private set; }
        internal DMesh3 DMesh { get; private set; }

        public HittableGeometryComponent() {

        }

        internal Task<HittableGeometryComponent> BuildTreeAsync() {
            if (!IsValid) { return Task<HittableGeometryComponent>.FromResult(this); }
            return Task.Run(() => {
                var norm = Normals.IsDefaultOrEmpty ? null : ConvertToVector3f(Normals);
                DMesh = DMesh3Builder.Build(ConvertToVector3f(Positions), Indices, norm);

                //var sm = new LaplacianMeshSmoother(DMesh);
                //sm.Initialize();
                //sm.SolveAndUpdateMesh();

                //DMesh = sm.Mesh;

                 Tree = new DMeshAABBTree3(DMesh);
                Tree.Build();
                IsBuilt = true;
                return this;
            });
        }
        #endregion

        protected override BoundingBox CalcuateBox() {
            return IsBuilt ? BoundingBox.CreateFromComponent(this) : BoundingBox.Zero;
        }

        static Vector3f[] ConvertToVector3f(ImmutableArray<Vector3> pos) {
            var v3f = new Vector3f[pos.Length];
            for (var i = 0; i < v3f.Length; ++i) {
                v3f[i] = pos[i].ToVector3f();
            }
            return v3f;
        }

    }
    /*
    public class GeometryComponent : BaseGeometryComponent, IGeometryComponent {

        //TODO: maybe need to separate DMesh functional to HittableGeometryComponent
        #region geometry3Sharp 

        internal bool IsBuilt { get; private set; }
        internal DMeshAABBTree3 Tree { get; private set; }
        internal DMesh3 DMesh { get; private set; }

        internal Task<GeometryComponent> BuildTreeAsync() {
            if (!IsValid) { return Task<GeometryComponent>.FromResult(this); }
            return Task.Run(() => {
                var norm = Normals.IsDefaultOrEmpty ? null : ConvertToVector3f(Normals);
                DMesh = DMesh3Builder.Build(ConvertToVector3f(Positions), Indices, norm);
                Tree = new DMeshAABBTree3(DMesh);
                Tree.Build();
                IsBuilt = true;
                return this;
            });
        }
        #endregion

        protected override BoundingBox CalcuateBox() {
            return IsBuilt ? BoundingBox.CreateFromComponent(this) : BoundingBox.Empty;
        }

        static Vector3f[] ConvertToVector3f(ImmutableArray<Vector3> pos) {
            var v3f = new Vector3f[pos.Length];
            for(var i=0;i < v3f.Length; ++i) {
                v3f[i] = pos[i].ToVector3f();
            }
            return v3f;
        }
    }*/

    public class SimpleGeometryComponent : GeometryComponent, IGeometryComponent {
        protected override BoundingBox CalcuateBox() {
            return BoundingBox.Zero;
        }
    }


    public abstract class GeometryComponent : GraphicComponent {
        public override bool IsValid => Positions.Length > 0 && Indices.Length > 0;

        public virtual ImmutableArray<Vector3> Positions { get; set; }
        public virtual ImmutableArray<Vector3> Normals { get; set; }
        public virtual ImmutableArray<Vector4> Colors {
            get {
                if (!colors.Any()) {
                    colors = Positions.Length.SelectToList(() => Color).ToImmutableArray();
                }
                return colors;
            }
            set {
                colors = value;
            }
        }
        public ImmutableArray<Vector2> TextureCoordinates { get; set; }
        public ImmutableArray<int> Indices { get; set; }
        
        [Obsolete("MOve to color component")]
        public Vector4 Color { get; set; }

        public BoundingBox Box => CalcuateBox();

        ImmutableArray<Vector4> colors;
        readonly Lazy<BoundingBox> box;

        public GeometryComponent() {
            colors = ImmutableArray<Vector4>.Empty;
            TextureCoordinates = ImmutableArray<Vector2>.Empty;
            IsModified = true;
            //
           // box = new Lazy<BoundingBox>(CalcuateBox);
        }

        protected abstract BoundingBox CalcuateBox();

        public void MarkAsRendered() {
            IsModified = false;
        }        
    }
}
