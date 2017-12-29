using D3DLab.Core.Components;
using System.Collections.Generic;

namespace D3DLab.Core.Context {
    public interface ISystemManager {
        TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem;
        IEnumerable<IComponentSystem> GetSystems();
        //void AddSystem(IComponentSystem system);
    }

}
