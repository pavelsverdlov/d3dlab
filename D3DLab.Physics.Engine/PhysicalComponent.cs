using BepuPhysics;
using D3DLab.Std.Engine.Core;

namespace D3DLab.Physics.Engine {
    public abstract class PhysicalComponent : GraphicComponent {
        public bool IsConstructed { get; protected set; }


        internal int BodyIndex;
        internal abstract void ConstructBody(Simulation Simulation);
    }
}

