using System.Collections.Generic;

namespace D3DLab.Std.Engine.Core {
    public interface IComponentManager : ISynchronizationContext {
        IGraphicComponent AddComponent(ElementTag tagEntity, IGraphicComponent com);
        void RemoveComponent(ElementTag tagEntity, IGraphicComponent com);
        T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent;
        IEnumerable<T> GetComponents<T>(ElementTag tagEntity) where T : IGraphicComponent;
        IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity);
        bool Has<T>(ElementTag tag) where T : IGraphicComponent;

        void Dispose();
    }

}
