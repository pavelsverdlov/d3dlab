using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core;
using D3DLab.Core.Host;
using D3DLab.Core.Viewport;
using D3DLab.Core.Visual3D;
using D3DLab.Debugger.Windows;
using D3DLab.Core.Test;
using System.Windows.Input;
using D3DLab.Properties;
using System.Windows;
using D3DLab.Core.Entities;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO;
using SharpDX;

namespace D3DLab {
   
    public sealed class MainWindowViewModel {
        private SceneView scene;
        private readonly ViewportSubscriber subscriber;

        public VisualTreeviewerPopup VisualTreeviewer { get; set; }
        public ICommand LoadDuck { get; set; }

        public ICollectionView Items { get; set; }
        readonly ObservableCollection<LoadedItem> items;

        public MainWindowViewModel() {
            LoadDuck = new Command(this);
            VisualTreeviewer = new VisualTreeviewerPopup();
            subscriber = new ViewportSubscriber(this);
            items = new ObservableCollection<LoadedItem>();
            Items = CollectionViewSource.GetDefaultView(items);
            
        }

        public void Init(FormsHost host, FrameworkElement overlay) {
            scene = new SceneView(host, overlay);
            scene.Notificator.Subscribe(subscriber);

            VisualTreeviewer.Show();
        }


        private class Command : ICommand {
            private MainWindowViewModel main;

            public Command(MainWindowViewModel mainWindowViewModel) {
                this.main = mainWindowViewModel;
            }

            public event EventHandler CanExecuteChanged = (s, r) => { };


            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                var item = main.scene.LoadObj(this.GetType().Assembly.GetManifestResourceStream("D3DLab.Resources.ducky.obj"));
                main.items.Add(item);
            }
        }
    }

    public sealed class ViewportSubscriber : IViewportChangeSubscriber<Entity>, IViewportRenderSubscriber {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Add(Entity entity) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.ViewModel.Add(entity);
            }));
        }

        public void Render(IEnumerable<Entity> entities) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.ViewModel.Refresh(entities);
            }));
        }
    }

    public sealed class LoadedItem {
        public LoadedItem() { }

        readonly ElementTag duckTag;

        public LoadedItem(IEntityManager emanager, ElementTag duckTag, ElementTag arrowZtag, ElementTag arrowXtag, ElementTag arrowYtag) {
            this.duckTag = duckTag;
        }
        public override string ToString() {
            return duckTag.ToString();
        }
    }

    public sealed class SceneView : D3DEngine {
        public SceneView(FormsHost host, FrameworkElement overlay) : base(host, overlay) {
        }

        public LoadedItem LoadObj(Stream content) {
            HelixToolkit.Wpf.SharpDX.ObjReader readerA = new HelixToolkit.Wpf.SharpDX.ObjReader();
            var res = readerA.Read(content);

            var dic = new Dictionary<string, HelixToolkit.Wpf.SharpDX.MeshBuilder>();
            foreach (var gr in readerA.Groups) {
                var key = gr.Name.Split(' ')[0];
                HelixToolkit.Wpf.SharpDX.MeshBuilder value;
                if (!dic.TryGetValue(key, out value)) {
                    value = new HelixToolkit.Wpf.SharpDX.MeshBuilder(true, false);
                    dic.Add(key, value);
                }
                value.Append(gr.MeshBuilder);
            }

            var index = 0;
            var builder = new HelixToolkit.Wpf.SharpDX.MeshBuilder(true, false);
            foreach (var item in dic) {
                builder.Append(item.Value);
            }
            var duck = VisualModelBuilder.Build(EntityManager, builder.ToMeshGeometry3D(), "duck" + Guid.NewGuid().ToString());
            var arrowz = ArrowBuilder.Build(EntityManager, Vector3.UnitZ, SharpDX.Color.Yellow);
            var arrowx = ArrowBuilder.Build(EntityManager, Vector3.UnitX, SharpDX.Color.Blue);
            var arrowy = ArrowBuilder.Build(EntityManager, Vector3.UnitY, SharpDX.Color.Green);
            var entities = new[] { duck, arrowz, arrowx, arrowy };
            var interactor = new EntityInteractor();
            interactor.ManipulateInteractingTwoWays(entities);
            //interactor.ManipulateInteractingTwoWays(duck, arrowx);
            //interactor.ManipulateInteractingTwoWays(duck, arrowy);
            //interactor.ManipulateInteracting(arrow, duck);

            return new LoadedItem(EntityManager, duck.Tag, arrowz.Tag, arrowx.Tag, arrowy.Tag);
        }
    }
}
