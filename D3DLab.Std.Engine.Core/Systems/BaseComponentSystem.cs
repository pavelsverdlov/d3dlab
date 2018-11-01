using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Systems {
    public abstract class BaseComponentSystem : IComponentSystemIncrementId {
        public int ID { get; set; }
    }
}
