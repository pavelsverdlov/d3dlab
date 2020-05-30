using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS {
    public interface IOctreeManager : 
        ISynchronizationContext, IDisposable {
        IEnumerable<ElementTag> GetColliding(ref Ray ray, Func<ElementTag, bool> predicate);
    }   
}
