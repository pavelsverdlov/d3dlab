using System;
using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {
    public interface IEntityManager {
        Entity CreateEntity(ElementTag tag);
        IEnumerable<Entity> GetEntities();
        Entity GetEntity(ElementTag tag);
        void SetFilter(Func<Entity, bool> predicate);
    }

}
