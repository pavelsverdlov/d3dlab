using D3DLab.Core.Common;
using D3DLab.Core.Entities;
using System.Collections.Generic;

namespace D3DLab.Core.Context {
    public interface IComponentManager {
        ID3DComponent AddComponent(ElementTag tagEntity, ID3DComponent com);
        void RemoveComponent(ElementTag tagEntity, ID3DComponent com);
        T GetComponent<T>(ElementTag tagEntity) where T : ID3DComponent;
        IEnumerable<ID3DComponent> GetComponents(ElementTag tagEntity);
        bool Has<T>(ElementTag tag) where T : ID3DComponent;
    }

}
