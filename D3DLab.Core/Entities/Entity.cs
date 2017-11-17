using D3DLab.Core.Components;

namespace D3DLab.Core.Entities {
    public class Entity<TData> : ComponentContainer {
        public TData Data { get; set; }
        public Entity(string tag) : base(tag) { }
    }
}