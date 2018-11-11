using D3DLab.Debugger.Presentation.SystemList;
using D3DLab.Debugger.Windows;
using D3DLab.Parser;
using D3DLab.Plugin.Contracts.Parsers;
using D3DLab.Plugins;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Visualization;
using D3DLab.Wpf.Engine.App;
using D3DLab.Wpf.Engine.App.Host;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace D3DLab {
    public interface IDropFiles {
        void Dropped(string[] files);
    }

    public sealed class MainWindowViewModel : IDropFiles, IFileLoader {

        class MoveToCenterWorldCommand : Debugger.BaseWPFCommand {
            private MainWindowViewModel main;

            public MoveToCenterWorldCommand(MainWindowViewModel main) {
                this.main = main;
            }

            public override void Execute(object parameter) {
                main.scene.Window.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ToCenterWorldCommand());
            }
        }
        class LoadCommand : ICommand {
            private MainWindowViewModel main;

            public LoadCommand(MainWindowViewModel mainWindowViewModel) {
                this.main = mainWindowViewModel;
            }

            public event EventHandler CanExecuteChanged = (s, r) => { };


            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                var file = this.GetType().Assembly.GetManifestResourceStream("D3DLab.Resources.ducky.obj");

                main.plugins.Import();
                var bl = new EntityBuilder(main.context.GetEntityManager());
                var tag = bl.Build(file, main.plugins.ParserPlugins.First());
                

               // main.items.Add(item);
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


        private SceneView scene;
        private readonly EngineNotificator notificator;
        ContextStateProcessor context;

        public VisualTreeviewerViewModel VisualTreeviewer { get; set; }
        public SystemViewPresenter SystemsView { get; set; }

        public ICommand LoadDuck { get; set; }
        public ICommand MoveToCenterWorld { get; }

        public ICollectionView Items { get; set; }
        readonly ObservableCollection<LoadedItem> items;
        readonly PluginImporter plugins;

        public MainWindowViewModel() {
            LoadDuck = new LoadCommand(this);
            MoveToCenterWorld = new MoveToCenterWorldCommand(this);
            VisualTreeviewer = new VisualTreeviewerViewModel();
            SystemsView = new SystemViewPresenter();

            items = new ObservableCollection<LoadedItem>();
            Items = CollectionViewSource.GetDefaultView(items);
            notificator = new EngineNotificator();

            notificator.Subscribe(new ViewportSubscriber(this));

            plugins = new PluginImporter();

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
            SystemsView.GameWindow = scene.Window;

            CoordinateSystemLinesGameObject.Build(context.GetEntityManager());

        }

        public void Dropped(string[] files) {
            plugins.Import();
            var win = new ChooseParseWindow();
            win.ViewModel.AddFiles(files);
            win.ViewModel.AddParsers(plugins.ParserPlugins);
            win.ViewModel.SetLoader(this);
            win.ShowDialog();
        }

        public void Load(FileInfo file, IFileParserPlugin parser) {
            var bl = new EntityBuilder(context.GetEntityManager());
            var tag = bl.Build(file, parser);
        }
    }

    public sealed class GenneralContextState : BaseContextState {
        public GenneralContextState(ContextStateProcessor processor, EngineNotificator notificator) : base(processor, new ManagerContainer(notificator)) {
        }
    }

    public sealed class ViewportSubscriber :
        IManagerChangeSubscriber<GraphicEntity>,
        IManagerChangeSubscriber<IComponentSystem>,
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

        public void Change(IComponentSystem sys) {
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

            return null;

            //var parser = new CncParser(new FileInfo(@"D:\Storage\trash\ncam\6848-Straumann_Tisue_4.8.cnc.obj"));
            //var points = parser.GetPaths()[0].Points.GetRange(0,100).ToArray();

            //points = new[] {
            //    Vector3.Zero - Vector3.UnitX *30, Vector3.Zero + Vector3.UnitX *30,
            //    Vector3.Zero- Vector3.UnitY *30, Vector3.Zero + Vector3.UnitY *30,
            //    Vector3.Zero- Vector3.UnitZ *30, Vector3.Zero + Vector3.UnitZ *30,
            //};

            //for(var i = 0; i < points.Length; ++i) {
            //    points[i] = Vector3.Transform(points[i], Matrix4x4.CreateTranslation(Vector3.UnitZ * 20));
            //}

            //var b = new LineBuilder();
            //b.Build(points);

            //var id = entityManager.BuildLineEntity(points);

            //var id = entityManager.BuildArrow(new ArrowData {
            //    axis = Vector3.UnitZ,
            //    center = Vector3.Zero,
            //    lenght = 20,
            //    radius = 10,
            //});

            //return new LoadedItem(entityManager, id);

            //duck
            /*
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



            var duckTag = entityManager.BuildGroupMeshElement(res.Select(x => x.Geometry));

            // duckTag = entityManager.BuildCoordinateSystemLinesEntity();

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
            */
            //return new LoadedItem(entityManager, duck.Tag, arrowz.Tag, arrowx.Tag, arrowy.Tag);
        }

        public class LineBuilder {
            private readonly List<Vector3> positions;
            private readonly List<int> lineListIndices;

            public LineBuilder() {
                positions = new List<Vector3>();
                lineListIndices = new List<int>();
            }

            public void Build(IEnumerable<Vector3> points, bool closed = false) {
                var first = positions.Count;
                positions.AddRange(points);
                var lineCount = positions.Count - first - 1;

                for (var i = 0; i < lineCount; i++) {
                    lineListIndices.Add(first + i);
                    lineListIndices.Add(first + i + 1);
                }

                if (closed) {
                    lineListIndices.Add(positions.Count - 1);
                    lineListIndices.Add(first);
                }
            }

        }

    }

   
}
