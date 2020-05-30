using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    public class OctreeManager : VisualOctree<ElementTag>,
        IManagerChangeSubscriber<IGraphicComponent>,
        IOctreeManager {
        struct SyncData {
            public AxisAlignedBox Box;
            public ElementTag EntityTag;
        }

        readonly IContextState context;
        protected bool isActualStateDrawed;
        readonly SynchronizationContext<OctreeManager, SyncData> sync;
        readonly object loker;

        public OctreeManager(IContextState context, AxisAlignedBox box, int MaximumChildren) : base(box, MaximumChildren) {
            this.context = context;
            isActualStateDrawed = false;
            loker = new object();
            sync = new SynchronizationContext<OctreeManager, SyncData>(this);
        }

        public void Remove(ref IGraphicComponent com) {
            var manager = context.GetComponentManager();
            if (!manager.Has<HittableComponent>(com.EntityTag)) {
                return;
            }
            switch (com) {
                case HittableComponent h:
                    this.Remove(com.EntityTag);
                    break;
                case TransformComponent tr:
                    if (!manager.Has<GeometryPoolComponent>(com.EntityTag)) {
                        this.Remove(com.EntityTag);
                    }
                    break;
                case GeometryPoolComponent geo:
                    if (!manager.Has<TransformComponent>(com.EntityTag)) {
                        this.Remove(com.EntityTag);
                    }
                    break;
            }
        }

        public void Add(ref IGraphicComponent com) {
            var manager = context.GetComponentManager();
            if (!context.GetComponentManager().Has<HittableComponent>(com.EntityTag)) {
                return;
            }

            TransformComponent tr;
            GeometryPoolComponent geo;
            switch (com) {
                case TransformComponent trcom:
                    if (!manager.TryGet(com.EntityTag, out geo)) {
                        return;
                    }
                    tr = trcom;
                    break;
                case GeometryPoolComponent geocom:
                    if (!manager.TryGet(com.EntityTag, out tr)) {
                        return;
                    }
                    geo = geocom;
                    break;
                default:
                    return;
            }
            var enTag = com.EntityTag;

            if (!geo.IsValid) {
                return;
            }

            var bounds = manager.GetComponent<GeometryBoundsComponent>(enTag);

            var box = bounds.Bounds.Transform(tr.MatrixWorld);

            sync.Add((_this, data) => {
                _this.Add(data.Box, data.EntityTag);
            }, new SyncData { Box = box, EntityTag = enTag });

        }

        public void Synchronize(int theadId) {
            sync.Synchronize(theadId);
        }

        public IEnumerable<ElementTag> GetColliding(ref Ray ray, Func<ElementTag, bool> predicate) {
            return base.GetColliding(ray, predicate).Select(x => x.Item);
        }

        public void Dispose() {
            Clear();
        }
    }
}
