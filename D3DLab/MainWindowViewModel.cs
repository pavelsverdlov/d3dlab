using System;
using System.Collections.Generic;
using D3DLab.Debugger.Windows;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO;
using D3DLab.Std.Engine.Core;
using D3DLab.Wpf.Engine.App.Host;
using System.Numerics;
using D3DLab.Wpf.Engine.App;
using Veldrid.Utilities;
using System.Linq;

namespace D3DLab {

    public sealed class MainWindowViewModel {
        private SceneView scene;
        private readonly EngineNotificator notificator;
        ContextStateProcessor context;

        public VisualTreeviewerViewModel VisualTreeviewer { get; set; }

        public ICommand LoadDuck { get; set; }

        public ICollectionView Items { get; set; }
        readonly ObservableCollection<LoadedItem> items;

        public MainWindowViewModel() {
            LoadDuck = new Command(this);
            VisualTreeviewer = new VisualTreeviewerViewModel();
            
            items = new ObservableCollection<LoadedItem>();
            Items = CollectionViewSource.GetDefaultView(items);
             notificator = new EngineNotificator();

            notificator.Subscribe(new ViewportSubscriber(this));
        }

        public void Init(FormsHost host, FrameworkElement overlay) {
            context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));

            context.SwitchTo(0);

            scene = new SceneView(host, overlay, context, notificator);
            scene.RenderStarted += OnRenderStarted;


