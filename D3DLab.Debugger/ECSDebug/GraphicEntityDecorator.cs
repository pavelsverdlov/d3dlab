using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D3DLab.Debugger.ECSDebug {
    public class GraphicEntityDecorator {
        readonly GraphicEntity entity;
        public ElementTag Tag => entity.Tag;
        public bool IsDestroyed => entity.IsDestroyed;

        readonly IEnumerable<IGraphicComponent> coms;

        public GraphicEntityDecorator(GraphicEntity entity) {
            this.entity = entity;
            coms = entity.GetComponents().ToList();

        }

        public IEnumerable<IGraphicComponent> GetComponents() {
            return coms;
        }

        public void Remove() {
            if (!entity.IsDestroyed) {
                entity.Remove();
            }
        }
    }
}
