using D3DLab.Core.Components;

namespace D3DLab.Core.Entities {
    public class SceneAttachedEntity<TData> : Entity<TData>, IAttachTo<BaseScene>, IAttacmenthOf<BaseScene> {
        public SceneAttachedEntity(string tag) : base("Scene." + tag) {
        }

        public void OnAttach(BaseScene parent) {
            Parent = parent;
        }

        public BaseScene Parent { get; set; }
    }
}