using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Filter {
    public class EntityHasSet {
        readonly Type[] types;
        public EntityHasSet(params Type[] types) {
            this.types = types;
        }

        public bool HasComponents(GraphicEntity entity) {
            return entity.Has(types);
        }
    }
}
