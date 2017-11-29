using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public interface IComponentSystem {
        void Execute(IEntityManager emanager,IContext ctx);        
    }
}
