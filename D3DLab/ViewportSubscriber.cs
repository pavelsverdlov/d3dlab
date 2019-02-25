using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;

namespace D3DLab {
    public sealed class ViewportSubscriber :
        IManagerChangeSubscriber<GraphicEntity>,
        IManagerChangeSubscriber<IGraphicSystem>,
        IEntityRenderSubscriber {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Change(GraphicEntity entity) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Add(entity);
            }));
        }

        public void Change(IGraphicSystem sys) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.SystemsView.AddSystem(sys);
            }));
        }

        public void Render(IEnumerable<GraphicEntity> entities) {
            if (App.Current == null) { return; }
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Refresh(entities);
            }));
        }
    }


}
