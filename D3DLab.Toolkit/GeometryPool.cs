using D3DLab.ECS;
using D3DLab.ECS.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    class GeometryPool : IGeometryMemoryPool,
         IManagerChangeSubscriber<GraphicEntity>
        //IManagerChangeSubscriber<Entity>
        {

        readonly ISynchronizationContext<GeometryPool, GeometryPoolComponent> sync;
        readonly ConcurrentDictionary<Guid, IGeometryData> pool;
        readonly IManagerChangeNotify notify;

        public GeometryPool(IManagerChangeNotify notify) {
            sync = SynchronizationContextBuilder.Create<GeometryPool, GeometryPoolComponent>(this, true);
            pool = new ConcurrentDictionary<Guid, IGeometryData>();
            this.notify = notify;
        }

        public TGeoData GetGeometry<TGeoData>(GeometryPoolComponent com) where TGeoData : IGeometryData {
            if (!pool.TryGetValue(com.Key, out var geo)) {
                throw new Exception("Can't obtain geometry.");
            }
            return (TGeoData)geo;
        }
        public TGeoData GetGeometry<TGeoData>(GraphicEntity entity) where TGeoData : IGeometryData {
            var com = entity.GetComponent<GeometryPoolComponent>();
            return GetGeometry<TGeoData>(com);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="geo">
        /// DO NOT USE GeometryData AFTER PASSING TO METHOD 
        /// COLLECTIONS ARE USED DIRECTLY
        /// </param>
        public void AddGeometry<TGeoData>(GraphicEntity entity, TGeoData geo) where TGeoData : IGeometryData {
            entity.AddComponent(AddGeometry(geo));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geo">
        /// DO NOT USE GeometryData AFTER PASSING TO METHOD 
        /// COLLECTIONS ARE USED DIRECTLY
        /// </param>
        /// <returns></returns>
        public GeometryPoolComponent AddGeometry<TGeoData>(TGeoData geo) where TGeoData : IGeometryData {
            var id = Guid.NewGuid();
            if (!pool.TryAdd(id, geo)) {
                throw new Exception("Can't store geometry.");
            }
            return GeometryPoolComponent.Create(id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGeo">
        /// DO NOT USE GeometryData AFTER PASSING TO METHOD 
        /// COLLECTIONS ARE USED DIRECTLY
        /// </param>
        /// <returns></returns>
        public GeometryPoolComponent UpdateGeometry<TGeoData>(GeometryPoolComponent old, TGeoData newGeo)
            where TGeoData : IGeometryData {            
            var id = Guid.NewGuid();
            if (!pool.TryAdd(id, newGeo)) {
                throw new Exception("Can't store geometry.");
            }
            if (old.IsValid) {
                sync.Add((_this, c) => {
                    if (pool.TryRemove(c.Key, out var oldgeo)) {
                        oldgeo.Dispose();
                    }
                }, old);
            }
            return GeometryPoolComponent.Create(id);
        }

        public void Synchronize(int theadId) {
            sync.Synchronize(theadId);
        }

        public void Add(in GraphicEntity obj) { /* NOT INTERESTING */ }
        public void Remove(in GraphicEntity obj) {
            if (obj.TryGetComponent<GeometryPoolComponent>(out var com)) {
                sync.Add((_this, c) => {
                    if (pool.TryRemove(c.Key, out var oldgeo)) {
                        oldgeo.Dispose();
                    }                    
                }, com);
            }
        }

        public void Dispose() {
            sync.Dispose();
            foreach (var val in pool.Values) {
                val.Dispose();
            }
            pool.Clear();
        }
    }
}
