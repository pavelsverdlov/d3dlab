using D3DLab.Core.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Components {
    public interface IComponentSystem {
        void Execute(IEntityManager emanager, IInputManager input, IViewportContext ctx);        
    }
}
