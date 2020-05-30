using System;
using System.Collections.Generic;

namespace D3DLab.ECS {

    public interface IComponentManager : ISynchronizationContext {
        void AddComponents(ElementTag tagEntity, IEnumerable<IGraphicComponent> com);
        IGraphicComponent AddComponent(ElementTag tagEntity, IGraphicComponent com);
        void RemoveComponent(ElementTag tagEntity, IGraphicComponent com);
        void RemoveComponents<T>(ElementTag tagEntity) where T : IGraphicComponent;
        void RemoveComponents(ElementTag tagEntity, params IGraphicComponent[] components);


        T GetComponent<T>(ElementTag tagEntity) where T : IGraphicComponent;
        IEnumerable<T> GetComponents<T>(ElementTag tagEntity) where T : IGraphicComponent;
        IEnumerable<IGraphicComponent> GetComponents(ElementTag tagEntity);
        T GetOrCreateComponent<T>(ElementTag tagEntity, T newone) where T : IGraphicComponent;
        IEnumerable<T> GetComponents<T>() where T : IGraphicComponent;


        bool TryGet<T>(ElementTag tagEntity, out T component) where T : IGraphicComponent;
        bool TryGet<T1, T2>(ElementTag tagEntity, out T1 c1, out T2 c2)
           where T1 : IGraphicComponent
           where T2 : IGraphicComponent;


        bool Has<T>(ElementTag tag) where T : IGraphicComponent;
        bool Has(ElementTag tag, params Type[] types);


        IEnumerable<IGraphicComponent> GetComponents(ElementTag tag, params Type[] types);

        void UpdateComponents<T>(ElementTag tagEntity, T com) where T : IGraphicComponent;

        void Dispose();
    }

}
