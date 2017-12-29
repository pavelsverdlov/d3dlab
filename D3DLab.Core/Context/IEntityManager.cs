using D3DLab.Core.Entities;
using System;
using System.Collections.Generic;

namespace D3DLab.Core.Context {
    public interface IEntityManager {
        Entity CreateEntity(ElementTag tag);
        IEnumerable<Entity> GetEntities();
        Entity GetEntity(ElementTag tag);
        void SetFilter(Func<Entity, bool> predicate);
    }

}
