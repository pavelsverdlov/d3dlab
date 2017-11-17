using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public static class ReconstructionBuilder {
        
        public static void Build(IEntityContext context) {
            var rec = context.CreateEntity("ReconstructionEntity");
            var sup = context.CreateEntity("SupportEntity");
            var arrow = context.CreateEntity("ArrowEntity");

            

        }

    }
}
