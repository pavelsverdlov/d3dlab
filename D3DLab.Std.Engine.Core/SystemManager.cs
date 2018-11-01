using System;
using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {
    public sealed class SystemManager : ISystemManager {
        readonly List<IComponentSystem> systems = new List<IComponentSystem>();
        public TSystem CreateSystem<TSystem>() where TSystem : class, IComponentSystem {
            var sys = Activator.CreateInstance<TSystem>();
            if(sys is IComponentSystemIncrementId incrementId) {
                incrementId.ID = systems.Count;
            }            
            systems.Add(sys);            
            notify.NotifyChange(sys);
            return sys;
        }
        public IEnumerable<IComponentSystem> GetSystems() {
            return systems;
        }

        public void Dispose() {
            systems.Clear();
        }

        readonly IManagerChangeNotify notify;

        public SystemManager(IManagerChangeNotify notify) {
            this.notify = notify;
        }
    }
}
