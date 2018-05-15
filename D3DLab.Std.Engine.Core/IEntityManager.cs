using System;
using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {
    public interface IEntityManager {
        GraphicEntity CreateEntity(ElementTag tag);
        IEnumerable<GraphicEntity> GetEntities();
        GraphicEntity GetEntity(ElementTag tag);
        void SetFilter(Func<GraphicEntity, bool> predicate);
    }

}
