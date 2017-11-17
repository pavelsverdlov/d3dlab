namespace D3DLab.Core.Components {
        public interface IAttachTo<in T> : ICanAttach {
            void OnAttach(T parent);
        }

    public interface IAttacmenthOf<out T> {
        T Parent { get; }
    }
        public interface ICanAttach {
            void AttachTo<T>(T parent) where T : Component;
        }

    public interface IDependentBy<in T> : IHaveDependency where T : Component {
        void OnAttach(T dependence);
    }
    public interface IHaveDependency {
        void AttachTo<T>(T dependence) where T : Component;
    }
}
