using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core {   
    public abstract class GameObject {
        public abstract void Hide(IEntityManager manager);
        public abstract void Show(IEntityManager manager);
    }
}
