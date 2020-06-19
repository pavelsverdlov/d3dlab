using D3DLab.ECS;
using D3DLab.ECS.Context;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    public class GenneralContextState : BaseContextState {
        public static GenneralContextState Full(ContextStateProcessor processor, AxisAlignedBox octreeBounds, EngineNotificator notificator) {

            var octree = new OctreeManager(processor, octreeBounds, 5);
            notificator.Subscribe(octree);
            var geoPool = new GeometryPool(notificator);
            notificator.Subscribe(geoPool);

            return new GenneralContextState(processor, octree, geoPool, notificator);
        }

        GenneralContextState(ContextStateProcessor processor, IOctreeManager octree, IGeometryMemoryPool geoPool, EngineNotificator notificator)
            : base(processor, new ManagerContainer(notificator, octree, processor, geoPool)) {
        }
    }
}
