using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Systems {
    public abstract class BaseEntitySystem : IComponentSystemIncrementId {
        public int ID { get; set; }
    }

    public abstract class ContainerSystem<TNestedSystem> : BaseEntitySystem {
        readonly SynchronizationContext<ContainerSystem<TNestedSystem>, TNestedSystem> synchronization;
        protected readonly List<TNestedSystem> nested;

        public ContainerSystem() {
            synchronization = new SynchronizationContext<ContainerSystem<TNestedSystem>, TNestedSystem>(this);
            nested = new List<TNestedSystem>();
        }

        public ContainerSystem<TNestedSystem> CreateNested<T>() where T : TNestedSystem {
            synchronization.Add((owner, tech) => {
                owner.nested.Add(tech);
            },Activator.CreateInstance<T>());
            return this;
        }

        protected void Synchronize() {
            synchronization.Synchronize(-1);
        }
    }
}