            VisualTreeviewer.RenderModeSwither = new RenderModeSwitherCommand(context);
        }

        private void OnRenderStarted() {
            VisualTreeviewer.GameWindow = scene.Window;

            
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
        public class RenderModeSwitherCommand : Debugger.BaseWPFCommand<Debugger.Infrastructure.IVisualTreeEntityItem> {
            readonly IContextState context;

            public RenderModeSwitherCommand(IContextState context) {
                this.context = context;
            }
            public override void Execute(Debugger.Infrastructure.IVisualTreeEntityItem item) {
                var tag = item.Name;
                //var sw = new EntityRenderModeSwither(context.GetEntityManager().GetEntity(tag));
                //sw.TurnOn(EntityRenderModeSwither.Modes.BoundingBox);
            }
        }
    }

    public sealed class GenneralContextState : BaseContextState {
        public GenneralContextState(ContextStateProcessor processor, EngineNotificator notificator) : base(processor,new ManagerContainer(notificator)) {
        }
    }

    public sealed class ViewportSubscriber : IManagerChangeSubscriber<GraphicEntity>, IEntityRenderSubscriber {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Change(GraphicEntity entity) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Add(entity);
            }));
        }

        public void Render(IEnumerable<GraphicEntity> entities) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Refresh(entities);
            }));
        }
    }

    public sealed class LoadedItem {
        public ICommand VisiblityChanged { get; set; }
        public string Header { get { return duckTag.ToString(); } }

        public LoadedItem() { }

        readonly ElementTag duckTag;
        readonly IEntityManager emanager;

        public LoadedItem(IEntityManager emanager, ElementTag duckTag) {
            this.emanager = emanager;
            this.duckTag = duckTag;
            VisiblityChanged = new Command(this);
        }
        public override string ToString() {
            return duckTag.ToString();
        }

        private class Command : ICommand {
            private LoadedItem item;
            private readonly InvisibleComponent com;

            public Command(LoadedItem item) {
                this.item = item;
                com = new InvisibleComponent();
            }

            public event EventHandler CanExecuteChanged = (s, r) => { };


            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                var _checked = (bool?)parameter;
                if (!_checked.HasValue) {
                    return;
                }
                var tag = item.duckTag;
                //if (_checked.Value) {
                //    item.emanager.GetEntity(tag).AddComponent(com);
                //    item.emanager.SetFilter(x => !x.Has<InvisibleComponent>());
                //} else {
                //    item.emanager.GetEntity(tag).RemoveComponent(com);
                //    item.emanager.SetFilter(x => true);
                //}
            }

            public sealed class InvisibleComponent : GraphicComponent {

            }
        }
    }

    public sealed class SceneView : Wpf.Engine.App.Scene {

        public SceneView(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, IEntityRenderNotify notify) 
            : base(host, overlay, context, notify) {


            //try {
            //    Fwk.ImageSharp.ImagePr.Load(Path.Combine(AppContext.BaseDirectory, "Textures", "spnza_bricks_a_diff.png"));
            //} catch (Exception ex) {
            //    ex.ToString();
            //}

            //var center = new Vector3();
            //var point = new Vector3(10, 10, 10);
            //var res = point + center;

            //var v = new Vector3(10, 10, 10) + new Vector3(5, 20, 0);
            //var v = new Vector3(5, 20, 0) - new Vector3(10, 10, 10);
            //var normal = v;
            //normal.Normalize();

            //var point1 = new Vector3(5, 20, 0) - normal * v.Length()/2;
            //var point2 = new Vector3(10, 10, 10) + normal * v.Length() / 2;

            

        }


        public LoadedItem LoadObj(Stream content) {
            var entityManager = Context.GetEntityManager();

            var parser = new CncParser(new FileInfo(@"C:\Storage\trash\ncam\6848-Straumann_Tisue_4.8.cnc.obj"));
            var points = parser.GetPaths()[0].Points.ToArray();
            // LineEntityBuilder.Build(context, parser.GetPaths()[0].Points.ToArray());

            var id = entityManager.BuildLineEntity(points);

            return new LoadedItem(entityManager, id);

            //duck


            var readerA = new Std.Engine.Core.Utilities.Helix.ObjReader();
            var res = readerA.Read(content);

            //var dic = new Dictionary<string, Std.Engine.Core.Utilities.Helix.MeshBuilder>();
            //foreach (var gr in readerA.Groups) {
            //    var key = gr.Name;//.Split(' ')[0];
            //    Std.Engine.Core.Utilities.Helix.MeshBuilder value;
            //    if (!dic.TryGetValue(key, out value)) {
            //        value = new Std.Engine.Core.Utilities.Helix.MeshBuilder(false, false);
            //        dic.Add(key, value);
            //    }
            //    value.Append(gr.MeshBuilder);
            //}

            //var builder = new Std.Engine.Core.Utilities.Helix.MeshBuilder(false, false);
            //foreach (var item in dic) {
            //    builder.Append(item.Value);
            //}

            //var mesh = res[0].Geometry;// builder.ToMeshGeometry3D();

            res[0].Geometry.Color = new Vector4(1, 0, 0, 1); 
            res[1].Geometry.Color = new Vector4(0, 1, 0, 1); 
            res[2].Geometry.Color = new Vector4(0, 0, 1, 1); 
            res[3].Geometry.Color = new Vector4(1, 1, 0, 1);

            var duckTag = entityManager.BuildGroupMeshElement(res.Select(x=>x.Geometry));

            return new LoadedItem(entityManager, duckTag);

            //var entityManager = Context.GetEntityManager();

            //var duck = VisualModelBuilder.Build(entityManager, builder.ToMeshGeometry3D(), "duck" + Guid.NewGuid().ToString());
            //var arrowz = ArrowBuilder.Build(entityManager, Vector3.UnitZ, SharpDX.Color.Yellow);
            //var arrowx = ArrowBuilder.Build(entityManager, Vector3.UnitX, SharpDX.Color.Blue);
            //var arrowy = ArrowBuilder.Build(entityManager, Vector3.UnitY, SharpDX.Color.Green);
            //var entities = new[] { duck, arrowz, arrowx, arrowy };
            //var interactor = new EntityInteractor();
            //interactor.ManipulateInteractingTwoWays(entities);
            ////interactor.ManipulateInteractingTwoWays(duck, arrowx);
            ////interactor.ManipulateInteractingTwoWays(duck, arrowy);
            ////interactor.ManipulateInteracting(arrow, duck);

            //return new LoadedItem(entityManager, duck.Tag, arrowz.Tag, arrowx.Tag, arrowy.Tag);
        }


    }
}
