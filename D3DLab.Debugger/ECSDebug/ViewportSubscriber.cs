using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace D3DLab.Debugger.ECSDebug {
    public interface IViewportNotifier {
        void AddSystem(IGraphicSystem sys);
        void GraphicEntityChange(GraphicEntityDecorator entity);
        void FrameRendered(IEnumerable<GraphicEntityDecorator> en);
    }

    public sealed class ViewportSubscriber :
         IManagerChangeSubscriber<GraphicEntity>,
         IManagerChangeSubscriber<IGraphicSystem>,
         IEntityRenderSubscriber {
        private readonly IViewportNotifier notify;

        public ViewportSubscriber(IViewportNotifier notify) {
            this.notify = notify;
        }

        public void Add(in IGraphicSystem sys) {
            var s = sys;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                notify.AddSystem(s);
            }));
        }
        public void Remove(in IGraphicSystem obj) {
        }

        public void Remove(in GraphicEntity obj) {
        }

        public void Add(in GraphicEntity entity) {
            var en = new GraphicEntityDecorator(entity);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                notify.GraphicEntityChange(en);
            }));
        }       

        public void Render(IEnumerable<GraphicEntity> entities) {
            if (Application.Current == null) { return; }
            var en = entities.Select(x => new GraphicEntityDecorator(x)).ToList();
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                notify.FrameRendered(en);
            }));
        }
    }
}
