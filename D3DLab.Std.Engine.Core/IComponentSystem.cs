

namespace D3DLab.Std.Engine.Core {
    internal interface IComponentSystemIncrementId {
        int ID { set; }
    }
    public interface IComponentSystem {
        int ID { get; }
        void Execute(SceneSnapshot snapshot);        
    }
}
