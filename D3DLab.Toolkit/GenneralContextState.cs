using D3DLab.ECS;
using D3DLab.ECS.Context;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    class FakeOctreeManager : IOctreeManager {
        public void Dispose() {
        }
        public IEnumerable<ElementTag> GetColliding(ref Ray ray, Func<ElementTag, bool> predicate) {
            throw new NotImplementedException();
        }
        public void Synchronize(int theadId) {
        }
    }
    public class GenneralContextState : BaseContextState {
        public static GenneralContextState Full(ContextStateProcessor processor, AxisAlignedBox octreeBounds, EngineNotificator notificator) {

            var octree = new OctreeManager(processor, octreeBounds, 5);
            notificator.Subscribe(octree);
            var geoPool = new GeometryPool(notificator);
            notificator.Subscribe(geoPool);

            return new GenneralContextState(processor, octree, geoPool, notificator);
        }

        public static GenneralContextState WithoutOctree(ContextStateProcessor processor, EngineNotificator notificator) {
            var geoPool = new GeometryPool(notificator);
            notificator.Subscribe(geoPool);

            return new GenneralContextState(processor, new FakeOctreeManager(), geoPool, notificator);
        }

        GenneralContextState(ContextStateProcessor processor, IOctreeManager octree, IGeometryMemoryPool geoPool, EngineNotificator notificator)
            : base(processor, new ManagerContainer(notificator, octree, processor, geoPool)) {
        }
    }
}
