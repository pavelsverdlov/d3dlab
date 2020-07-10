using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Sync {
    public interface ISynchronization {
        void Synchronize(int theadId);
    }
    public interface ISynchronizationContext : ISynchronization {
        bool HasChanges { get; }
        void FrameSynchronize(int theadId);
        void Dispose();
    }

    public interface ISynchronizationQueue<TOwner, TInput> {
        void Add(Func<TOwner, TInput, bool> action, TInput input);
        void AddRange(Func<TOwner, TInput, bool> action, IEnumerable<TInput> inputs);
    }
}
