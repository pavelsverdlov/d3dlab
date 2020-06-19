using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit {
    public class OctreeManager : VisualOctree<ElementTag>,
        IManagerChangeSubscriber<GeometryPoolComponent>,
        IManagerChangeSubscriber<TransformComponent>,
        IManagerChangeSubscriber<HittableComponent>,
        IManagerChangeSubscriber<GraphicEntity>,
        IOctreeManager {

        struct SyncData {
            public AxisAlignedBox Box;
            public ElementTag EntityTag;
        }

        readonly IContextState context;
        public bool IsDrawingBoxesEnable { get; private set; }
        readonly SynchronizationContext<OctreeManager, SyncData> sync;
        readonly object loker;
        readonly Task drawerTask;
        public OctreeManager(IContextState context, AxisAlignedBox box, int MaximumChildren) : base(box, MaximumChildren) {
            this.context = context;
            IsDrawingBoxesEnable = false;
            loker = new object();
            sync = new SynchronizationContext<OctreeManager, SyncData>(this);
            drawerTask = Task.CompletedTask;
        }

        #region Subscribers

        public void Add(in TransformComponent tr) {
            var entity = context.GetEntityManager().GetEntityOf(tr);
            if (entity.TryGetComponents<GeometryPoolComponent, HittableComponent>(out var geo, out var _)) {
                Add(entity, tr, geo);
            }
        }
        public void Add(in HittableComponent com) {
            var entity = context.GetEntityManager().GetEntityOf(com);
            if (entity.TryGetComponents<TransformComponent, GeometryPoolComponent>(out var tr, out var geo)) {
                Add(entity, tr, geo);
            }
        }
        public void Add(in GeometryPoolComponent geo) {
            var entity = context.GetEntityManager().GetEntityOf(geo);
            if (entity.TryGetComponents<TransformComponent, HittableComponent>(out var tr, out var _)) {
                Add(entity, tr, geo);
            }
        }

        public void Remove(in HittableComponent com) {
            var entity = context.GetEntityManager().GetEntityOf(com);
            this.Remove(entity.Tag);
        }
        public void Remove(in TransformComponent com) {
            //var entity = context.GetEntityManager().GetEntityOf(com);
            //this.Remove(entity.Tag);
        }
        public void Remove(in GeometryPoolComponent com) {
            //var entity = context.GetEntityManager().GetEntityOf(com);
            //this.Remove(entity.Tag);
        }

        public void Add(in GraphicEntity obj) { }
        public void Remove(in GraphicEntity obj) {
            if (obj.Contains<HittableComponent>()) {
                TryRemove(obj.Tag);
            }
        }

        #endregion

        void Add(GraphicEntity entity, in TransformComponent tr, in GeometryPoolComponent geo) {
            var enTag = entity.Tag;

            if (!geo.IsValid) {
                return;
            }

            var bounds = entity.GetComponent<GeometryBoundsComponent>();

            var box = bounds.Bounds.Transform(tr.MatrixWorld);

            sync.Add((_this, data) => {
                _this.TryRemove(data.EntityTag);
                _this.Add(data.Box, data.EntityTag);
            }, new SyncData { Box = box, EntityTag = enTag });
        }

        public void Synchronize(int theadId) {
            var changed = sync.IsChanged;
            sync.Synchronize(theadId);

            if (changed && IsDrawingBoxesEnable) {
                drawerTask.ContinueWith(_ => {
                    this.Draw(context);
                });
            }
        }

        public IEnumerable<ElementTag> GetColliding(ref Ray ray, Func<ElementTag, bool> predicate) {
            return base.GetColliding(ray, predicate).Select(x => x.Item);
        }

        public void Dispose() {
            Clear();
        }

        public void EnableDrawingBoxes() {
            IsDrawingBoxesEnable = true;
            drawerTask.ContinueWith(_ => {
                this.Draw(context);
            });
        }
        public void DisableDrawingBoxes() {
            IsDrawingBoxesEnable = false;
            drawerTask.ContinueWith(_ => {
                ClearDrew(context);
            });
        }


    }
}
